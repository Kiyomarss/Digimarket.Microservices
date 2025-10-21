using Basket.Core.DTO;
using Basket.Core.ServiceContracts;
using BuildingBlocks.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Basket.Api.Controllers
{
    public class BasketController : BaseController
    {
        private readonly IBasketUpdaterService _basketUpdaterService;

        public BasketController(
            IBasketUpdaterService basketUpdaterService)
        {
            _basketUpdaterService = basketUpdaterService;
        }

        [HttpPost()]
        public async Task<IActionResult> AddItem([FromBody] BasketItemDto dto)
        {
            await _basketUpdaterService.AddItem(dto.CatalogId, dto.Quantity);
            return Ok(new { message = "Item added to basket successfully." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> RemoveItem(Guid id)
        {
            await _basketUpdaterService.RemoveItem(id);
            return Ok();
        }
    }
}