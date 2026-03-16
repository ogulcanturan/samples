using System.Runtime.Serialization;

namespace Wcf.Abstractions
{
    [DataContract]
    public class CompositeType
    {
        [DataMember] 
        public bool BoolValue { get; set; } = true;

        [DataMember] 
        public string StringValue { get; set; } = "Hello ";
    }
}