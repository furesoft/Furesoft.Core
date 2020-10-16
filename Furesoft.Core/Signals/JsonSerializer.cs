using Newtonsoft.Json;
using System;
using System.Text;

namespace Furesoft.Core.Signals
{
    public class JsonSerializer : ISerializer
    {
        public object Deserialize(byte[] raw, Type type)
        {
            return JsonConvert.DeserializeObject(Encoding.ASCII.GetString(raw), type);
        }

        public byte[] Serialize(object obj)
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj));
        }
    }
}