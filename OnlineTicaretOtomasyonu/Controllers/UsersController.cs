using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITenantProvider _tenantProvider;
        private readonly IMapper _mapper;

        public UsersController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ITenantProvider tenantProvider,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tenantProvider = tenantProvider;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            IQueryable<AppUser> usersQuery;

            if (User.IsInRole("SuperAdmin"))
            {
                // SuperAdmin can see all users
                usersQuery = _userManager.Users;
            }
            else if (User.IsInRole("TenantAdmin") && currentUser.TenantId.HasValue)
            {
                // TenantAdmin can only see users from their tenant
                usersQuery = _userManager.Users.Where(u => u.TenantId == currentUser.TenantId);
            }
            else
            {
                return Forbid();
            }
            
            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                usersQuery = usersQuery.Where(u => 
                    u.UserName.Contains(search) || 
                    u.Email.Contains(search) || 
                    u.FirstName.Contains(search) || 
                    u.LastName.Contains(search));
            }
            
            // Count total before pagination
            var totalCount = await usersQuery.CountAsync();

            // Apply pagination
            var users = await usersQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var userViewModel = _mapper.Map<UserViewModel>(user);
                userViewModel.Roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(userViewModel);
            }
            
            // Create paged result
            var pagedResult = PagedResultViewModel<UserViewModel>.Create(
                userViewModels, 
                totalCount, 
                pageNumber, 
                pageSize);

            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> GetUser(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check permissions
            if (User.IsInRole("TenantAdmin") && 
                (!currentUser.TenantId.HasValue || user.TenantId != currentUser.TenantId))
            {
                return Forbid();
            }

            var userViewModel = _mapper.Map<UserViewModel>(user);
            userViewModel.Roles = await _userManager.GetRolesAsync(user);

            return Ok(userViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            // Check permissions
            if (User.IsInRole("TenantAdmin"))
            {
                if (!currentUser.TenantId.HasValue)
                {
                    return BadRequest("Tenant bilgisi bulunamadı");
                }

                // TenantAdmin can only create users for their tenant
                model.TenantId = currentUser.TenantId;

                // TenantAdmin cannot create admin users
                if (model.Role == "SuperAdmin" || model.Role == "TenantAdmin")
                {
                    return Forbid();
                }
            }

            // Check if the email is already in use
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return BadRequest("Bu e-posta adresi zaten kullanılıyor");
            }

            var user = new AppUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                TenantId = model.TenantId,
                TenantRole = model.Role,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Add to role
            await _userManager.AddToRoleAsync(user, model.Role);

            // Return the created user
            var userViewModel = _mapper.Map<UserViewModel>(user);
            userViewModel.Roles = await _userManager.GetRolesAsync(user);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, userViewModel);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check permissions
            if (User.IsInRole("TenantAdmin") &&
                (!currentUser.TenantId.HasValue || user.TenantId != currentUser.TenantId))
            {
                return Forbid();
            }

            // Update user properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            user.ProfilePictureUrl = model.ProfilePictureUrl;

            // Only SuperAdmin can change tenant
            if (User.IsInRole("SuperAdmin") && model.TenantId != user.TenantId)
            {
                user.TenantId = model.TenantId;
            }

            // Update user
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return BadRequest(updateResult.Errors);
            }

            // Update roles if specified
            if (model.Roles != null && model.Roles.Any())
            {
                // Only SuperAdmin can assign SuperAdmin role
                if (model.Roles.Contains("SuperAdmin") && !User.IsInRole("SuperAdmin"))
                {
                    return Forbid();
                }

                // Only SuperAdmin or TenantAdmin can assign TenantAdmin role
                if (model.Roles.Contains("TenantAdmin") && 
                    !User.IsInRole("SuperAdmin") && 
                    !(User.IsInRole("TenantAdmin") && currentUser.Id == user.Id))
                {
                    return Forbid();
                }

                // Get current roles
                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove roles that are not in the new role list
                var rolesToRemove = currentRoles.Except(model.Roles).ToArray();
                if (rolesToRemove.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                }

                // Add roles that are not in the current role list
                var rolesToAdd = model.Roles.Except(currentRoles).ToArray();
                if (rolesToAdd.Any())
                {
                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                }
            }

            // Return the updated user
            var userViewModel = _mapper.Map<UserViewModel>(user);
            userViewModel.Roles = await _userManager.GetRolesAsync(user);

            return Ok(userViewModel);
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // Check current password
            var checkPassword = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!checkPassword)
            {
                return BadRequest("Mevcut şifre yanlış");
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "Şifre başarıyla değiştirildi" });
        }

        [HttpGet("roles")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public async Task<IActionResult> GetRoles()
        {
            if (User.IsInRole("SuperAdmin"))
            {
                // SuperAdmin can see all roles
                var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return Ok(roles);
            }
            else if (User.IsInRole("TenantAdmin"))
            {
                // TenantAdmin can only see tenant roles
                var roles = new[] { "TenantManager", "TenantStaff" };
                return Ok(roles);
            }

            return Forbid();
        }
    }
} 