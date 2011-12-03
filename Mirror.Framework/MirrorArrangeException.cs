using System;

namespace Mirror.Framework
{
    /// <summary>
    /// Exception that gets thrown when a call to Arrange() fails
    /// </summary>
    public class MirrorArrangeException : Exception
    {
        public MirrorArrangeException()
            : base()
        {
        }

        public MirrorArrangeException(string message)
            : base(message)
        {
        }

        public MirrorArrangeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
