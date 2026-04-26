using ExpenseService.Application.DTOs;
using ExpenseService.Domain.Enums;
using FluentValidation;

namespace ExpenseService.Application.Validators;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).Must(BeValidRole).WithMessage("Invalid role.");
    }

    private static bool BeValidRole(string role)
    {
        return role is Roles.Admin or Roles.HR or Roles.Personnel;
    }
}

public sealed class UpdateUserRolesRequestValidator : AbstractValidator<UpdateUserRolesRequest>
{
    public UpdateUserRolesRequestValidator()
    {
        RuleFor(x => x.Roles).NotEmpty();
        RuleForEach(x => x.Roles).Must(BeValidRole).WithMessage("Invalid role.");
    }

    private static bool BeValidRole(string role)
    {
        return role is Roles.Admin or Roles.HR or Roles.Personnel;
    }
}
