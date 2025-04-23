using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineTicaretOtomasyonu.ViewModels.Pages;
using OnlineTicaretOtomasyonu.ViewModels.Pages.Dashboard;
using OnlineTicaretOtomasyonu.ViewModels.Pages.Customers;
using OnlineTicaretOtomasyonu.ViewModels.Pages.Orders;
using OnlineTicaretOtomasyonu.ViewModels.Pages.Users;
using OnlineTicaretOtomasyonu.ViewModels.Pages.Roles;
using System.Collections.Generic;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagesController : ControllerBase
    {
        [HttpGet("navigation")]
        public IActionResult GetNavigation()
        {
            var navigationItems = new List<object>
            {
                new 
                {
                    Title = "Dashboard",
                    Icon = "home",
                    Url = "/dashboard",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager" }
                },
                new 
                {
                    Title = "Müşteriler",
                    Icon = "users",
                    Url = "/customers",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager", "TenantStaff" }
                },
                new 
                {
                    Title = "Siparişler",
                    Icon = "shopping-cart",
                    Url = "/orders",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager", "TenantStaff" }
                },
                new 
                {
                    Title = "Ürünler",
                    Icon = "box",
                    Url = "/products",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager", "TenantStaff" }
                },
                new 
                {
                    Title = "Kategoriler",
                    Icon = "grid",
                    Url = "/categories",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager" }
                },
                new 
                {
                    Title = "Raporlar",
                    Icon = "bar-chart-2",
                    Url = "/reports",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin", "TenantManager" }
                },
                new 
                {
                    Title = "Kullanıcılar",
                    Icon = "user",
                    Url = "/users",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin" }
                },
                new 
                {
                    Title = "Roller",
                    Icon = "shield",
                    Url = "/roles",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin" }
                },
                new 
                {
                    Title = "Ayarlar",
                    Icon = "settings",
                    Url = "/settings",
                    Roles = new List<string> { "SuperAdmin", "TenantAdmin" }
                }
            };

            return Ok(navigationItems);
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,TenantManager")]
        public IActionResult GetDashboardPage()
        {
            return Ok(DashboardPage.GetStructure());
        }

        [HttpGet("customers")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,TenantManager,TenantStaff")]
        public IActionResult GetCustomersPage()
        {
            return Ok(CustomersListPage.GetStructure());
        }

        [HttpGet("orders")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin,TenantManager,TenantStaff")]
        public IActionResult GetOrdersPage()
        {
            return Ok(OrdersListPage.GetStructure());
        }

        [HttpGet("users")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public IActionResult GetUsersPage()
        {
            return Ok(UsersListPage.GetStructure());
        }

        [HttpGet("roles")]
        [Authorize(Roles = "SuperAdmin,TenantAdmin")]
        public IActionResult GetRolesPage()
        {
            return Ok(RolesListPage.GetStructure());
        }
    }
} 