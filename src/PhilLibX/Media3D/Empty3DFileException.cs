using System;

namespace PhilLibX.Media3D.Translator
{
    /// <summary>
    /// An exception thrown if an empty file is found.
    /// </summary>
    public class Empty3DFileException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Empty3DFileException"/> class.
        /// </summary>
        public Empty3DFileException() : base() { }
    }
}
