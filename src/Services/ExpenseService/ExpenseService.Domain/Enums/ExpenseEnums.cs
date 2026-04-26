namespace ExpenseService.Domain.Enums;

public enum ExpenseCategory
{
    Travel = 1,
    Equipment = 2,
    Education = 3,
    Other = 4
}

public enum ExpenseCurrency
{
    TRY = 1,
    USD = 2,
    EUR = 3
}

public enum ExpenseStatus
{
    Draft = 1,
    Pending = 2,
    Approved = 3,
    Rejected = 4
}

public enum ApprovalStep
{
    HR = 1,
    Admin = 2
}

public enum ApprovalDecision
{
    Approved = 1,
    Rejected = 2
}
