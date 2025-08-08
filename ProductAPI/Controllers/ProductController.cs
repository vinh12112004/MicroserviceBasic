using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductAPI.Implementation;
using ProductAPI.Models;

namespace ProductAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductDbContext _productDbContext;
        private IDbContextTransaction _transaction;

        public ProductController(ProductDbContext productDbContext)
        {
            _productDbContext = productDbContext;
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            try
            {
                await _productDbContext.Products.AddAsync(product);
                return StatusCode(StatusCodes.Status201Created, product);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }

        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await _productDbContext.Products.ToListAsync();
                return StatusCode(StatusCodes.Status200OK, list);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var product = await _productDbContext.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }
        [HttpPost("saga-prepare-product/{id:int}/{quantity:int}/{token}")]
        public async Task<IActionResult> SagaReserveProduct(int id, int quantity, CancellationToken token)
        {
            var transaction = await _productDbContext.Database.BeginTransactionAsync();

            try
            {
                var product = await _productDbContext.Products
                    .FindAsync(new object?[] { id }, cancellationToken: token);
                if (product == null)
                {
                    return NotFound($"Inventory with ID {id} not found.");
                }
                product.Quantity -= quantity;
                if (product.Quantity < 0)
                {
                    return BadRequest("Không đủ số lượng sản phẩm.");
                }
                await _productDbContext.SaveChangesAsync(token);
                await transaction.CommitAsync(token);
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(token);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("saga-compensate-product/{id:int}/{quantity:int}/{token}")]
        public async Task<IActionResult> SagaCompensateProduct(int id, int quantity, CancellationToken token)
        {
            var transaction = await _productDbContext.Database.BeginTransactionAsync();

            try
            {
                var product = await _productDbContext.Products
                    .FindAsync(new object?[] { id }, cancellationToken: token);
                if (product == null)
                {
                    return NotFound($"Inventory with ID {id} not found.");
                }
                product.Quantity += quantity;
                await _productDbContext.SaveChangesAsync(token);
                await transaction.CommitAsync(token);
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(token);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}