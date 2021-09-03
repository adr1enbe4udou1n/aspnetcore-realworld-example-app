using System;

namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Resource not found")
        {
        }
    }
}