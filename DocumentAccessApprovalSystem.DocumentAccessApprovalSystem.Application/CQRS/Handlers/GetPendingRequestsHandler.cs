using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Mappers;
using DocumentAccessApprovalSystem.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Application.CQRS.Handlers
{
    public class GetPendingRequestsHandler : IRequestHandler<GetPendingRequestsQuery, IReadOnlyList<AccessRequestDto>>
    {
        private readonly AppDbContext _db;
        public GetPendingRequestsHandler(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<AccessRequestDto>> Handle(GetPendingRequestsQuery request, CancellationToken ct)
        {
            var list = await _db.AccessRequests
                .Include(ar => ar.Decision)
                .Where(ar => ar.Status == DocumentAccessApprovalSystem.Domain.Enums.RequestStatus.Pending)
                .OrderBy(ar => ar.CreatedAt)
                .ToListAsync(ct);

            return list.Select(AccessRequestMapper.Map).ToList();
        }
    }
}
