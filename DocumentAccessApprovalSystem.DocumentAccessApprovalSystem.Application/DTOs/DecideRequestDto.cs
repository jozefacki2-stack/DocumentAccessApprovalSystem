using DocumentAccessApprovalSystem.Domain.Enums;

namespace DocumentAccessApprovalSystem.Application.DTOs
{
    public record DecideRequestDto(
        Guid ApproverId,
        DecisionType Type,
        string? Comment
    );
}
