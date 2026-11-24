namespace CareerFlow.Web.Dtos;

public record ResumeDto(Guid Id, string OriginalFileName, long SizeBytes, DateTime UploadedAtUtc);