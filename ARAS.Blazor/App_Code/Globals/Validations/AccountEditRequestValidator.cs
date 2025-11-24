using ARAS.Blazor.App_Code.Globals.Constants;
using ARAS.Blazor.Models.DTOs;
using FluentValidation;

namespace ARAS.Blazor.App_Code.Globals.Validations
{
	public class AccountEditRequestValidator : AbstractValidator<AccountEditRequestDto>
	{
		public AccountEditRequestValidator()
		{
			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("First Name is Required")
				.NotNull().WithMessage("First Name is Required")
				.MaximumLength(100)
				.WithMessage("First Name not exceed 100 characters.")
				.Matches(RegEx.NAMES).WithMessage("First name contains invalid characters.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("Last Name is Required")
				.NotNull().WithMessage("Last Name is Required")
				.MaximumLength(100)
				.WithMessage("Email must not exceed 100 characters.")
				.Matches(RegEx.NAMES).WithMessage("Last name contains invalid characters.");

			RuleFor(x => x.AccountRole)
				.NotEmpty().WithMessage("Account Role is Required")
				.NotNull().WithMessage("Account Role is Required")
				.MaximumLength(50);

			RuleFor(x => x.GroupCode)
				.Matches(RegEx.GROUPCODE)
					.WithMessage("Group Code contains invalid characters.")
				.MaximumLength(5)
					.WithMessage("Group Code must not exceed 5 characters.")
				.When(x => !string.IsNullOrEmpty(x.GroupCode));
		}
	}
}
