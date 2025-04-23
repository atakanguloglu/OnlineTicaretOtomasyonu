using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ITenantService _tenantService;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            JwtTokenService jwtTokenService,
            ITenantService tenantService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _tenantService = tenantService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new { message = "This account has been deactivated" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Check if user belongs to a tenant and if the tenant is active
            if (user.TenantId.HasValue)
            {
                var isTenantActive = await _tenantService.IsTenantActiveAsync(user.TenantId.Value);
                if (!isTenantActive)
                {
                    return Unauthorized(new { message = "Your organization is currently inactive" });
                }
            }

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate JWT token
            var token = _jwtTokenService.GenerateJwtToken(user, roles, user.TenantId);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    tenantId = user.TenantId,
                    roles
                }
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Only allow SuperAdmin to create tenant admins
            if (User.IsInRole("SuperAdmin") && model.Role != "TenantAdmin")
            {
                return BadRequest(new { message = "SuperAdmin can only create TenantAdmin accounts" });
            }

            // Verify tenant exists and is active
            if (model.TenantId.HasValue)
            {
                var tenant = await _tenantService.GetTenantByIdAsync(model.TenantId.Value);
                if (tenant == null)
                {
                    return BadRequest(new { message = "Invalid tenant specified" });
                }

                if (!tenant.IsActive)
                {
                    return BadRequest(new { message = "The specified tenant is inactive" });
                }
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

            // Assign role
            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Get tenant info if available
            Tenant tenant = null;
            if (user.TenantId.HasValue)
            {
                tenant = await _tenantService.GetTenantByIdAsync(user.TenantId.Value);
            }

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                tenantId = user.TenantId,
                tenantName = tenant?.Name,
                roles
            });
        }
    }
} 