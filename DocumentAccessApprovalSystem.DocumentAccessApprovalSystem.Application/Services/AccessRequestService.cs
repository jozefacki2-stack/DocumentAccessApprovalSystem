using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Interfaces;
using DocumentAccessApprovalSystem.Application.Mappers;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Application.Services
{
    public class AccessRequestService : IAccessRequestService
    {
        private readonly AppDbContext _db;
        public AccessRequestService(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<AccessRequestDto>> GetPendingAsync(CancellationToken ct = default)
        {
            var list = await _db.AccessRequests
                .Include(ar => ar.Decision)
                .Where(ar => ar.Status == RequestStatus.Pending)
                .OrderBy(ar => ar.CreatedAt)
                .ToListAsync(ct);

            return list.Select(AccessRequestMapper.Map).ToList();
        }


        public async Task<AccessRequestDto> DecideAsync(Guid requestId, DecideRequestDto dto, CancellationToken ct = default)
        {
            var req = await _db.AccessRequests
                .Include(ar => ar.Decision)
                .FirstOrDefaultAsync(ar => ar.Id == requestId, ct)
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

            // 👇 KLUCZOWA LINIA
            _db.Decisions.Add(decision);

            req.Decision = decision;
            req.Status = dto.Type == DecisionType.Approved ? RequestStatus.Approved : RequestStatus.Rejected;

            await _db.SaveChangesAsync(ct);
            return AccessRequestMapper.Map(req);
        }

        public async Task<IReadOnlyList<AccessRequestDto>> GetForUserAsync(Guid userId, CancellationToken ct = default)
        {
            var list = await _db.AccessRequests
                .Include(ar => ar.Decision)
                .Where(ar => ar.UserId == userId)
                .OrderByDescending(ar => ar.CreatedAt)
                .ToListAsync(ct);

            return list.Select(AccessRequestMapper.Map).ToList();
        }

        public async Task<AccessRequestDto> CreateAsync(CreateAccessRequestDto dto, CancellationToken ct = default)
        {
            // 1) Walidacja istnienia użytkownika i dokumentu
            var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId, ct);
            var docExists = await _db.Documents.AnyAsync(d => d.Id == dto.DocumentId, ct);
            if (!userExists || !docExists)
                throw new InvalidOperationException("User or Document not found.");

            // 2) Sprawdzenie czy nie ma już pending dla (User, Document, AccessType)
            var hasPending = await _db.AccessRequests.AnyAsync(ar =>
                ar.UserId == dto.UserId &&
                ar.DocumentId == dto.DocumentId &&
                ar.AccessType == dto.AccessType &&
                ar.Status == RequestStatus.Pending, ct);

            if (hasPending)
                throw new InvalidOperationException("There is already a pending request for this document and access type.");

            // 3) Utworzenie encji
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

            // 4) Zapis
            _db.AccessRequests.Add(entity);
            await _db.SaveChangesAsync(ct);

            // 5) Mapowanie na DTO
            return AccessRequestMapper.Map(entity);
        }
    }
}
