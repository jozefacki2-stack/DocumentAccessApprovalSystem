using DocumentAccessApprovalSystem.Domain.Enums;

namespace DocumentAccessApprovalSystem.Application.DTOs
{
    public record AccessRequestDto(
        Guid Id,
        Guid UserId,
        Guid DocumentId,
        string Reason,
        AccessType AccessType,
        DateTimeOffset CreatedAt,
        string Status,
        DecisionDto? Decision
    );
}
