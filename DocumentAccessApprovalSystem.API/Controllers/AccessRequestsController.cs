using DocumentAccessApprovalSystem.Application.CQRS.Commands;
using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAccessApprovalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccessRequestsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AccessRequestsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Policy = "UserOrApprover")]
    public async Task<ActionResult<AccessRequestDto>> Create(CreateAccessRequestDto dto, CancellationToken ct)
    {
        var created = await _mediator.Send(new CreateAccessRequestCommand(dto), ct);
        return CreatedAtAction(nameof(GetMine), new { userId = created.UserId }, created);
    }

    [HttpGet("pending")]
    [Authorize(Policy = "ApproverOnly")]
    public async Task<ActionResult<IEnumerable<AccessRequestDto>>> GetPending(CancellationToken ct)
        => Ok(await _mediator.Send(new GetPendingRequestsQuery(), ct));

    [HttpPost("{id:guid}/decision")]
    [Authorize(Policy = "ApproverOnly")]
    public async Task<ActionResult<AccessRequestDto>> Decide(Guid id, DecideRequestDto dto, CancellationToken ct)
        => Ok(await _mediator.Send(new DecideAccessRequestCommand(id, dto), ct));

    [HttpGet("mine/{userId:guid}")]
    [Authorize(Policy = "UserOrApprover")]
    public async Task<ActionResult<IEnumerable<AccessRequestDto>>> GetMine(Guid userId, CancellationToken ct)
        => Ok(await _mediator.Send(new GetUserRequestsQuery(userId), ct));
}
