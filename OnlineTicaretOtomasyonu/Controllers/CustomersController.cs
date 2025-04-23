using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineTicaretOtomasyonu.Data;
using OnlineTicaretOtomasyonu.Models;
using OnlineTicaretOtomasyonu.Services;
using OnlineTicaretOtomasyonu.ViewModels;

namespace OnlineTicaretOtomasyonu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CustomersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IMapper _mapper;

        public CustomersController(
            ApplicationDbContext context,
            ITenantProvider tenantProvider,
            IMapper mapper)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers(
            [FromQuery] string search = null, 
            [FromQuery] string customerType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;
            
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var query = _context.Customers
                .Where(c => c.TenantId == tenantId.Value && c.IsActive);

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.FirstName.Contains(search) || 
                                         c.LastName.Contains(search) || 
                                         c.Email.Contains(search) || 
                                         c.Phone.Contains(search) ||
                                         c.CompanyName.Contains(search));
            }

            // Apply customer type filter
            if (!string.IsNullOrEmpty(customerType))
            {
                query = query.Where(c => c.CustomerType == customerType);
            }

            // Count before pagination for total count
            var totalCount = await query.CountAsync();
            
            // Apply pagination
            var customers = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
                
            var customerViewModels = _mapper.Map<IEnumerable<CustomerViewModel>>(customers);
            
            // Create paged result
            var pagedResult = PagedResultViewModel<CustomerViewModel>.Create(
                customerViewModels, 
                totalCount, 
                pageNumber, 
                pageSize);
                
            return Ok(pagedResult);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (customer == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<CustomerViewModel>(customer));
        }

        [HttpPost]
        [Authorize(Roles = "TenantAdmin,TenantManager,TenantStaff")]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            // Check if email already exists for this tenant
            if (!string.IsNullOrEmpty(model.Email))
            {
                var emailExists = await _context.Customers
                    .AnyAsync(c => c.Email == model.Email && c.TenantId == tenantId.Value);

                if (emailExists)
                {
                    return BadRequest("Bu e-posta adresi ile kayıtlı bir müşteri zaten mevcut");
                }
            }

            var customer = new Customer
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                City = model.City,
                Country = model.Country,
                PostalCode = model.PostalCode,
                CompanyName = model.CompanyName,
                TaxNumber = model.TaxNumber,
                CustomerType = model.CustomerType,
                TenantId = tenantId.Value,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, _mapper.Map<CustomerViewModel>(customer));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "TenantAdmin,TenantManager,TenantStaff")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest("ID mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if email already exists for other customer
            if (!string.IsNullOrEmpty(model.Email) && customer.Email != model.Email)
            {
                var emailExists = await _context.Customers
                    .AnyAsync(c => c.Email == model.Email && c.TenantId == tenantId.Value && c.Id != id);

                if (emailExists)
                {
                    return BadRequest("Bu e-posta adresi ile kayıtlı başka bir müşteri zaten mevcut");
                }
            }

            customer.FirstName = model.FirstName;
            customer.LastName = model.LastName;
            customer.Email = model.Email;
            customer.Phone = model.Phone;
            customer.Address = model.Address;
            customer.City = model.City;
            customer.Country = model.Country;
            customer.PostalCode = model.PostalCode;
            customer.CompanyName = model.CompanyName;
            customer.TaxNumber = model.TaxNumber;
            customer.CustomerType = model.CustomerType;
            customer.IsActive = model.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(_mapper.Map<CustomerViewModel>(customer));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "TenantAdmin,TenantManager")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            if (!tenantId.HasValue)
            {
                return BadRequest("Tenant bilgisi bulunamadı");
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenantId.Value);

            if (customer == null)
            {
                return NotFound();
            }

            // Check if customer has orders
            var hasOrders = await _context.Orders
                .AnyAsync(o => o.CustomerId == id);

            if (hasOrders)
            {
                // Soft delete instead of hard delete
                customer.IsActive = false;
                customer.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Bu müşteriye ait siparişler bulunduğu için pasif duruma alındı" });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("types")]
        public IActionResult GetCustomerTypes()
        {
            var customerTypes = new[] { "Individual", "Business" };
            return Ok(customerTypes);
        }

        private bool CustomerExists(int id)
        {
            var tenantId = _tenantProvider.GetTenantId();
            return _context.Customers.Any(c => c.Id == id && c.TenantId == tenantId.Value);
        }
    }
} 