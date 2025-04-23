using Microsoft.AspNetCore.Identity;
using OnlineTicaretOtomasyonu.Models;

namespace OnlineTicaretOtomasyonu.Data
{
    public static class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles if they don't exist
            string[] roles = { "SuperAdmin", "TenantAdmin", "TenantManager", "TenantStaff" };
            
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Create super admin if it doesn't exist
            var superAdminEmail = "admin@onlineticaret.com";
            var superAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            
            if (superAdmin == null)
            {
                superAdmin = new AppUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Super",
                    LastName = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(superAdmin, "Admin123!");
                
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                }
            }

            // Create default tenant if none exist
            if (!context.Tenants.Any())
            {
                var defaultTenant = new Tenant
                {
                    Id = Guid.NewGuid(),
                    Name = "Demo Tenant",
                    Slug = "demo",
                    Description = "Default demo tenant for testing",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    ContactEmail = "contact@demotenant.com",
                    ContactPhone = "5551234567",
                    CompanyName = "Demo Company Ltd.",
                };

                context.Tenants.Add(defaultTenant);
                await context.SaveChangesAsync();

                // Create tenant admin
                var tenantAdminEmail = "admin@demotenant.com";
                var tenantAdmin = new AppUser
                {
                    UserName = tenantAdminEmail,
                    Email = tenantAdminEmail,
                    EmailConfirmed = true,
                    FirstName = "Tenant",
                    LastName = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    TenantId = defaultTenant.Id,
                    TenantRole = "TenantAdmin"
                };

                var tenantAdminResult = await userManager.CreateAsync(tenantAdmin, "Tenant123!");
                
                if (tenantAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(tenantAdmin, "TenantAdmin");
                }

                // Create default categories for demo tenant
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Electronics",
                        Description = "Electronic devices and gadgets",
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "Clothing",
                        Description = "Apparel and fashion items",
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "Home & Kitchen",
                        Description = "Items for home and kitchen use",
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Create some sample products
                var random = new Random();
                var electronicsCategory = categories[0];
                var clothingCategory = categories[1];
                var homeCategory = categories[2];

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Smartphone X",
                        Description = "Latest smartphone with advanced features",
                        Price = 999.99m,
                        StockQuantity = 50,
                        SKU = "PHONE-X1",
                        Barcode = "123456789012",
                        CategoryId = electronicsCategory.Id,
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        TaxRate = 18.0m,
                        CostPrice = 750.0m
                    },
                    new Product
                    {
                        Name = "Laptop Pro",
                        Description = "High-performance laptop for professionals",
                        Price = 1499.99m,
                        StockQuantity = 30,
                        SKU = "LAPTOP-P1",
                        Barcode = "123456789013",
                        CategoryId = electronicsCategory.Id,
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        TaxRate = 18.0m,
                        CostPrice = 1200.0m
                    },
                    new Product
                    {
                        Name = "T-Shirt Classic",
                        Description = "Cotton t-shirt with classic fit",
                        Price = 29.99m,
                        StockQuantity = 100,
                        SKU = "TSHIRT-C1",
                        Barcode = "123456789014",
                        CategoryId = clothingCategory.Id,
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        TaxRate = 8.0m,
                        CostPrice = 15.0m
                    },
                    new Product
                    {
                        Name = "Kitchen Mixer",
                        Description = "Professional kitchen mixer for all your baking needs",
                        Price = 249.99m,
                        StockQuantity = 20,
                        SKU = "MIXER-K1",
                        Barcode = "123456789015",
                        CategoryId = homeCategory.Id,
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                        TaxRate = 18.0m,
                        CostPrice = 180.0m
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                // Create sample customers
                var customers = new List<Customer>
                {
                    new Customer
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@example.com",
                        Phone = "5551234567",
                        Address = "123 Main St",
                        City = "Istanbul",
                        Country = "Turkey",
                        PostalCode = "34000",
                        CustomerType = "Individual",
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Customer
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane.smith@example.com",
                        Phone = "5559876543",
                        Address = "456 Oak Ave",
                        City = "Ankara",
                        Country = "Turkey",
                        PostalCode = "06000",
                        CustomerType = "Individual",
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    },
                    new Customer
                    {
                        FirstName = "ABC",
                        LastName = "Company",
                        Email = "contact@abccompany.com",
                        Phone = "5551112233",
                        Address = "789 Business Rd",
                        City = "Izmir",
                        Country = "Turkey",
                        PostalCode = "35000",
                        CompanyName = "ABC Company Ltd.",
                        TaxNumber = "1234567890",
                        CustomerType = "Business",
                        TenantId = defaultTenant.Id,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    }
                };

                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
            }
        }
    }
} 