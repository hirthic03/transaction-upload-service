using FluentValidation;
using System.Globalization;
using Transactions.Domain.Models;

namespace Transactions.Domain.Validators;

public class TransactionValidator : AbstractValidator<TransactionRecord>
{
    private static readonly HashSet<string> ValidCurrencyCodes = new()
    {
        "USD", "EUR", "GBP", "JPY", "CHF", "CAD", "AUD", "NZD", "CNY", "INR",
        "KRW", "SGD", "HKD", "NOK", "SEK", "DKK", "PLN", "CZK", "HUF", "RON",
        "BGN", "HRK", "RUB", "TRY", "BRL", "ZAR", "MXN", "IDR", "MYR", "PHP",
        "THB", "VND", "EGP", "NGN", "AED", "SAR", "QAR", "KWD", "BHD", "OMR"
    };

    private static readonly HashSet<string> ValidStatuses = new()
    {
        "Approved", "Failed", "Finished", "Rejected", "Done"
    };

    public TransactionValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Transaction ID is required")
            .MaximumLength(50).WithMessage("Transaction ID must not exceed 50 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required")
            .Length(3).WithMessage("Currency code must be 3 characters")
            .Must(BeValidCurrencyCode).WithMessage("Invalid ISO4217 currency code");

        RuleFor(x => x.TransactionDate)
            .NotEqual(DateTime.MinValue).WithMessage("Valid transaction date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1)).WithMessage("Transaction date cannot be in the future");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidStatus).WithMessage("Invalid status. Must be one of: Approved, Failed, Finished, Rejected, Done");
    }

    private bool BeValidCurrencyCode(string currencyCode)
    {
        return ValidCurrencyCodes.Contains(currencyCode?.ToUpperInvariant() ?? string.Empty);
    }

    private bool BeValidStatus(string status)
    {
        return ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase);
    }
}