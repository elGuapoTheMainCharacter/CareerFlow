namespace CareerFlow.Web.Models;
public class MvcDemoViewModel
{
 public string Title { get; set; } = "CareerFlow MVC demo";
 public DateTime ServerTime { get; set; } = DateTime.Now;
 public string Message { get; set; } = "The controller created this model and passed it to a Razor view.";
}