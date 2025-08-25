using DocumentAccessApprovalSystem.Application.CQRS.Commands;
using DocumentAccessApprovalSystem.Application.CQRS.Handlers;
using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Notifications;
using DocumentAccessApprovalSystem.Domain.Entities;
using DocumentAccessApprovalSystem.Domain.Enums;
using DocumentAccessApprovalSystem.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace DocumentAccessApprovalSystem.Tests
{
    public class DecisionNotificationTests
    {
        [Test]
        public async Task Decide_should_publish_DecisionMadeNotification()
        {
            var (sp, sink) = BuildServicesWithSink();
            var db = sp.GetRequiredService<AppDbContext>();

            var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var docId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            db.Users.Add(new User { Id = userId, Name = "Alice" });
            db.Documents.Add(new Document { Id = docId, Name = "HR" });
            await db.SaveChangesAsync();

            var create = new CreateAccessRequestHandler(db);
            var created = await create.Handle(new CreateAccessRequestCommand(
                new CreateAccessRequestDto(userId, docId, "Need", AccessType.Read)), default);

            var publisher = sp.GetRequiredService<IPublisher>();
            var decide = new DecideAccessRequestHandler(db, publisher);

            await decide.Handle(new DecideAccessRequestCommand(
                created.Id,
                new DecideRequestDto(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), DecisionType.Approved, "OK")), default);

            sink.Count.Should().Be(1);
            var n = sink.Take();
            n.AccessRequestId.Should().Be(created.Id);
            n.UserId.Should().Be(userId);
            n.DocumentId.Should().Be(docId);
            n.DecisionType.Should().Be(DecisionType.Approved);
        }

        private static (ServiceProvider sp, BlockingCollection<DecisionMadeNotification> sink) BuildServicesWithSink()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DecisionMadeNotification).Assembly));

            var sink = new BlockingCollection<DecisionMadeNotification>();
            services.AddSingleton(sink);
            services.AddTransient<INotificationHandler<DecisionMadeNotification>, SinkNotificationHandler>();

            return (services.BuildServiceProvider(), sink);
        }

        private sealed class SinkNotificationHandler : INotificationHandler<DecisionMadeNotification>
        {
            private readonly BlockingCollection<DecisionMadeNotification> _sink;
            public SinkNotificationHandler(BlockingCollection<DecisionMadeNotification> sink) => _sink = sink;

            public Task Handle(DecisionMadeNotification notification, CancellationToken cancellationToken)
            {
                _sink.Add(notification, cancellationToken);
                return Task.CompletedTask;
            }
        }
    }
}
