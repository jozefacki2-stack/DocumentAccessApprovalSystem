using DocumentAccessApprovalSystem.Application.DTOs;
using MediatR;

namespace DocumentAccessApprovalSystem.Application.CQRS.Queries
{
    public record GetUserRequestsQuery(Guid UserId) : IRequest<IReadOnlyList<AccessRequestDto>>;
}
