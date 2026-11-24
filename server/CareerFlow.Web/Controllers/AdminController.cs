using CareerFlow.Web.Data;
using CareerFlow.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CareerFlow.Web.Controllers;
[Authorize(Roles = "Admin")]
[Route("admin")]
public class AdminController(AppDbContext db) : Controller
{
 [HttpGet("")]
 public async Task<IActionResult> Index()
 {
 var vm = new AdminDashboardViewModel(
 await db.Users.CountAsync(), await db.Companies.CountAsync(),
 await db.JobOpportunities.CountAsync(), await db.JobApplications.CountAsync());
 return View(vm);
 }
}