using DocumentAccessApprovalSystem.Domain.Enums;
using MediatR;

namespace DocumentAccessApprovalSystem.Application.Notifications
{
    public record DecisionMadeNotification(
        Guid AccessRequestId,
        Guid UserId,
        Guid DocumentId,
        DecisionType DecisionType,
        string? Comment,
        DateTimeOffset DecidedAt
    ) : INotification;
}
