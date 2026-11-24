using CareerFlow.Web.Models;
using Microsoft.AspNetCore.Mvc;
namespace CareerFlow.Web.Controllers;
public class MvcDemoController : Controller
{
 [HttpGet("/mvc-demo")]
 public IActionResult Index()
 {
 var vm = new MvcDemoViewModel();
 return View(vm);
 }
}