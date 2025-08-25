using DocumentAccessApprovalSystem.Application.CQRS.Handlers;
using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Tests
{
    public class CreateAccessRequestHandlerTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);
            db.Users.Add(new User { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Alice" });
            db.Documents.Add(new Document { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "HR" });
            db.SaveChanges();
            return db;
        }

        [Test]
        public async Task Creates_pending_request()
        {
            using var db = NewDb();
            var handler = new CreateAccessRequestHandler(db);

            var dto = new CreateAccessRequestDto(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Need to review",
                AccessType.Read
            );

            var result = await handler.Handle(new CreateAccessRequestCommand(dto), default);

            result.Status.Should().Be(RequestStatus.Pending.ToString());
            (await db.AccessRequests.CountAsync()).Should().Be(1);
        }
    }
}
