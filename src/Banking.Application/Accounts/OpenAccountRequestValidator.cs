using FluentValidation;

namespace Banking.Application.Accounts;

public class OpenAccountRequestValidator : AbstractValidator<OpenAccountRequest>
{
    public OpenAccountRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.InitialDeposit)
            .GreaterThanOrEqualTo(0).WithMessage("Initial credit cannot be negative.");
    }
}
