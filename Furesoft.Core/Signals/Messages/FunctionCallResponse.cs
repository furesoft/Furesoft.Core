namespace Furesoft.Core.Signals.Messages
{
    internal class FunctionCallResponse
    {
        public string ErrorMessage { get; set; }
        public int ID { get; set; }
        public byte[] ReturnValue { get; set; }
    }
}