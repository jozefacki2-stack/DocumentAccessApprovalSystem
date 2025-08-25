using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using DocumentAccessApprovalSystem.Application.DTOs;
using DocumentAccessApprovalSystem.Application.Pipeline;
using DocumentAccessApprovalSystem.Domain.Enums;
using FluentAssertions;
using FluentValidation;

namespace DocumentAccessApprovalSystem.Tests
{
    public class ValidationTests
    {
        [Test]
        public void CreateAccessRequest_should_fail_when_reason_empty()
        {
            var v = new CreateAccessRequestCommandValidator();
            var cmd = new CreateAccessRequestCommand(
                new CreateAccessRequestDto(Guid.NewGuid(), Guid.NewGuid(), "", AccessType.Read));
            Action act = () => v.ValidateAndThrow(cmd);
            act.Should().Throw<ValidationException>();
        }
    }
}
