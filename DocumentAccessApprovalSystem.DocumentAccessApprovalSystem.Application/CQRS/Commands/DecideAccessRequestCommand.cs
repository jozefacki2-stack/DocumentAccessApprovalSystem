using DocumentAccessApprovalSystem.Application.DTOs;
using MediatR;

namespace DocumentAccessApprovalSystem.Application.CQRS.Commands
{
    public record DecideAccessRequestCommand(
        Guid RequestId, 
        DecideRequestDto Dto) : IRequest<AccessRequestDto>;
}
