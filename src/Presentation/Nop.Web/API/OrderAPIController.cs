using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Configuration;
using Nop.Services.Orders;
using Nop.Web.API.Dtos;
using Nop.Web.Framework.Controllers;

namespace Nop.Web.API;

[Route("api/orderApi/")]
[ApiController]
public class OrderAPIController : BasePluginController
{
    #region Fields
    protected readonly AppSettings _appSettings;
    protected readonly IOrderService _orderService;
    #endregion

    #region Ctor
    public OrderAPIController(AppSettings appSettings,
        IOrderService orderService) 
    {
        _appSettings = appSettings;
        _orderService = orderService;
    }
    #endregion

    #region API Endpoints

    [HttpPost("GenerateToken")]
    public IActionResult GenerateToken([FromBody]AuthRequestModel model) 
    {
        // You should validate username/password from DB or NopCommerce services
        if (model == null || string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password))
            return BadRequest("Username and password are required.");

        // Sample hardcoded check (replace with DB/Nop validation)
        if (model.UserName != "admin" || model.Password != "admin123")
            return Unauthorized("Invalid credentials");

        var jwtSettings = _appSettings.Configuration["Jwt"];
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value<string>("Key")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, model.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value<string>("Issuer"),
            audience: jwtSettings.Value<string>("Audience"),
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings.Value<int>("ExpiresInMinutes"))),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new { Token = tokenString });
    }

    [Authorize]
    [HttpGet("GetDetails/{id}")]
    public async Task<IActionResult> GetDetails(int id)
    {
        if (id == 0)
            return NotFound("Invalid order id.");

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return NotFound("Order not found.");

        return Json(new { 
            OrderId = order.Id,
            TotalAmount = order.OrderTotal,
            OrderDate = order.CreatedOnUtc.ToShortDateString(),
        });
        
    }
    #endregion
}
