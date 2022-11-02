using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanApp
{
    public class UnhandledApiErrorException : Exception
    {
        public UnhandledApiErrorException(string message) : base(message)
        {

        }
    }
}
