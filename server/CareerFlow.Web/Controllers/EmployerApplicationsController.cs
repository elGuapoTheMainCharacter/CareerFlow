[HttpGet("{id:guid}/resume")]
public async Task<IActionResult> Resume(Guid id)
{
    var file = await storage.GetForEmployerAsync(id);
    return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
}
