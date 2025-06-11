using Microsoft.AspNetCore.Mvc;
using OrderApi.Application.DTOs;
using OrderApi.Application.Interfaces;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new order
        /// </summary>
        /// <param name="request">Order creation request</param>
        /// <returns>Created order information</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _orderService.CreateOrderAsync(request);

                return CreatedAtAction(
                    nameof(CreateOrder),
                    new { orderId = response.OrderId },
                    response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Conflict creating order: {Message}", ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Bad request creating order: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating order");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { error = "An unexpected error occurred while processing your request" });
            }
        }
    }
}
