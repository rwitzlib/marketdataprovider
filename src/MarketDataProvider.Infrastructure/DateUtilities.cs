namespace MarketDataProvider.Infrastructure
{
    public static class DateUtilities
    {
        public static List<DateTime> GetMarketOpenDays(DateTimeOffset start, DateTimeOffset end)
        {
            var dates = new List<DateTime>();

            var days = (end.Date - start.Date).Days;

            for (int i = 0; i <= days; i++)
            {
                var currentDay = start.AddDays(i).Date;

                if (currentDay.DayOfWeek != DayOfWeek.Saturday && currentDay.DayOfWeek != DayOfWeek.Sunday)
                {
                    dates.Add(currentDay);
                }
            }

            return dates;
        }
    }
}
