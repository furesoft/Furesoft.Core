namespace Furesoft.Core.ExpressionEvaluator
{


    public class Channel
    {
        public string Name { get; set; }

        public Action<object> OnReceive;

        public void Send<T>(T value)
        {
            OnReceive?.Invoke(value);
        }

        public void Subscribe(Action<object> callback)
        {
            OnReceive += callback;
        }
    }
}