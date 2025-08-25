using DocumentAccessApprovalSystem.Application.CQRS.Commands;
using DocumentAccessApprovalSystem.Application.CQRS.Handlers;
using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DocumentAccessApprovalSystem.Tests
{
    public class AccessRequestHandlersTests
    {
        private static AppDbContext NewDb()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var db = new AppDbContext(options);

            db.Users.AddRange(
                new User { Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Name = "Alice" },
                new User { Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Name = "Bob" }
            );
            db.Documents.AddRange(
                new Document { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "HR" },
                new Document { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Fin" }
            );
            db.SaveChanges();
            return db;
        }

        [Test]
        public async Task Approve_sets_status_and_creates_decision()
        {
            using var db = NewDb();
            var create = new CreateAccessRequestHandler(db);
            var decide = new DecideAccessRequestHandler(db, new MediatR.NoMediatorPublisher()); // patrz klasa pomocnicza poniżej

            var created = await create.Handle(new CreateAccessRequestCommand(
                new CreateAccessRequestDto(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "Need it", AccessType.Read
                )), default);

            var result = await decide.Handle(new DecideAccessRequestCommand(
                created.Id,
                new DecideRequestDto(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), DecisionType.Approved, "OK")
            ), default);

            result.Status.Should().Be(RequestStatus.Approved.ToString());
            result.Decision.Should().NotBeNull();
            (await db.Decisions.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task Reject_sets_status_and_creates_decision()
        {
            using var db = NewDb();
            var create = new CreateAccessRequestHandler(db);
            var decide = new DecideAccessRequestHandler(db, new MediatR.NoMediatorPublisher());

            var created = await create.Handle(new CreateAccessRequestCommand(
                new CreateAccessRequestDto(
                    Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    "No longer needed", AccessType.Edit
                )), default);

            var result = await decide.Handle(new DecideAccessRequestCommand(
                created.Id,
                new DecideRequestDto(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), DecisionType.Rejected, "Not now")
            ), default);

            result.Status.Should().Be(RequestStatus.Rejected.ToString());
            result.Decision.Should().NotBeNull();
            (await db.Decisions.CountAsync()).Should().Be(1);
        }

        [Test]
        public async Task Creating_duplicate_pending_request_is_blocked()
        {
            using var db = NewDb();
            var create = new CreateAccessRequestHandler(db);

            var dto = new CreateAccessRequestDto(
                Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Need it", AccessType.Read
            );

            await create.Handle(new CreateAccessRequestCommand(dto), default);

            var act = async () => await create.Handle(new CreateAccessRequestCommand(dto), default);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*pending request*");
        }

        [Test]
        public async Task GetPending_returns_only_pending_in_creation_order()
        {
            using var db = NewDb();
            var create = new CreateAccessRequestHandler(db);
            var getPending = new GetPendingRequestsHandler(db);
            var decide = new DecideAccessRequestHandler(db, new MediatR.NoMediatorPublisher());

            var alice = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var doc1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var doc2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var r1 = await create.Handle(new CreateAccessRequestCommand(new CreateAccessRequestDto(alice, doc1, "R1", AccessType.Read)), default);
            var r2 = await create.Handle(new CreateAccessRequestCommand(new CreateAccessRequestDto(alice, doc2, "R2", AccessType.Edit)), default);

            await decide.Handle(new DecideAccessRequestCommand(r1.Id,
                new DecideRequestDto(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), DecisionType.Approved, "OK")), default);

            var pending = await getPending.Handle(new GetPendingRequestsQuery(), default);

            pending.Should().HaveCount(1);
            pending.Single().Id.Should().Be(r2.Id);
            pending.Single().Status.Should().Be(RequestStatus.Pending.ToString());
        }
    }

    namespace MediatR
    {
        public sealed class NoMediatorPublisher : IPublisher
        {
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
                where TNotification : INotification => Task.CompletedTask;
        }
    }
}
