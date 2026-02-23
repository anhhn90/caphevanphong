#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BrandService(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Brand>> GetAllAsync(CancellationToken cancellationToken = default)
        => _brandRepository.GetAllAsync(cancellationToken);

    public Task<Brand?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _brandRepository.GetByIdAsync(id, cancellationToken);

    public Task<Brand?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
        => _brandRepository.GetBySlugAsync(slug, cancellationToken);

    public async Task<(bool success, string? error)> CreateAsync(
        string name, string slug, string? description, string? logoName, int displayOrder,
        CancellationToken cancellationToken = default)
    {
        if (await _brandRepository.SlugExistsAsync(slug, null, cancellationToken))
            return (false, $"Slug '{slug}' đã được sử dụng.");

        var brand = Brand.Create(name, slug, description, logoName, displayOrder);
        await _brandRepository.AddAsync(brand, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool success, string? error)> UpdateAsync(
        int id, string name, string slug, string? description, string? logoName, int displayOrder, bool isActive,
        CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
        if (brand is null)
            return (false, "Thương hiệu không tồn tại.");

        if (await _brandRepository.SlugExistsAsync(slug, id, cancellationToken))
            return (false, $"Slug '{slug}' đã được sử dụng.");

        brand.Update(name, slug, description, logoName, displayOrder);
        brand.SetActive(isActive);
        _brandRepository.Update(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var brand = await _brandRepository.GetByIdAsync(id, cancellationToken);
        if (brand is null)
            return (false, "Thương hiệu không tồn tại.");

        _brandRepository.Delete(brand);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (true, null);
    }
}
