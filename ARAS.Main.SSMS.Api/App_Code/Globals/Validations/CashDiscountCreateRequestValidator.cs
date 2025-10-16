using ARAS.Main.SSMS.Api.App_Code.Globals.Constants;
using ARAS.Main.SSMS.Api.Models.Dtos;
using FluentValidation;

namespace ARAS.Main.SSMS.Api.App_Code.Globals.Validations
{
	public class CashDiscountCreateRequestValidator : AbstractValidator<CashDiscountCreateValidationDto>
	{
		public CashDiscountCreateRequestValidator() 
		{
			RuleFor(x => x.InvoiceNumber)
				.NotNull().WithMessage("Invoice Number is required.")
				.NotEmpty().WithMessage("Invoice Number is required.")
				.Matches(RegEx.INVOICE_NUMBER)
					.WithMessage("Invoice Number contains invalid characters. Only letters, numbers, dashes, underscores, and slashes are allowed.")
				.MaximumLength(30)
					.WithMessage("Invoice Number must not exceed 30 characters.");

			RuleFor(x => x.DiscountPercentage)
				.InclusiveBetween(0.000001, 100)
					.WithMessage("Discount must be between 0.01% and 100%.");

			RuleFor(x => x.Remarks)
				.NotEmpty().WithMessage("Remarks is required.")
				.MaximumLength(500).WithMessage("Remarks must not exceed 500 characters.");
		}
	}
}
