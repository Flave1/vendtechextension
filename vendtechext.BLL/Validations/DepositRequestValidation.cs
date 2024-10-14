using FluentValidation;
using vendtechext.Contracts;

namespace vendtechext.BLL.Validations
{
    public class DepositRequestValidator : AbstractValidator<DepositRequest>
    {
        public DepositRequestValidator()
        {
            RuleFor(user => user.Amount)
                .NotNull().WithMessage("Amount cannot be null")
                .NotEmpty().WithMessage("Amount is required.");

            RuleFor(user => user.Reference)
                .NotEmpty().WithMessage("Reference is required.");

        }
    }
}
