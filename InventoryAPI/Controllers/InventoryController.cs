using InventoryAPI.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Transactions;

namespace InventoryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryDbContext _inventoryDbContext;
        private IDbContextTransaction _transaction;
        public InventoryController(InventoryDbContext inventoryDbContext)
        {
            _inventoryDbContext = inventoryDbContext;
            // Initialize any required services or data here
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var list = await _inventoryDbContext.Inventories.ToListAsync();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("saga-reserve-inventory/{id:int}/{quantity:int}/{token}")]
        public async Task<IActionResult> SagaReserveInventory(int id, int quantity, CancellationToken token)
        {
            var transaction = await _inventoryDbContext.Database.BeginTransactionAsync();
            try
            {
                var inventory = await _inventoryDbContext.Inventories
                    .FindAsync(new object?[] { id }, cancellationToken: token);
                if (inventory == null)
                {
                    return NotFound($"Inventory with ID {id} not found.");
                }
                inventory.Quantity -= quantity;
                await _inventoryDbContext.SaveChangesAsync(token);
                await transaction.CommitAsync(token);
                return Ok();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(token);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("commit-inventory/{token}")]
        public async Task<IActionResult> RollBackInventory(CancellationToken token)
        {
            try
            {
                await _transaction.CommitAsync(token);
                return Ok("Transaction committed successfully.");
            }
            catch (Exception ex)
            {
                await _transaction.RollbackAsync(token);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
