using DocumentAccessApprovalSystem.Domain.Enums;

namespace DocumentAccessApprovalSystem.Application.DTOs
{
    public record DecisionDto(
        Guid ApproverId,
        DecisionType Type,
        string? Comment,
        DateTimeOffset DecidedAt
    );
}
