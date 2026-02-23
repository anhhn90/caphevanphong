#nullable enable

using CapheVanPhong.Domain.Entities;

namespace CapheVanPhong.Application.Services;

public interface ICustomerService
{
    // Customer (company) CRUD
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> CreateAsync(
        string name,
        string? description,
        bool isGoldCustomer,
        bool isActive,
        int displayOrder,
        Stream? logoStream = null,
        string? logoFileName = null,
        CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> UpdateAsync(
        int id,
        string name,
        string? description,
        bool isGoldCustomer,
        bool isActive,
        int displayOrder,
        string? existingLogoName = null,
        Stream? newLogoStream = null,
        string? newLogoFileName = null,
        CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Representative management
    Task<(bool success, string? error)> AddRepresentativeAsync(
        int customerId,
        string userId,
        string title,
        string displayName,
        string position,
        string? comment,
        int starRating,
        bool isShowOnHomepage,
        Stream? avatarStream = null,
        string? avatarFileName = null,
        CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> UpdateRepresentativeAsync(
        int representativeId,
        string userId,
        string title,
        string displayName,
        string position,
        string? comment,
        int starRating,
        bool isShowOnHomepage,
        string? existingAvatarName = null,
        Stream? newAvatarStream = null,
        string? newAvatarFileName = null,
        CancellationToken cancellationToken = default);
    Task<(bool success, string? error)> RemoveRepresentativeAsync(int representativeId, CancellationToken cancellationToken = default);

    // Public-facing queries
    Task<IReadOnlyList<Customer>> GetGoldCustomersForHomepageAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomerRepresentative>> GetTestimonialsForHomepageAsync(CancellationToken cancellationToken = default);
}
