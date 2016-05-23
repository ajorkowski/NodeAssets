using System;
using System.Runtime.Serialization;

namespace NodeAssets.Compilers
{
    [Serializable]
    public class CompileException : Exception
    {
        public CompileException(string message) : base(message) { }
        public CompileException(string message, Exception inner) : base(message, inner) { }

        // For deserialization
        protected CompileException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }
    }
}
