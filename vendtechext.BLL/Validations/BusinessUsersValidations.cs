using FluentValidation;
using vendtechext.Contracts;

public class IntegratorValidator : AbstractValidator<BusinessUserCommandDTO>
{
    public IntegratorValidator()
    {
        RuleFor(user => user.FirstName)
            .NotNull().WithMessage("ReceivedFrom cannot be null")
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");

        RuleFor(user => user.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

        RuleFor(user => user.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\d{8}$").WithMessage("Phone number must be exactly 8 digits.");

        RuleFor(user => user.BusinessName)
            .NotEmpty().WithMessage("Business name is required.")
            .MaximumLength(400).WithMessage("Business name must not exceed 400 characters.");

        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(200).WithMessage("Email must not exceed 200 characters.");

        //RuleFor(user => user.Clientkey)
        //    .NotEmpty().WithMessage("Client key is required.");

        //RuleFor(user => user.ApiKey)
        //    .NotEmpty().WithMessage("API key is required.");
    }
}
