// Copyright 2011 Marty Dill
// See License.txt for details

using System;

namespace Mirror.Framework
{
    /// <summary>
    /// Exception that gets thrown when a call to Returns, Throws, or Calls fails
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
