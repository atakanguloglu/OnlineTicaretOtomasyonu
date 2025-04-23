using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Security.Claims;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;  // ApplicationUser → AppUser
        private readonly ITenantService _tenantService;

        public RolesController(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,  // ApplicationUser → AppUser
            ITenantService tenantService)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tenantService = tenantService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles(
            [FromQuery] string search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);

            IQueryable<IdentityRole> rolesQuery;

            // SuperAdmin can see all roles
            if (userRoles.Contains("SuperAdmin"))
            {
                rolesQuery = _roleManager.Roles;
            }
            // TenantAdmin can see only tenant specific roles
            else if (userRoles.Contains("TenantAdmin"))
            {
                rolesQuery = _roleManager.Roles
                    .Where(r => r.Name != "SuperAdmin" && r.Name != "TenantAdmin");
            }
            else
            {
                return Forbid();
            }

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                rolesQuery = rolesQuery.Where(r => r.Name.Contains(search) || r.NormalizedName.Contains(search.ToUpper()));
            }

            // Get total count before pagination
            var totalCount = await rolesQuery.CountAsync();

            // Apply pagination
            var roles = await rolesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var roleViewModels = roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                NormalizedName = r.NormalizedName
            }).ToList();

            // Create paged result
            var pagedResult = PagedResultViewModel<RoleViewModel>.Create(
                roleViewModels,
                totalCount,
                pageNumber,
                pageSize);

            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // SuperAdmin can access all roles
            if (userRoles.Contains("SuperAdmin"))
            {
                var roleViewModel = new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    NormalizedName = role.NormalizedName
                };
                return Ok(roleViewModel);
            }
            // TenantAdmin cannot access SuperAdmin or TenantAdmin roles
            else if (userRoles.Contains("TenantAdmin") && role.Name != "SuperAdmin" && role.Name != "TenantAdmin")
            {
                var roleViewModel = new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    NormalizedName = role.NormalizedName
                };
                return Ok(roleViewModel);
            }

            return Forbid();
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if role already exists
            var roleExists = await _roleManager.RoleExistsAsync(model.Name);
            if (roleExists)
            {
                return BadRequest(new { error = "Role already exists" });
            }

            var role = new IdentityRole { Name = model.Name };
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                var roleViewModel = new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    NormalizedName = role.NormalizedName
                };
                return CreatedAtAction(nameof(GetRole), new { id = role.Id }, roleViewModel);
            }

            return BadRequest(result.Errors);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent renaming system roles
            if ((role.Name == "SuperAdmin" || role.Name == "TenantAdmin" || role.Name == "Customer")
                && role.Name != model.Name)
            {
                return BadRequest(new { error = "Cannot rename system roles" });
            }

            // Check if new role name already exists (but not this role)
            var existingRole = await _roleManager.FindByNameAsync(model.Name);
            if (existingRole != null && existingRole.Id != id)
            {
                return BadRequest(new { error = "Role name already taken" });
            }

            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                var roleViewModel = new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    NormalizedName = role.NormalizedName
                };
                return Ok(roleViewModel);
            }

            return BadRequest(result.Errors);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            // Prevent deleting system roles
            if (role.Name == "SuperAdmin" || role.Name == "TenantAdmin" || role.Name == "Customer")
            {
                return BadRequest(new { error = "Cannot delete system roles" });
            }

            // Check if any users are in this role
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                return BadRequest(new { error = $"Cannot delete role with users assigned. There are {usersInRole.Count} users in this role." });
            }

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("{roleId}/users")]
        public async Task<IActionResult> GetUsersInRole(
            string roleId,
            [FromQuery] string search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            var userRoles = await _userManager.GetRolesAsync(user);

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound();
            }

            // Get all users in role first
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            IEnumerable<AppUser> filteredUsers;  // ApplicationUser → AppUser

            // SuperAdmin can see all users in a role
            if (userRoles.Contains("SuperAdmin"))
            {
                filteredUsers = usersInRole;
            }
            // TenantAdmin can see users in tenant-specific roles, filtered by TenantId
            else if (userRoles.Contains("TenantAdmin") && role.Name != "SuperAdmin")
            {
                var tenantId = user.TenantId;
                filteredUsers = usersInRole.Where(u => u.TenantId == tenantId);
            }
            else
            {
                return Forbid();
            }

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                filteredUsers = filteredUsers.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search));
            }

            // Get total count
            var totalCount = filteredUsers.Count();

            // Apply pagination
            var pagedUsers = filteredUsers
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var userViewModels = pagedUsers.Select(u => new
            {
                u.Id,
                u.UserName,
                u.Email,
                u.TenantId
            });

            // Create paged result
            var pagedResult = new
            {
                Items = userViewModels,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                HasPrevious = pageNumber > 1,
                HasNext = pageNumber < (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(pagedResult);
        }
    }
}