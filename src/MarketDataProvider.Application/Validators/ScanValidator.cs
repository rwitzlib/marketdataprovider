using FluentValidation;
using MarketDataProvider.Contracts.Requests;

namespace MarketDataProvider.Application.Validators
{
    public class ScanValidator : AbstractValidator<ScanPopulateRequest>
    {
        public ScanValidator()
        {
            RuleFor(request => request.Timestamp).GreaterThan(DateTime.MinValue).LessThanOrEqualTo(DateTime.Now);
        }
    }
}
