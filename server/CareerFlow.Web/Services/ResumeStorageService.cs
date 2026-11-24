using CareerFlow.Web.Data;
using CareerFlow.Web.Dtos;
using CareerFlow.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerFlow.Web.Services;

public class ResumeStorageService(AppDbContext db, ICurrentUserService currentUser, IWebHostEnvironment env)
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private const int MaxResumesPerUser = 5;

    private string UploadRoot => Path.Combine(env.ContentRootPath, "Uploads");

    public async Task<ResumeDto> SaveAsync(IFormFile file)
    {
        if (file.Length <= 0 || file.Length > MaxBytes)
            throw new ArgumentException("File must be between 1 byte and 5 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".pdf" && ext != ".docx")
            throw new ArgumentException("Only PDF and DOCX files are allowed.");

        var count = await db.ResumeDocuments.CountAsync(d => d.OwnerId == currentUser.UserId);
        if (count >= MaxResumesPerUser)
            throw new ArgumentException($"You can store up to {MaxResumesPerUser} CVs. Delete one first.");

        // Do not trust the extension alone: check the first bytes of the file.
        var header = new byte[4];
        int read;
        await using (var peek = file.OpenReadStream())
            read = await peek.ReadAsync(header.AsMemory(0, 4));

        var looksRight = ext == ".pdf"
            ? read == 4 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46
            : read >= 2 && header[0] == 0x50 && header[1] == 0x4B;
        if (!looksRight)
            throw new ArgumentException("The file content does not match its extension.");

        Directory.CreateDirectory(UploadRoot);
        var stored = $"{Guid.NewGuid():N}{ext}";
        await using (var stream = File.Create(Path.Combine(UploadRoot, stored)))
            await file.CopyToAsync(stream);

        var originalName = Path.GetFileName(file.FileName);
        if (originalName.Length > 255) originalName = originalName[^255..];

        var doc = new ResumeDocument
        {
            OwnerId = currentUser.UserId,
            OriginalFileName = originalName,
            StoredFileName = stored,
            // Set by us, never copied from the browser.
            ContentType = ext == ".pdf"
                ? "application/pdf"
                : "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            SizeBytes = file.Length
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
    public async Task<(ResumeDocument Meta, string Path)?> GetOwnAsync(Guid id)
    {
        var doc = await db.ResumeDocuments.AsNoTracking()
            .SingleOrDefaultAsync(d => d.Id == id && d.OwnerId == currentUser.UserId);
        return doc is null ? null : Locate(doc);
    }

    // An employer downloading the CV attached to an application on THEIR job.
    public async Task<(ResumeDocument Meta, string Path)?> GetForEmployerAsync(Guid applicationId)
    {
        var doc = await db.JobApplications.AsNoTracking()
            .Where(a => a.Id == applicationId
                && a.JobOpportunity!.OwnerId == currentUser.UserId
                && a.ResumeDocument != null)
            .Select(a => a.ResumeDocument!)
            .SingleOrDefaultAsync();
        return doc is null ? null : Locate(doc);
    }
    // An employer downloading the latest CV of a person who opted in to be found.
public async Task<(ResumeDocument Meta, string Path)?> GetLatestForTalentAsync(string seekerId)
{
    var doc = await db.ResumeDocuments.AsNoTracking()
        .Where(r => r.OwnerId == seekerId && r.Owner!.IsDiscoverable)
        .OrderByDescending(r => r.UploadedAtUtc)
        .FirstOrDefaultAsync();
    return doc is null ? null : Locate(doc);
}
    public async Task<bool> DeleteAsync(Guid id)
    {
        var doc = await db.ResumeDocuments.SingleOrDefaultAsync(d =>
            d.Id == id && d.OwnerId == currentUser.UserId);
        if (doc is null) return false;

        var inUse = await db.JobApplications.AnyAsync(a => a.ResumeDocumentId == id);
        if (inUse)
            throw new InvalidOperationException("This CV was sent with an application, so it cannot be deleted.");

        var path = Path.Combine(UploadRoot, doc.StoredFileName);
        db.ResumeDocuments.Remove(doc);
        await db.SaveChangesAsync();

        // Delete the file only after the database change succeeded.
        if (File.Exists(path)) File.Delete(path);
        return true;
    }

    private (ResumeDocument Meta, string Path)? Locate(ResumeDocument doc)
    {
        var path = Path.Combine(UploadRoot, doc.StoredFileName);
        return File.Exists(path) ? (doc, path) : null;
    }
}