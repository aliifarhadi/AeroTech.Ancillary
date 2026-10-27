namespace AeroTech.Ancillary.Providers._Shared
{
    public static class WireEnum
    {
        public static TEnum? Named<TEnum>(string? name)
            where TEnum : struct, Enum
            => name is not null && Enum.GetNames<TEnum>().Contains(name, StringComparer.Ordinal) ? Enum.Parse<TEnum>(name) : null;
    }
}
