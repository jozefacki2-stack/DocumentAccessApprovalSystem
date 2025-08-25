using DocumentAccessApprovalSystem.Application.CQRS.Commands;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Mappers;
using DocumentAccessApprovalSystem.Application.Notifications;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Application.CQRS.Handlers
{
    public class DecideAccessRequestHandler : IRequestHandler<DecideAccessRequestCommand, AccessRequestDto>
    {
        private readonly AppDbContext _db;
        private readonly IPublisher _publisher;
        public DecideAccessRequestHandler(AppDbContext db, IPublisher publisher) // <— wstrzyknij
        {
            _db = db; _publisher = publisher;
        }

        public async Task<AccessRequestDto> Handle(DecideAccessRequestCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var req = await _db.AccessRequests
                .Include(ar => ar.Decision)
                .FirstOrDefaultAsync(ar => ar.Id == request.RequestId, ct)
                ?? throw new KeyNotFoundException("AccessRequest not found");

            if (req.Status != RequestStatus.Pending)
                throw new InvalidOperationException("Only pending requests can be decided.");

            var decision = new Decision
            {
                Id = Guid.NewGuid(),
                AccessRequestId = req.Id,
                ApproverId = dto.ApproverId,
                Type = dto.Type,
                Comment = dto.Comment,
                DecidedAt = DateTimeOffset.UtcNow
            };

            _db.Decisions.Add(decision);

            req.Decision = decision;
            req.Status = dto.Type == DecisionType.Approved ? RequestStatus.Approved : RequestStatus.Rejected;

            await _db.SaveChangesAsync(ct);

            await _publisher.Publish(new DecisionMadeNotification(
                AccessRequestId: req.Id,
                UserId: req.UserId,
                DocumentId: req.DocumentId,
                DecisionType: decision.Type,
                Comment: decision.Comment,
                DecidedAt: decision.DecidedAt
            ), ct);

            return AccessRequestMapper.Map(req);
        }
    }
}
