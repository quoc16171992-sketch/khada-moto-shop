using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Demo1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Demo1.Pages.Admin.Backup;

[Authorize(Roles = "Admin")]
[RequestFormLimits(MultipartBodyLengthLimit = MaxRestoreBytes)]
[RequestSizeLimit(MaxRestoreBytes)]
public class IndexModel(ApplicationDbContext db, IWebHostEnvironment environment, IConfiguration configuration) : PageModel
{
    private const long MaxRestoreBytes = 200 * 1024 * 1024;
    private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public string DatabasePath { get; private set; } = "Chưa tìm thấy";
    public long DatabaseSizeBytes { get; private set; }
    public int UploadFileCount { get; private set; }
    public long UploadSizeBytes { get; private set; }
    public int UploadFolderCount { get; private set; }

    [TempData] public string? StatusMessage { get; set; }
    [TempData] public string? ErrorMessage { get; set; }

    public void OnGet()
    {
        var dbPath = GetDbPath();
        if (System.IO.File.Exists(dbPath))
        {
            DatabasePath = dbPath;
            DatabaseSizeBytes = new FileInfo(dbPath).Length;
        }

        var uploadsRoot = GetUploadsRoot();
        if (Directory.Exists(uploadsRoot))
        {
            var files = Directory.EnumerateFiles(uploadsRoot, "*", SearchOption.AllDirectories).ToList();
            UploadFileCount = files.Count;
            UploadSizeBytes = files.Sum(f => new FileInfo(f).Length);
            UploadFolderCount = Directory.EnumerateDirectories(uploadsRoot).Count();
        }
    }

    public async Task<IActionResult> OnGetDownloadAsync(CancellationToken cancellationToken)
    {
        var dbPath = GetDbPath();
        if (!System.IO.File.Exists(dbPath))
        {
            ErrorMessage = "Không tìm thấy file database để sao lưu.";
            return RedirectToPage();
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"khada-backup-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            // VACUUM INTO tạo bản snapshot sạch, gộp luôn dữ liệu đang nằm trong WAL.
            var snapshotDb = Path.Combine(tempDir, "khadashop.db");
            await db.Database.OpenConnectionAsync(cancellationToken);
            try
            {
                var connection = (SqliteConnection)db.Database.GetDbConnection();
                await using var command = connection.CreateCommand();
                command.CommandText = "VACUUM INTO $file";
                command.Parameters.AddWithValue("$file", snapshotDb);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
            finally
            {
                await db.Database.CloseConnectionAsync();
            }

            var zipPath = Path.Combine(tempDir, "backup.zip");
            await using (var zipStream = new FileStream(zipPath, FileMode.CreateNew))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                archive.CreateEntryFromFile(snapshotDb, "khadashop.db", CompressionLevel.Optimal);

                var manifest = JsonSerializer.Serialize(new
                {
                    createdAtUtc = DateTime.UtcNow.ToString("O"),
                    databaseBytes = new FileInfo(snapshotDb).Length,
                    note = "Backup KhadaShop: khadashop.db + thu muc uploads/. Khoi phuc qua trang Admin > Sao luu."
                }, new JsonSerializerOptions { WriteIndented = true });
                var manifestEntry = archive.CreateEntry("backup-manifest.json", CompressionLevel.Optimal);
                await using var manifestWriter = new StreamWriter(manifestEntry.Open());
                await manifestWriter.WriteAsync(manifest);

                var uploadsRoot = GetUploadsRoot();
                if (Directory.Exists(uploadsRoot))
                {
                    var uploadsParent = Path.GetDirectoryName(uploadsRoot)!;
                    foreach (var file in Directory.EnumerateFiles(uploadsRoot, "*", SearchOption.AllDirectories))
                    {
                        var entryName = Path.GetRelativePath(uploadsParent, file).Replace('\\', '/');
                        archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
                    }
                }
            }

            var bytes = await System.IO.File.ReadAllBytesAsync(zipPath, cancellationToken);
            return base.File(bytes, "application/zip", $"khadashop-backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.zip");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Tạo file backup thất bại: {ex.Message}";
            return RedirectToPage();
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
        }
    }

