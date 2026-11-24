using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
namespace CareerFlow.Web.Controllers;
[ApiController]
[Route("api/security")]
public class SecurityController(IAntiforgery antiforgery) : ControllerBase
{
 [HttpGet("csrf")]
 [IgnoreAntiforgeryToken]
 public IActionResult Csrf()
 {
 var tokens = antiforgery.GetAndStoreTokens(HttpContext);
 return Ok(new { token = tokens.RequestToken });
 }
}