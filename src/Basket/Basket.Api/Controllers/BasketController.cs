using Basket_Application.DTO;
using Basket_Application.Orders.Commands.CreateOrder;
using BuildingBlocks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers
{
    public class BasketController : BaseController
    {
        private readonly ISender _sender;

        public BasketController(ISender sender)
        {
            _sender = sender;
        }

        [HttpPost()]
        [Authorize]
        public async Task<IActionResult> AddItem([FromBody] BasketItemDto dto)
        {
            return Ok(new { message = "Item added to basket successfully." });
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var command = new CreateOrderCommand();

            var result = await _sender.Send(command);

            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            return Ok();
        }
    }
}