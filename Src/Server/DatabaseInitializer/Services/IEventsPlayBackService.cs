namespace DatabaseInitializer.Services
{
    public interface IEventsPlayBackService
    {
        int CountEvent(string aggregateType);
        void ReplayAllEvents();
    }
}