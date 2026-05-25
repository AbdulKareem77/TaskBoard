using FluentValidation;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class CreateTaskCommentCommandValidator : AbstractValidator<CreateTaskCommentCommand>
{
    public CreateTaskCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment cannot be empty.")
            .MaximumLength(4000).WithMessage("Comment cannot exceed 4000 characters.");
    }
}
