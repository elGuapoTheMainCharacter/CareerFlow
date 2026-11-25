[HttpPost]
[RequestSizeLimit(3 * 1024 * 1024)]
public async Task<IActionResult> Upload(IFormFile file)
{
    try { return Ok(await storage.SaveAsync(file)); }
    catch (ArgumentException ex) { return BadRequest(new ApiError(ex.Message)); }
}

[HttpGet("{id:guid}/download")]
public async Task<IActionResult> Download(Guid id)
{
    var file = await storage.GetOwnAsync(id);
    return file is null ? NotFound() : File(file.Content, file.ContentType, file.FileName);
}
