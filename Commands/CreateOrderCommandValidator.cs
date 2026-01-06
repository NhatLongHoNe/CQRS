using FluentValidation;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().WithMessage("FirstName is required");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("LastName is required");
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required");
        RuleFor(x => x.TotalCost).GreaterThan(0).WithMessage("TotalCost must be greater than 0");
    }
}