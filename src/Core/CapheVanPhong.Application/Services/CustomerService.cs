#nullable enable

using CapheVanPhong.Domain.Entities;
using CapheVanPhong.Domain.Interfaces;

namespace CapheVanPhong.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorageService;

    public CustomerService(
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorageService)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _fileStorageService = fileStorageService;
    }

    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        => _customerRepository.GetAllWithRepresentativesAsync(cancellationToken);

    public Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => _customerRepository.GetByIdWithRepresentativesAsync(id, cancellationToken);

    public async Task<(bool success, string? error)> CreateAsync(
        string name,
        string? description,
        bool isGoldCustomer,
        bool isActive,
        int displayOrder,
        Stream? logoStream = null,
        string? logoFileName = null,
        CancellationToken cancellationToken = default)
    {
        string? logoName = null;
        if (logoStream is not null && !string.IsNullOrWhiteSpace(logoFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{logoFileName}";
            logoName = await _fileStorageService.SaveAsync("customers", fileName, logoStream, cancellationToken);
        }

        try
        {
            var customer = Customer.Create(name, description, logoName, isGoldCustomer, isActive, displayOrder);
            await _customerRepository.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (true, null);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> UpdateAsync(
        int id,
        string name,
        string? description,
        bool isGoldCustomer,
        bool isActive,
        int displayOrder,
        string? existingLogoName = null,
        Stream? newLogoStream = null,
        string? newLogoFileName = null,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        if (customer is null)
            return (false, "Customer not found.");

        var oldLogoName = customer.LogoName;
        var logoName = existingLogoName;

        if (newLogoStream is not null && !string.IsNullOrWhiteSpace(newLogoFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{newLogoFileName}";
            logoName = await _fileStorageService.SaveAsync("customers", fileName, newLogoStream, cancellationToken);
        }

        try
        {
            customer.Update(name, description, logoName, isGoldCustomer, isActive, displayOrder);
            _customerRepository.Update(customer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message);
        }

        if (oldLogoName is not null && !string.Equals(oldLogoName, logoName, StringComparison.OrdinalIgnoreCase))
            await _fileStorageService.RenameToDeletedAsync("customers", oldLogoName, cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdWithRepresentativesAsync(id, cancellationToken);
        if (customer is null)
            return (false, "Customer not found.");

        // Collect image names before deletion
        var logoName = customer.LogoName;
        var avatarNames = customer.Representatives
            .Where(r => r.AvatarName is not null)
            .Select(r => r.AvatarName!)
            .ToList();

        _customerRepository.Delete(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Soft-delete images after DB changes committed
        if (logoName is not null)
            await _fileStorageService.RenameToDeletedAsync("customers", logoName, cancellationToken);

        foreach (var avatarName in avatarNames)
            await _fileStorageService.RenameToDeletedAsync("representatives", avatarName, cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> AddRepresentativeAsync(
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
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer is null)
            return (false, "Customer not found.");

        var alreadyExists = await _customerRepository.RepresentativeExistsAsync(customerId, userId, cancellationToken: cancellationToken);
        if (alreadyExists)
            return (false, "This user is already a representative of this customer.");

        string? avatarName = null;
        if (avatarStream is not null && !string.IsNullOrWhiteSpace(avatarFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{avatarFileName}";
            avatarName = await _fileStorageService.SaveAsync("representatives", fileName, avatarStream, cancellationToken);
        }

        try
        {
            var representative = CustomerRepresentative.Create(customerId, userId, title, displayName, position, comment, starRating, isShowOnHomepage, avatarName);
            await _customerRepository.AddRepresentativeAsync(representative, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return (true, null);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> UpdateRepresentativeAsync(
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
        CancellationToken cancellationToken = default)
    {
        var representative = await _customerRepository.GetRepresentativeByIdAsync(representativeId, cancellationToken);
        if (representative is null)
            return (false, "Representative not found.");

        // Check uniqueness only if userId changed
        if (!string.Equals(representative.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            var alreadyExists = await _customerRepository.RepresentativeExistsAsync(representative.CustomerId, userId, representativeId, cancellationToken);
            if (alreadyExists)
                return (false, "This user is already a representative of this customer.");
        }

        var oldAvatarName = representative.AvatarName;
        var avatarName = existingAvatarName;

        if (newAvatarStream is not null && !string.IsNullOrWhiteSpace(newAvatarFileName))
        {
            var fileName = $"{Guid.NewGuid():N}_{newAvatarFileName}";
            avatarName = await _fileStorageService.SaveAsync("representatives", fileName, newAvatarStream, cancellationToken);
        }

        try
        {
            representative.Update(userId, title, displayName, position, comment, starRating, isShowOnHomepage, avatarName);
            _customerRepository.UpdateRepresentative(representative);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ArgumentException ex)
        {
            return (false, ex.Message);
        }

        if (oldAvatarName is not null && !string.Equals(oldAvatarName, avatarName, StringComparison.OrdinalIgnoreCase))
            await _fileStorageService.RenameToDeletedAsync("representatives", oldAvatarName, cancellationToken);

        return (true, null);
    }

    public async Task<(bool success, string? error)> RemoveRepresentativeAsync(int representativeId, CancellationToken cancellationToken = default)
    {
        var representative = await _customerRepository.GetRepresentativeByIdAsync(representativeId, cancellationToken);
        if (representative is null)
            return (false, "Representative not found.");

        var avatarName = representative.AvatarName;
        _customerRepository.DeleteRepresentative(representative);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (avatarName is not null)
            await _fileStorageService.RenameToDeletedAsync("representatives", avatarName, cancellationToken);

        return (true, null);
    }

    public Task<IReadOnlyList<Customer>> GetGoldCustomersForHomepageAsync(CancellationToken cancellationToken = default)
        => _customerRepository.GetGoldCustomersAsync(cancellationToken);

    public Task<IReadOnlyList<CustomerRepresentative>> GetTestimonialsForHomepageAsync(CancellationToken cancellationToken = default)
        => _customerRepository.GetHomepageTestimonialsAsync(cancellationToken);
}
