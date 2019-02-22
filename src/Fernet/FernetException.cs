using System;

namespace Fernet
{
    public class FernetException : Exception
    {
        public FernetException(string message) : base(message)
        {
        }

        public FernetException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
    }
}
