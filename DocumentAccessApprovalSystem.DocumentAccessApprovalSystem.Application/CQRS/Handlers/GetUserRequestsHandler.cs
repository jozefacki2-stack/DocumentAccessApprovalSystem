using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Mappers;
using DocumentAccessApprovalSystem.Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Application.CQRS.Handlers
{
    public class GetUserRequestsHandler : IRequestHandler<GetUserRequestsQuery, IReadOnlyList<AccessRequestDto>>
    {
        private readonly AppDbContext _db;
        public GetUserRequestsHandler(AppDbContext db) => _db = db;

        public async Task<IReadOnlyList<AccessRequestDto>> Handle(GetUserRequestsQuery request, CancellationToken ct)
        {
            var list = await _db.AccessRequests
                .Include(ar => ar.Decision)
                .Where(ar => ar.UserId == request.UserId)
                .OrderByDescending(ar => ar.CreatedAt)
                .ToListAsync(ct);

            return list.Select(AccessRequestMapper.Map).ToList();
        }
    }
}
