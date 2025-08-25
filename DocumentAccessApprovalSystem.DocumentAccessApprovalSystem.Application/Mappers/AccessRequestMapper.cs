using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Domain.Entities;

namespace DocumentAccessApprovalSystem.Application.Mappers
{
    public static class AccessRequestMapper
    {
        public static AccessRequestDto Map(AccessRequest ar)
            => new(
            ar.Id,
            ar.UserId,
            ar.DocumentId,
            ar.Reason,
            ar.AccessType,
            ar.CreatedAt,
            ar.Status.ToString(),
            Map(ar.Decision)
        );


        private static DecisionDto? Map(Decision? d)
            => d is null
            ? null
            : new DecisionDto(d.ApproverId, d.Type, d.Comment, d.DecidedAt);
    }
}
