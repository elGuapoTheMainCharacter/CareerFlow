using CareerFlow.Web.Models;
namespace CareerFlow.Web.Dtos;
public class ApplicationQuery
{
 public string? Search { get; set; }
 public ApplicationStatus? Status { get; set; }
 public Guid? CompanyId { get; set; }
 public string Sort { get; set; } = "updated-desc";
 public int Page { get; set; } = 1;
 public int PageSize { get; set; } = 10;
}