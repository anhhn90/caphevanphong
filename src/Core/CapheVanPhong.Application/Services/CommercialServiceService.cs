#nullable enable

using CapheVanPhong.Application.Helpers;
using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class CommercialServiceService : ICommercialServiceService
{
    private readonly ICommercialServiceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    private const string ImageSubfolder = "commercial-services";

    public CommercialServiceService(
        ICommercialServiceRepository repository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public Task<IReadOnlyList<CommercialService>> GetAllAsync(CancellationToken ct = default)
        => _repository.GetAllAsync(ct);

    public Task<IReadOnlyList<CommercialService>> GetActiveAsync(CancellationToken ct = default)
        => _repository.GetActiveOrderedAsync(ct);

    public Task<CommercialService?> GetByIdAsync(int id, CancellationToken ct = default)
        => _repository.GetByIdAsync(id, ct);

    public Task<CommercialService?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => _repository.GetBySlugAsync(slug, ct);

    public async Task<(bool Success, string? Error)> CreateAsync(
        string title,
        string introduction,
        string? content,
        string iconClass,
        bool isActive,
        int displayOrder,
        Stream? imageStream,
        string? imageFileName,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title)) return (false, "Title is required.");
        if (string.IsNullOrWhiteSpace(introduction)) return (false, "Introduction is required.");
        if (string.IsNullOrWhiteSpace(iconClass)) return (false, "Icon class is required.");

        var slug = SlugHelper.Generate(title);
        if (string.IsNullOrWhiteSpace(slug))
            return (false, "Could not generate a valid slug from the title.");

        if (await _repository.SlugExistsAsync(slug, excludeId: null, ct))
            return (false, $"A service with slug \"{slug}\" already exists.");

        string? storedImageName = null;
        if (imageStream is not null && !string.IsNullOrWhiteSpace(imageFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{imageFileName}";
            storedImageName = await _fileStorageService.SaveAsync(ImageSubfolder, fileName, imageStream, ct);
        }

        var service = CommercialService.Create(title, slug, introduction, content, iconClass, storedImageName, isActive, displayOrder);
        await _repository.AddAsync(service, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(
        int id,
        string title,
        string introduction,
        string? content,
        string iconClass,
        bool isActive,
        int displayOrder,
        string? existingImageName,
        Stream? newImageStream,
        string? newImageFileName,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title)) return (false, "Title is required.");
        if (string.IsNullOrWhiteSpace(introduction)) return (false, "Introduction is required.");
        if (string.IsNullOrWhiteSpace(iconClass)) return (false, "Icon class is required.");

        var service = await _repository.GetByIdAsync(id, ct);
        if (service is null) return (false, "Service not found.");

        var slug = SlugHelper.Generate(title);
        if (string.IsNullOrWhiteSpace(slug))
            return (false, "Could not generate a valid slug from the title.");

        if (await _repository.SlugExistsAsync(slug, excludeId: id, ct))
            return (false, $"A service with slug \"{slug}\" already exists.");

        var oldImageName = service.ImageName;
        var imageName = existingImageName;

        if (newImageStream is not null && !string.IsNullOrWhiteSpace(newImageFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{newImageFileName}";
            imageName = await _fileStorageService.SaveAsync(ImageSubfolder, fileName, newImageStream, ct);
        }

        service.Update(title, slug, introduction, content, iconClass, imageName, isActive, displayOrder);
        _repository.Update(service);
        await _unitOfWork.SaveChangesAsync(ct);

        if (oldImageName is not null && !string.Equals(oldImageName, imageName, StringComparison.OrdinalIgnoreCase))
            await _fileStorageService.RenameToDeletedAsync(ImageSubfolder, oldImageName, ct);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(int id, CancellationToken ct = default)
    {
        var service = await _repository.GetByIdAsync(id, ct);
        if (service is null) return (false, "Service not found.");

        var imageName = service.ImageName;
        _repository.Delete(service);
        await _unitOfWork.SaveChangesAsync(ct);

        if (imageName is not null)
            await _fileStorageService.RenameToDeletedAsync(ImageSubfolder, imageName, ct);

        return (true, null);
    }
}
