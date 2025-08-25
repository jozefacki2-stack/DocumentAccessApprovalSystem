using DocumentAccessApprovalSystem.Domain.Enums;

namespace DocumentAccessApprovalSystem.Domain.Entities
{
    public class AccessRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid DocumentId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public AccessType AccessType { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public RequestStatus Status { get; set; } = RequestStatus.Pending;


        public Decision? Decision { get; set; }
    }
}
