using FluentValidation;

namespace TaskBoard.Api.Logic.Requests.Tasks;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Todo", "InProgress", "Review", "Done"
    };

    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(300).WithMessage("Title cannot exceed 300 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Status)
            .Must(s => s == null || ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: Todo, InProgress, Review, Done.");
    }
}
