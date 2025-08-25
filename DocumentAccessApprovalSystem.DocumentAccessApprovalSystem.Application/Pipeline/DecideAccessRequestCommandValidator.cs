using DocumentAccessApprovalSystem.Application.CQRS.Commands;
using FluentValidation;

namespace DocumentAccessApprovalSystem.Application.Pipeline
{
    public class DecideAccessRequestCommandValidator : AbstractValidator<DecideAccessRequestCommand>
    {
        public DecideAccessRequestCommandValidator()
        {
            RuleFor(x => x.RequestId).NotEmpty();
            RuleFor(x => x.Dto.ApproverId).NotEmpty();
            RuleFor(x => x.Dto.Type).IsInEnum();
            RuleFor(x => x.Dto.Comment).MaximumLength(1000);
        }
    }
}
