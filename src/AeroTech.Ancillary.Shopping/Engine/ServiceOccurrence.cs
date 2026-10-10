namespace AeroTech.Ancillary.Shopping.Engine
{
    internal readonly record struct ServiceOccurrence(DateTimeOffset Instant, DateTime Local, TimeZoneInfo Zone)
    {
        public DateOnly LocalDate => DateOnly.FromDateTime(Local);

        public static bool TryResolve(DateTimeOffset instant, string? ianaTimeZoneId, out ServiceOccurrence occurrence)
        {
            occurrence = default;

            if (string.IsNullOrWhiteSpace(ianaTimeZoneId)
                || !TimeZoneInfo.TryFindSystemTimeZoneById(ianaTimeZoneId, out var zone)
                || !zone.HasIanaId)
                return false;

            occurrence = new ServiceOccurrence(instant, TimeZoneInfo.ConvertTime(instant, zone).DateTime, zone);

            return true;
        }

        public static int CompletedYears(DateOnly dateOfBirth, DateOnly at)
        {
            var years = at.Year - dateOfBirth.Year;

            return dateOfBirth.AddYears(years) > at ? years - 1 : years;
        }

        public static int CompletedMonths(DateOnly dateOfBirth, DateOnly at)
        {
            var months = ((at.Year - dateOfBirth.Year) * 12) + at.Month - dateOfBirth.Month;

            return dateOfBirth.AddMonths(months) > at ? months - 1 : months;
        }
    }
}
