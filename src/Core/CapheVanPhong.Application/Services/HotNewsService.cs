#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class HotNewsService : IHotNewsService
{
    private readonly IHotNewsRepository _hotNewsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public HotNewsService(
        IHotNewsRepository hotNewsRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _hotNewsRepository = hotNewsRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public Task<IReadOnlyList<HotNews>> GetAllAsync(CancellationToken cancellationToken = default)
        => _hotNewsRepository.GetAllAsync(cancellationToken);

    public Task<HotNews?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _hotNewsRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<HotNews>> GetActiveAsync(CancellationToken cancellationToken = default)
        => _hotNewsRepository.GetActiveAsync(cancellationToken);

    public async Task<(bool success, string? error)> CreateAsync(
        string title,
        string content,
        bool isActive,
        Stream? imageStream = null,
        string? originalFileName = null,
        CancellationToken cancellationToken = default)
    {
        string? imageName = null;
        if (imageStream is not null && !string.IsNullOrWhiteSpace(originalFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{originalFileName}";
            imageName = await _fileStorageService.SaveAsync("hotnews", fileName, imageStream, cancellationToken);
        }

        var hotNews = HotNews.Create(title, content, imageName, isActive);
        await _hotNewsRepository.AddAsync(hotNews, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool success, string? error)> UpdateAsync(
        int id,
        string title,
        string content,
        bool isActive,
        string? existingImageName = null,
        Stream? newImageStream = null,
        string? newOriginalFileName = null,
        CancellationToken cancellationToken = default)
    {
        var hotNews = await _hotNewsRepository.GetByIdAsync(id, cancellationToken);
        if (hotNews is null)
            return (false, "Hot news not found.");

        var oldImageName = hotNews.ImageName;
        var imageName = existingImageName; // keep existing unless replaced

        if (newImageStream is not null && !string.IsNullOrWhiteSpace(newOriginalFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{newOriginalFileName}";
            imageName = await _fileStorageService.SaveAsync("hotnews", fileName, newImageStream, cancellationToken);
        }

        hotNews.Update(title, content, imageName, isActive);
        _hotNewsRepository.Update(hotNews);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (oldImageName is not null && !string.Equals(oldImageName, imageName, StringComparison.OrdinalIgnoreCase))
            await _fileStorageService.RenameToDeletedAsync("hotnews", oldImageName, cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var hotNews = await _hotNewsRepository.GetByIdAsync(id, cancellationToken);
        if (hotNews is null)
            return (false, "Hot news not found.");

        var imageName = hotNews.ImageName;
        _hotNewsRepository.Delete(hotNews);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (imageName is not null)
            await _fileStorageService.RenameToDeletedAsync("hotnews", imageName, cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> SetActiveAsync(int id, bool isActive, CancellationToken cancellationToken = default)
    {
        var hotNews = await _hotNewsRepository.GetByIdAsync(id, cancellationToken);
        if (hotNews is null)
            return (false, "Hot news not found.");

        hotNews.SetActive(isActive);
        _hotNewsRepository.Update(hotNews);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
