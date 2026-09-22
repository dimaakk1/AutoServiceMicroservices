namespace AutoserviceNotification
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
