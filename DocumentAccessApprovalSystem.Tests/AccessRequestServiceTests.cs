using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Services;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Tests
{
    public class AccessRequestServiceTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);

            db.Users.Add(new User { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Alice" });
            db.Documents.Add(new Document { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "HR" });
            db.SaveChanges();
            return db;
        }

        [Test]
        public async Task Approving_pending_request_sets_status_and_creates_decision()
        {
            // Arrange
            using var db = NewDb();
            var service = new AccessRequestService(db);
            var create = new CreateAccessRequestDto(
                UserId: Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                DocumentId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Reason: "Need to review",
                AccessType: AccessType.Read
            );
            var created = await service.CreateAsync(create);

            // Act
            var decided = await service.DecideAsync(created.Id,
                new DecideRequestDto(
                    ApproverId: Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                    Type: DecisionType.Approved,
                    Comment: "OK"
                ));

            // Assert
            decided.Status.Should().Be(RequestStatus.Approved.ToString());
            decided.Decision.Should().NotBeNull();
            decided.Decision!.Type.Should().Be(DecisionType.Approved);
            (await db.Decisions.CountAsync()).Should().Be(1);
        }
    }
}
