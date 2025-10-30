using Basket_Application.DTO;
using Basket_Application.Orders.Commands.CreateOrder;
using Basket.Domain.ServiceContracts;
using BuildingBlocks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers
{
    public class BasketController : BaseController
    {
        private readonly IBasketUpdaterService _basketUpdaterService;
        private readonly ISender _sender;

        public BasketController(
            IBasketUpdaterService basketUpdaterService, ISender sender)
        {
            _basketUpdaterService = basketUpdaterService;
            _sender = sender;
        }

        [HttpPost()]
        public async Task<IActionResult> AddItem([FromBody] BasketItemDto dto)
        {
            await _basketUpdaterService.AddItem(dto.CatalogId, dto.Quantity);
            return Ok(new { message = "Item added to basket successfully." });
        }
        
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var command = new CreateOrderCommand();

            var result = await _sender.Send(command);

            return Ok();
        }


        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            await _basketUpdaterService.RemoveItem(id);
            return Ok();
        }
    }
}