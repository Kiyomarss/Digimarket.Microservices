using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Api.Contracts;
using Ordering.Application.Orders.Queries;

namespace Ordering.Api.Controllers
{
    [Route(ApiEndpoints.Orders.Base)]
    public class OrderController : BaseController
    {
        private readonly ISender _sender;

        public OrderController(ISender sender)
        {
            _sender = sender;
        }
        
        [HttpGet(ApiEndpoints.Orders.GetCurrentUserOrders)]
        public async Task<IActionResult> GetCurrentUserOrders([FromQuery] [Required] string state)
        {
            var command = new GetCurrentUserOrdersQuery(state);

            var result = await _sender.Send(command);

            return Ok(result);
        }
    }
}