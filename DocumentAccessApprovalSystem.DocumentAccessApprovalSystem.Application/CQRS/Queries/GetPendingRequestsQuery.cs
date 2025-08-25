using DocumentAccessApprovalSystem.Application.DTOs;
using MediatR;

namespace DocumentAccessApprovalSystem.Application.CQRS.Queries
{
    public record GetPendingRequestsQuery() : IRequest<IReadOnlyList<AccessRequestDto>>;
}
