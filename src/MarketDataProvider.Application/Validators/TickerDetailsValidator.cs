using FluentValidation;

namespace MarketDataProvider.Application.Validators
{
    public class TickerDetailsValidator : AbstractValidator<string>
    {
        public TickerDetailsValidator()
        {
            RuleFor(ticker => ticker).NotNull().MinimumLength(1).MaximumLength(6).Matches("^[A-Za-z]+$");
        }
    }
}
