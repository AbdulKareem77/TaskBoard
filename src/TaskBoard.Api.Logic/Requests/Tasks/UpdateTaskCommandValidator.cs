using FluentValidation;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Todo", "InProgress", "Review", "Done"
    };

    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title cannot exceed 300 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.RowVersion)
            .GreaterThan(0).WithMessage("RowVersion must be greater than 0.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: Todo, InProgress, Review, Done.");
    }
}
