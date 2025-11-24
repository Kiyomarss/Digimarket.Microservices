using BuildingBlocks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Core.Orders.Queries;

namespace Ordering.Api.Controllers
{
    public class OrderController : BaseController
    {
        private readonly ISender _sender;

        public OrderController(ISender sender)
        {
            _sender = sender;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetCurrentUserOrders(string state)
        {
            var command = new GetCurrentUserOrdersQuery(state);

            var result = await _sender.Send(command);

            return Ok(result);
        }
    }
}