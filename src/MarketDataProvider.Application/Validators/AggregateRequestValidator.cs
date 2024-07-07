using FluentValidation;
using MarketDataProvider.Contracts.Requests;

namespace MarketDataProvider.Application.Validators
{
    public class AggregateRequestValidator : AbstractValidator<AggregateRequest>
    {
        public AggregateRequestValidator()
        {
            RuleFor(param => param.Ticker).NotNull().MinimumLength(1).MaximumLength(6).Matches("^[A-Za-z]+$");
            RuleFor(param => param.Multiplier).GreaterThanOrEqualTo(1).LessThanOrEqualTo(15);
            RuleFor(param => param.From).GreaterThan(DateTimeOffset.MinValue).LessThanOrEqualTo(param => param.To);
            RuleFor(param => param.To).GreaterThanOrEqualTo(param => param.From);
            RuleFor(param => param.To.Date).LessThanOrEqualTo(DateTime.Now.Date);
        }
    }
}
