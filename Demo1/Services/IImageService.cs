namespace Demo1.Services;

public interface IImageService
{
    Task<string?> SaveAsync(IFormFile? file, string uploadFolder, long maximumBytes, CancellationToken cancellationToken = default);
    Task DeleteAsync(string? publicPath, string uploadFolder);
}
