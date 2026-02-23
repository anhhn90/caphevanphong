#nullable enable

using CapheVanPhong.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace CapheVanPhong.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _baseImagePath;

    public FileStorageService(IWebHostEnvironment env)
    {
        _baseImagePath = Path.Combine(env.WebRootPath, "public", "img");
    }

    public async Task<string> SaveAsync(string subfolder, string fileName, Stream content, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_baseImagePath, subfolder);
        Directory.CreateDirectory(dir);

        var filePath = Path.Combine(dir, fileName);
        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fs, cancellationToken);

        return fileName;
    }

    public Task RenameToDeletedAsync(string subfolder, string fileName, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(_baseImagePath, subfolder);
        var filePath = Path.Combine(dir, fileName);

        if (!File.Exists(filePath))
            return Task.CompletedTask;

        var ext = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var newPath = Path.Combine(dir, $"{nameWithoutExt}_deleted{ext}");

        File.Move(filePath, newPath, overwrite: true);
        return Task.CompletedTask;
    }
}
