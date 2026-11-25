using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public record ResumeFile(string FileName, string ContentType, byte[] Content);

public class ResumeStorageService(AppDbContext db, ICurrentUserService currentUser)
{
    private const long MaxBytes = 2 * 1024 * 1024;
    private const int MaxResumesPerUser = 3;

    public async Task<ResumeDto> SaveAsync(IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxBytes)
            throw new ArgumentException("File must be between 1 byte and 2 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf" && ext != ".docx")
            throw new ArgumentException("Only PDF and DOCX files are allowed.");

        var count = await db.ResumeDocuments.CountAsync(d => d.OwnerId == currentUser.UserId);
        if (count >= MaxResumesPerUser)
            throw new ArgumentException($"You can store up to {MaxResumesPerUser} CVs. Delete one first.");

        await using var input = file.OpenReadStream();
        using var memory = new MemoryStream();
        await input.CopyToAsync(memory);
        var bytes = memory.ToArray();

        if (bytes.Length == 0 || bytes.Length > MaxBytes)
            throw new ArgumentException("File must be between 1 byte and 2 MB.");

        // Do not trust the extension alone: check the first bytes of the file.
        var looksRight = ext == ".pdf"
            ? bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46
            : bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B;
        if (!looksRight)
            throw new ArgumentException("The file content does not match its extension.");

        var originalName = Path.GetFileName(file.FileName);
        if (originalName.Length > 255) originalName = originalName[^255..];

        var doc = new ResumeDocument
        {
            OwnerId = currentUser.UserId,
            OriginalFileName = originalName,
            // Set by us, never copied from the browser.
            ContentType = ext == ".pdf"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            SizeBytes = bytes.Length,
            Content = bytes
        };
        db.ResumeDocuments.Add(doc);
        await db.SaveChangesAsync();

        return new ResumeDto(doc.Id, doc.OriginalFileName, doc.SizeBytes, doc.UploadedAtUtc);
    }

    public async Task<IReadOnlyList<ResumeDto>> ListAsync() =>
        await db.ResumeDocuments.AsNoTracking()
            .Where(d => d.OwnerId == currentUser.UserId)
            .OrderByDescending(d => d.UploadedAtUtc)
            .Select(d => new ResumeDto(d.Id, d.OriginalFileName, d.SizeBytes, d.UploadedAtUtc))
            .ToListAsync();

    // A seeker downloading their own CV.
    public async Task<ResumeFile?> GetOwnAsync(Guid id) =>
        await db.ResumeDocuments.AsNoTracking()
            .Where(d => d.Id == id && d.OwnerId == currentUser.UserId)
            .Select(d => new ResumeFile(d.OriginalFileName, d.ContentType, d.Content))
            .SingleOrDefaultAsync();

    // An employer downloading the CV attached to an application on THEIR job.
    public async Task<ResumeFile?> GetForEmployerAsync(Guid applicationId) =>
        await db.JobApplications.AsNoTracking()
            .Where(a => a.Id == applicationId
                && a.JobOpportunity!.OwnerId == currentUser.UserId
                && a.ResumeDocument != null)
            .Select(a => new ResumeFile(
                a.ResumeDocument!.OriginalFileName, a.ResumeDocument.ContentType, a.ResumeDocument.Content))
            .SingleOrDefaultAsync();

    // An employer downloading the latest CV of a person who opted in to be found.
    public async Task<ResumeFile?> GetLatestForTalentAsync(string seekerId) =>
        await db.ResumeDocuments.AsNoTracking()
            .Where(r => r.OwnerId == seekerId && r.Owner!.IsDiscoverable)
            .OrderByDescending(r => r.UploadedAtUtc)
            .Select(r => new ResumeFile(r.OriginalFileName, r.ContentType, r.Content))
            .FirstOrDefaultAsync();

    public async Task<bool> DeleteAsync(Guid id)
    {
        var mine = db.ResumeDocuments.Where(d => d.Id == id && d.OwnerId == currentUser.UserId);
        if (!await mine.AnyAsync()) return false;

        var inUse = await db.JobApplications.AnyAsync(a => a.ResumeDocumentId == id);
        if (inUse)
            throw new InvalidOperationException("This CV was sent with an application, so it cannot be deleted.");

        await mine.ExecuteDeleteAsync();
        return true;
    }
}
