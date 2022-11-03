using System;

namespace PointsChecker
{
    public class ApiErrorException : Exception
    {
        public ApiErrorException(string message) : base(message)
        {

        }
    }
}
