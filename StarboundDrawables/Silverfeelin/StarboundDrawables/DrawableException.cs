using System;

namespace Silverfeelin.StarboundDrawables
{
    /// <summary>
    /// Exception class used to raise exceptions specifically related to Drawables and the generation thereof.
    /// </summary>
    [Serializable]
    public class DrawableException : Exception
    {
        public DrawableException() {}

        public DrawableException(string message) : base(message) {}
    }
}
