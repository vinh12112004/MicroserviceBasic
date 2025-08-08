using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Models;
using System.Threading.Tasks;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private HttpClient _httpClientProduct;
        private HttpClient _httpClientInventory;

        public OrderController(IHttpClientFactory httpClientFactory)
        {
            _httpClientProduct = httpClientFactory.CreateClient("ProductAPI");
            _httpClientInventory = httpClientFactory.CreateClient("InventoryAPI");
            // Initialize any required services or data here
        }
        [HttpPost("create")]
        public IActionResult CreateOrder(Order order)
        {
            try
            {
                MessageBroker messagebroker = new MessageBroker();
                messagebroker.SendMessage("HIII");
                // Giả lập việc tạo đơn hàng thành công
                // Trong thực tế, bạn sẽ cần lưu trữ đơn hàng vào cơ sở dữ liệu hoặc thực hiện các hành động khác.
                return StatusCode(StatusCodes.Status201Created, "Order created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("saga-create")]
        public async Task<IActionResult> SagaCreateOrder(Order order, CancellationToken token)
        {
            try
            {
                // Bước 1: Gọi ProductAPI để chuẩn bị (giảm) số lượng sản phẩm
                var productResponse = await _httpClientProduct
                    .PostAsync($"api/product/saga-prepare-product/{order.Product.Id}/{order.Product.Quantity}/{token}",
                    new StringContent(string.Empty));

                // Nếu bước 1 thất bại, dừng Saga ngay lập tức.
                if (!productResponse.IsSuccessStatusCode)
                {
                    return StatusCode(StatusCodes.Status501NotImplemented, "Product success is not ok.");
                }

                // Bước 2: Gọi InventoryAPI để đặt trước hàng trong kho
                var inventoryResponse = await _httpClientInventory
                    .PostAsync($"api/inventory/saga-reserve-inventory/{order.Inventory.InventoryID}/{order.Inventory.Quantity}/{token}",
                    new StringContent(string.Empty));

                // Nếu bước 2 thành công, toàn bộ Saga hoàn tất.
                if (inventoryResponse.IsSuccessStatusCode)
                {
                    return StatusCode(StatusCodes.Status501NotImplemented, "order is created.");
                }
                // Nếu bước 2 thất bại, thực hiện hành động bù trừ cho bước 1.
                if (await CompensateProduct(order.Product.Id, order.Product.Quantity, token: token))
                {
                    return StatusCode(StatusCodes.Status501NotImplemented, "Product is compensation because Inventory failed to process.");
                }
                return StatusCode(StatusCodes.Status501NotImplemented, "order is created.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }


        }
        private async Task<bool> CompensateProduct(int id, int quantity, CancellationToken token)
        {
            var response = await _httpClientProduct
                .PostAsync($"api/product/compensate-product/{id}/{quantity}/{token}",
                new StringContent(string.Empty));
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }

    }
}