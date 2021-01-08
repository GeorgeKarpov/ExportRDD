using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact
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
