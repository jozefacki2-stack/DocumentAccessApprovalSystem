using DocumentAccessApprovalSystem.Application.DTOs;

namespace DocumentAccessApprovalSystem.Application.Interfaces
{
    public interface IAccessRequestService
    {
        Task<AccessRequestDto> CreateAsync(CreateAccessRequestDto dto, CancellationToken ct = default);
        Task<IReadOnlyList<AccessRequestDto>> GetPendingAsync(CancellationToken ct = default);
        Task<AccessRequestDto> DecideAsync(Guid requestId, DecideRequestDto dto, CancellationToken ct = default);
        Task<IReadOnlyList<AccessRequestDto>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    }
}
