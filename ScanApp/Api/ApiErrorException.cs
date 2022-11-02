using System;

namespace ScanApp
{
    public class ApiErrorException : Exception
    {
        public ApiErrorException(string message) : base(message)
        {

        }
    }
}
