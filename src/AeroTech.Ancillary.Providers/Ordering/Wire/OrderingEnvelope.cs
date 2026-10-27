namespace AeroTech.Ancillary.Providers.Ordering.Wire
{
    public sealed class OrderingEnvelope<T>
    {
        public T? Data { get; set; }
    }
}
