namespace Si.CoreHub.EventBus.Entitys
{
    public class Subscription : IDisposable
    {
        private readonly Action _unsubscribeAction;

        public Subscription(Action unsubscribeAction)
        {
            _unsubscribeAction = unsubscribeAction;
        }

        public void Dispose()
        {
            _unsubscribeAction();
        }
    }
}
