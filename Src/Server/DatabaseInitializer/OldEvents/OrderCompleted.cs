using Infrastructure.EventSourcing;

namespace DatabaseInitializer.OldEvents
{
    public class OrderCompleted : VersionedEvent
    {
        public double? Fare { get; set; }
        public double? Toll { get; set; }
        public double? Tip { get; set; }
        public double? Tax { get; set; }
    }
}