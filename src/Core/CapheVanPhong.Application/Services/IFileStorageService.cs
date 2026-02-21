#nullable enable

namespace CapheVanPhong.Application.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Saves a file to wwwroot/public/img/{subfolder}/{fileName} and returns the stored filename.
    /// </summary>
    Task<string> SaveAsync(string subfolder, string fileName, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a file to {name}_deleted.{ext} — used when the owning entity is deleted.
    /// </summary>
    Task RenameToDeletedAsync(string subfolder, string fileName, CancellationToken cancellationToken = default);
}
