using System;
using System.Runtime.Serialization;

namespace S16Editor.domain
{
    [Serializable]
    internal class CrcException : Exception
    {
        public CrcException()
        {
        }

        public CrcException(string message) : base(message)
        {
        }

        public CrcException(string message, Exception innerException) : base(message, innerException)
        {
        }


        protected CrcException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}