using DocumentAccessApprovalSystem.Domain.Enums;

namespace DocumentAccessApprovalSystem.Application.DTOs
{
    public record CreateAccessRequestDto(
        Guid UserId,
        Guid DocumentId,
        string Reason,
        AccessType AccessType
    );
}
