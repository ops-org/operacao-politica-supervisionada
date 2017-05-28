using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OPS.Core
{
    public class BusinessException : Exception
    {
        public BusinessException()
        {
        }

        public BusinessException(string message)
        : base(message)
        {
        }

        public BusinessException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
