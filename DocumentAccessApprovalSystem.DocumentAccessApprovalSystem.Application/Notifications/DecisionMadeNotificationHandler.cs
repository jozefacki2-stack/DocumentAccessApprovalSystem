using MediatR;
using Microsoft.Extensions.Logging;

namespace DocumentAccessApprovalSystem.Application.Notifications
{
    public class DecisionMadeNotificationHandler : INotificationHandler<DecisionMadeNotification>
    {
        private readonly ILogger<DecisionMadeNotificationHandler> _logger;
        public DecisionMadeNotificationHandler(ILogger<DecisionMadeNotificationHandler> logger) => _logger = logger;

        public async Task Handle(DecisionMadeNotification notification, CancellationToken ct)
        {
            _logger.LogInformation(
                "Notifying user {UserId} about decision {Decision} on request {RequestId} (doc {DocumentId}). Comment: {Comment}",
                notification.UserId, notification.DecisionType, notification.AccessRequestId, notification.DocumentId, notification.Comment);

            await Task.Delay(100, ct);
        }
    }
}
