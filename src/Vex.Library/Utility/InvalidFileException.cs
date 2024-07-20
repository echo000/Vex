using System;

namespace Vex.Library
{
    public class InvalidFileException : Exception
    {
        public InvalidFileException()
        {
        }

        public InvalidFileException(string message) : base(message)
        {
        }

        public InvalidFileException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
