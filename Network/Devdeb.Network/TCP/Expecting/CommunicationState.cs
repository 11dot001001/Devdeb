using Devdeb.Serialization.Serializers.System;

namespace Devdeb.Network.TCP.Expecting
{
    internal class CommunicationState
    {
        public int ExpectingBytesCount { get; set; }

        public bool IsLengthReceived { get; set; }

        public CommunicationState()
        {
            ExpectingBytesCount = Int32Serializer.Default.Size;
        }
    }
}
