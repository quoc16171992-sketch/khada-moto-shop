using System.ComponentModel.DataAnnotations;

namespace Demo1.Services;

public class LocalImageService(IWebHostEnvironment environment, IConfiguration configuration) : IImageService
{
    private static readonly IReadOnlyDictionary<string, string[]> AllowedTypes = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ["image/jpeg", "image/jpg"],
        [".jpeg"] = ["image/jpeg", "image/jpg"],
        [".png"] = ["image/png", "image/x-png"],
        [".webp"] = ["image/webp"]
    };

    public async Task<string?> SaveAsync(IFormFile? file, string uploadFolder, long maximumBytes, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        ValidateFolder(uploadFolder);
        if (file.Length > maximumBytes)
        {
            throw new ValidationException($"Ảnh vượt quá dung lượng cho phép ({maximumBytes / 1024 / 1024} MB). ");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedTypes.TryGetValue(extension, out var expectedContentTypes)
            || (!string.IsNullOrWhiteSpace(file.ContentType)
                && !expectedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)))
        {
            throw new ValidationException("Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP hợp lệ.");
        }

        var root = GetUploadRoot(uploadFolder);
        Directory.CreateDirectory(root);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var destination = Path.GetFullPath(Path.Combine(root, fileName));
        if (!destination.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationException("Đường dẫn ảnh không hợp lệ.");
        }

        await using var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream, cancellationToken);
        return $"/uploads/{uploadFolder}/{fileName}";
    }

    public Task DeleteAsync(string? publicPath, string uploadFolder)
    {
        ValidateFolder(uploadFolder);
        var prefix = $"/uploads/{uploadFolder}/";
        if (string.IsNullOrWhiteSpace(publicPath) || !publicPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        var fileName = Path.GetFileName(publicPath);
        var root = GetUploadRoot(uploadFolder);
        var target = Path.GetFullPath(Path.Combine(root, fileName));
        if (target.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && File.Exists(target))
        {
            File.Delete(target);
        }

        return Task.CompletedTask;
    }

    private string GetUploadRoot(string uploadFolder)
    {
        var persistentRoot = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH")
            ?? configuration["PersistentDataPath"];
        var uploadRoot = string.IsNullOrWhiteSpace(persistentRoot)
            ? Path.Combine(environment.WebRootPath, "uploads")
            : Path.Combine(Path.GetFullPath(persistentRoot), "uploads");

        return Path.GetFullPath(Path.Combine(uploadRoot, uploadFolder));
    }

    private static void ValidateFolder(string folder)
    {
        if (folder is not ("banners" or "products"))
        {
            throw new ArgumentOutOfRangeException(nameof(folder));
        }
    }
}
