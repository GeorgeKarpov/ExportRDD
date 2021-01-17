using System;

namespace ExpRddApp
{
    public class WrongArgUsageException : Exception
    {
        public WrongArgUsageException()
        {
        }

        public WrongArgUsageException(string message)
            : base(message)
        {
        }

        public WrongArgUsageException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
