using System;

namespace Mirror.Framework
{
    public class MirrorCreationException : Exception
    {
        public MirrorCreationException()
            : base()
        {
        }

        public MirrorCreationException(string message)
            : base(message)
        {
        }
        
        public MirrorCreationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
