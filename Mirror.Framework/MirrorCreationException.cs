using System;

namespace Mirror.Framework
{
    /// <summary>
    /// Exception that gets thrown when the creation of a mirror fails
    /// </summary>
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
