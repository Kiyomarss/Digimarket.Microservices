using System.ComponentModel.DataAnnotations;
using BuildingBlocks.Controllers;
using Identity.Api.Contracts.Auth;
using Identity.Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
                                          request.Email,
                                          request.Password);

        var result = await _sender.Send(command);

        return Ok(result);
    }
}