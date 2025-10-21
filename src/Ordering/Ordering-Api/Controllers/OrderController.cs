using BuildingBlocks.Controllers;
using Microsoft.AspNetCore.Mvc;
using Ordering.Components.DTO;
using Ordering.Components.ServiceContracts;

namespace Ordering.Api.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrderController(
            IOrderService orderService)
        {
            _orderService = orderService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await _orderService.CreateOrder(new OrderDto());

            return Ok();
        }
    }
}