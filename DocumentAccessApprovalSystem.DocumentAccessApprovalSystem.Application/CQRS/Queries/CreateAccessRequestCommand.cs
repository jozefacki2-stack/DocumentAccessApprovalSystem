using DocumentAccessApprovalSystem.Application.DTOs;
using MediatR;

namespace DocumentAccessApprovalSystem.Application.CQRS.Queries
{
    public record CreateAccessRequestCommand(CreateAccessRequestDto Dto) : IRequest<AccessRequestDto>;
}
