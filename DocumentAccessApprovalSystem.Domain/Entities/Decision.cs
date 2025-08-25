using DocumentAccessApprovalSystem.Domain.Enums;

namespace DocumentAccessApprovalSystem.Domain.Entities
{
    public class Decision
    {
        public Guid Id { get; set; }
        public Guid AccessRequestId { get; set; }
        public Guid ApproverId { get; set; }
        public DecisionType Type { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset DecidedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}
