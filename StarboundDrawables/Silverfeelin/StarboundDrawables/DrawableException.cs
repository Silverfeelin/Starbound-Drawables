using System;

namespace Silverfeelin.StarboundDrawables
{
    /// <summary>
    /// Represents errors generating drawables.
    /// </summary>
    [Serializable]
    public class DrawableException : Exception
    {
        /// <summary>
        /// Instantiates a new drawable exception.
        /// </summary>
        public DrawableException() {}

        /// <summary>
        /// Instantiates a new drawabrle exception with a message.
        /// </summary>
        public DrawableException(string message) : base(message) {}
    }
}
