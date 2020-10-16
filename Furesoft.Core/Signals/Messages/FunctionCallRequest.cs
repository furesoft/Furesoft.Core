namespace Furesoft.Core.Signals.Messages
{
    internal class FunctionCallRequest
    {
        public int ID { get; set; }
        public object[] Parameter { get; set; }
        public byte[][] ParameterRaw { get; set; }
    }
}