using DocumentAccessApprovalSystem.Application.CQRS.Queries;
using FluentValidation;

namespace DocumentAccessApprovalSystem.Application.Pipeline
{
    public class CreateAccessRequestCommandValidator : AbstractValidator<CreateAccessRequestCommand>
    {
        public CreateAccessRequestCommandValidator()
        {
            RuleFor(x => x.Dto.UserId).NotEmpty();
            RuleFor(x => x.Dto.DocumentId).NotEmpty();
            RuleFor(x => x.Dto.Reason).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Dto.AccessType).IsInEnum();
        }
    }
}