    public async Task<IActionResult> OnPostRestoreAsync(IFormFile? backupFile, CancellationToken cancellationToken)
    {
        if (backupFile is null || backupFile.Length == 0)
        {
            ErrorMessage = "Vui lòng chọn file backup (.zip).";
            return RedirectToPage();
        }
        if (backupFile.Length > MaxRestoreBytes)
        {
            ErrorMessage = $"File backup vượt quá dung lượng cho phép ({MaxRestoreBytes / 1024 / 1024} MB).";
            return RedirectToPage();
        }
        if (!string.Equals(Path.GetExtension(backupFile.FileName), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Chỉ chấp nhận file .zip xuất từ trang Sao lưu.";
            return RedirectToPage();
        }

        var tempDir = Path.Combine(Path.GetTempPath(), $"khada-restore-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        try
        {
            var zipPath = Path.Combine(tempDir, "upload.zip");
            await using (var stream = new FileStream(zipPath, FileMode.CreateNew))
            {
                await backupFile.CopyToAsync(stream, cancellationToken);
            }

            using var archive = ZipFile.OpenRead(zipPath);
            var safeEntries = archive.Entries
                .Where(e => e.Name.Length > 0)
                .Select(e => new
                {
                    Entry = e,
                    NormalizedName = e.FullName.Replace('\\', '/')
                })
                .ToList();

            if (safeEntries.Any(e => e.NormalizedName.Contains("..") || Path.IsPathRooted(e.NormalizedName)))
            {
                ErrorMessage = "File backup chứa đường dẫn không hợp lệ, đã hủy khôi phục.";
                return RedirectToPage();
            }

            var dbEntry = safeEntries.FirstOrDefault(e => Path.GetFileName(e.NormalizedName).Equals("khadashop.db", StringComparison.OrdinalIgnoreCase));
            if (dbEntry is null)
            {
                ErrorMessage = "File zip không chứa khadashop.db — phải là file backup xuất từ trang này.";
                return RedirectToPage();
            }
            if (safeEntries.Count(e => Path.GetExtension(e.NormalizedName).Equals(".db", StringComparison.OrdinalIgnoreCase)) > 1)
            {
                ErrorMessage = "File zip chứa nhiều file .db, không hợp lệ.";
                return RedirectToPage();
            }

            var totalUncompressed = safeEntries.Sum(e => e.Entry.Length);
            if (totalUncompressed > MaxRestoreBytes)
            {
                ErrorMessage = $"Nội dung file backup vượt quá dung lượng cho phép ({MaxRestoreBytes / 1024 / 1024} MB).";
                return RedirectToPage();
            }

            var uploadsRoot = GetUploadsRoot();
            Directory.CreateDirectory(uploadsRoot);
            var restoredImages = 0;
            foreach (var item in safeEntries.Where(e => e.NormalizedName.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase)))
            {
                if (!AllowedImageExtensions.Contains(Path.GetExtension(item.Entry.Name).ToLowerInvariant()))
                {
                    continue;
                }

                var relative = Path.GetRelativePath("uploads", item.NormalizedName);
                var target = Path.GetFullPath(Path.Combine(uploadsRoot, relative));
                if (!target.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                item.Entry.ExtractToFile(target, overwrite: true);
                restoredImages++;
            }

            await RestoreDatabaseAsync(dbEntry.Entry, tempDir, cancellationToken);

            StatusMessage = $"Khôi phục thành công: database + {restoredImages} ảnh uploads. Nếu bạn vừa đăng nhập, hãy đăng nhập lại bằng tài khoản nằm trong file backup.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Khôi phục thất bại, dữ liệu hiện tại vẫn giữ nguyên: {ex.Message}";
        }
        finally
        {
            try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
        }

        return RedirectToPage();
    }

    private async Task RestoreDatabaseAsync(ZipArchiveEntry dbEntry, string tempDir, CancellationToken cancellationToken)
    {
        var dbPath = GetDbPath();
        var uploadedDb = Path.Combine(tempDir, "khadashop.db");
        dbEntry.ExtractToFile(uploadedDb, overwrite: true);

        var previousDb = Path.Combine(tempDir, "previous.db");
        if (System.IO.File.Exists(dbPath))
        {
            System.IO.File.Copy(dbPath, previousDb, overwrite: true);
        }

        await db.Database.CloseConnectionAsync();
        SqliteConnection.ClearAllPools();

        try
        {
            System.IO.File.Copy(uploadedDb, dbPath, overwrite: true);
            foreach (var suffix in new[] { "-wal", "-shm" })
            {
                var sidecar = dbPath + suffix;
                if (System.IO.File.Exists(sidecar))
                {
                    System.IO.File.Delete(sidecar);
                }
            }

            await db.Database.OpenConnectionAsync(cancellationToken);
            await using var command = ((SqliteConnection)db.Database.GetDbConnection()).CreateCommand();
            command.CommandText = "PRAGMA integrity_check;";
            var result = (await command.ExecuteScalarAsync(cancellationToken))?.ToString();
            if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File database trong backup không hợp lệ (integrity_check: {result}).");
            }
        }
        catch
        {
            // Trả lại database cũ nếu file mới lỗi để admin không bị mất trạng thái hiện tại.
            SqliteConnection.ClearAllPools();
            if (System.IO.File.Exists(previousDb))
            {
                System.IO.File.Copy(previousDb, dbPath, overwrite: true);
            }
            throw;
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }
    }

    private string GetDbPath()
    {
        var connectionString = db.Database.GetConnectionString() ?? string.Empty;
        var match = Regex.Match(connectionString, @"Data Source=([^;]+)", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            var path = match.Groups[1].Value.Trim();
            return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(environment.ContentRootPath, path));
        }

        var persistentRoot = GetPersistentRoot();
        return Path.Combine(string.IsNullOrWhiteSpace(persistentRoot) ? environment.ContentRootPath : Path.GetFullPath(persistentRoot), "khadashop.db");
    }

    private string GetUploadsRoot()
    {
        var persistentRoot = GetPersistentRoot();
        var uploadRoot = string.IsNullOrWhiteSpace(persistentRoot)
            ? Path.Combine(environment.WebRootPath ?? environment.ContentRootPath, "uploads")
            : Path.Combine(Path.GetFullPath(persistentRoot), "uploads");
        return Path.GetFullPath(uploadRoot);
    }

    private string? GetPersistentRoot()
    {
        var persistentRoot = Environment.GetEnvironmentVariable("RAILWAY_VOLUME_MOUNT_PATH") ?? configuration["PersistentDataPath"];
        return string.IsNullOrWhiteSpace(persistentRoot) ? null : Path.GetFullPath(persistentRoot);
    }
}
