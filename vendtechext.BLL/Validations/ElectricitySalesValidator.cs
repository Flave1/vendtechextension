using FluentValidation;
using vendtechext.BLL.DTO;

namespace vendtechext.BLL.Validations
{
    public class ElectricitySalesValidator: AbstractValidator<ElectricitySaleRequest>
    {
        public ElectricitySalesValidator()
        {
            RuleFor(user => user.Amount)
            .NotNull().WithMessage("Amount cannot be null")
            .NotEmpty().WithMessage("Amount is required.");

            RuleFor(user => user.MeterNumber)
                .NotNull().WithMessage("Meter Number cannot be null")
                .NotEmpty().WithMessage("Meter Number is required.")
                .Length(11).WithMessage("Meter Number must be 11 Numbers.");

            RuleFor(user => user.TransactionId)
                .NotNull().WithMessage("Transaction Id cannot be null")
                .NotEmpty().WithMessage("Transaction Id is required.");
        }
    }
}
