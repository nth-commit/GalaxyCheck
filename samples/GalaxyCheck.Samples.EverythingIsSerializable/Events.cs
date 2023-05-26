namespace GalaxyCheck.Samples.EverythingIsSerializable;

public static class Events
{
    public interface IEvent
    {
        string Type { get; }
    }

    public record InvoiceCreatedEvent(string SupplierId, string CustomerId, string InvoiceId) : IEvent
    {
        public string Type => "InvoiceCreatedEvent";
    }

    public record InvoiceUpdatedEvent(string SupplierId, string InvoiceId) : IEvent
    {
        public string Type => "InvoiceUpdatedEvent";
    }

    public record InvoicePaidEvent(string SupplierId, string InvoiceId, decimal PaidAmount) : IEvent
    {
        public string Type => "InvoicePaidEvent";
    }
}
