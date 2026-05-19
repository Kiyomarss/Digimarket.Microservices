using Basket_Application.Orders.Commands.CreateOrder;
using BuildingBlocks.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Basket.Api.Controllers
{
    [Route("/Basket")]
    public class BasketController : BaseController
    {
        private readonly ISender _sender;

        public BasketController(ISender sender)
        {
            _sender = sender;
        }

        /*[HttpPost()]
        [Authorize]
        public Task<IActionResult> AddItem([FromBody] BasketItemDto dto)
        {
            return Task.FromResult<IActionResult>(Ok(new { message = "Item added to basket successfully." }));
        }*/
        
        [HttpPost("/Basket/Checkout")]
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var command = new CreateOrderCommand();

            var result = await _sender.Send(command);

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Test()
        {
            var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
            var rawToken = authHeader.StartsWith("Bearer ")
                               ? authHeader.Substring("Bearer ".Length).Trim()
                               : authHeader;

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(rawToken))
            {
                return BadRequest(new
                {
                    message = "Token is not a valid JWT.",
                    authorizationHeader = authHeader
                });
            }

            var jwtToken = handler.ReadJwtToken(rawToken);

            var claims = jwtToken.Claims.Select(c => new
            {
                c.Type,
                c.Value
            });

            return Ok(new
            {
                authorizationHeader = authHeader,
                rawToken = rawToken,
                issuer = jwtToken.Issuer,
                audiences = jwtToken.Audiences,
                validFrom = jwtToken.ValidFrom,
                validTo = jwtToken.ValidTo,
                claims = claims
            });
        }
    }
}