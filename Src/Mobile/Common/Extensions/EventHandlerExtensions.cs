namespace System
{
    public static class EventHandlerExtensions
    {
        public static void Raise<T>(this EventHandler<T> eh, object sender, T args) where T:EventArgs
        {
            if (eh != null)
            {
                eh.Invoke(sender, args);
            }
        }

        public static void Raise(this EventHandler eh, object sender, EventArgs args)
        {
            if (eh != null)
            {
                eh.Invoke(sender, args);
            }
        }
    }
}

