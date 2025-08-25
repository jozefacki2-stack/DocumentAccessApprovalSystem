using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Mappers;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Application.CQRS.Handlers
{
    public class CreateAccessRequestHandler : IRequestHandler<CreateAccessRequestCommand, AccessRequestDto>
    {
        private readonly AppDbContext _db;
        public CreateAccessRequestHandler(AppDbContext db) => _db = db;

        public async Task<AccessRequestDto> Handle(CreateAccessRequestCommand request, CancellationToken ct)
        {
            var dto = request.Dto;

            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId, ct);
            var docExists = await _db.Documents.AnyAsync(d => d.Id == dto.DocumentId, ct);
            if (!userExists || !docExists)
                throw new InvalidOperationException("User or Document not found.");

            var hasPending = await _db.AccessRequests.AnyAsync(ar =>
                ar.UserId == dto.UserId &&
                ar.DocumentId == dto.DocumentId &&
                ar.AccessType == dto.AccessType &&
                ar.Status == RequestStatus.Pending, ct);
            if (hasPending)
                throw new InvalidOperationException("There is already a pending request for this document and access type.");

            var entity = new AccessRequest
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                DocumentId = dto.DocumentId,
                Reason = dto.Reason,
                AccessType = dto.AccessType,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = RequestStatus.Pending
            };

            _db.AccessRequests.Add(entity);
            await _db.SaveChangesAsync(ct);
            return AccessRequestMapper.Map(entity);
        }
    }
}
