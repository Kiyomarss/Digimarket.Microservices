using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ordering.Api.Contracts;
using Ordering.Application.Orders.Commands.CancelledAfterPayment;
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
        
        [Authorize]
        [HttpGet(ApiEndpoints.Orders.GetCurrentUserOrders)]
        public async Task<IActionResult> GetCurrentUserOrders([FromQuery] [Required] string state)
        {
            var command = new GetCurrentUserOrdersQuery(state);

            var result = await _sender.Send(command);

            return Ok(result);
        }
        
        [Authorize]
        [HttpPost(ApiEndpoints.Orders.CancelledAfterPayment)]
        public async Task<IActionResult> CancelledAfterPayment([FromQuery] [Required] Guid id)
        {
            var command = new CancelledAfterPaymentCommand(id);

            await _sender.Send(command);

            return Ok();
        }
    }
}