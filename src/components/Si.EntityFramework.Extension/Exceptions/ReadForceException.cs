using System.Runtime.Serialization;

namespace Si.EntityFramework.Extension.Exceptions
{
    public class ReadForceException : Exception
    {

        public ReadForceException()
        {
        }

        public ReadForceException(string message)
            : base(message)
        {
        }

        public ReadForceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ReadForceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
