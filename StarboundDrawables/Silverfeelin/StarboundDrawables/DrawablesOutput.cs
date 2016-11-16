using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silverfeelin.StarboundDrawables
{
    public class DrawablesOutput
    {
        public Drawable[,] Drawables { get; }
        public string ImagePath { get; }
        public int ImageWidth { get; }
        public int ImageHeight { get; }
        public int OffsetX { get; }
        public int OffsetY { get; }
        
        public DrawablesOutput(Drawable[,] drawables) : this(drawables, drawables.GetLength(0) * 32, drawables.GetLength(1) * 8, 0, 0) { }

        public DrawablesOutput(Drawable[,] drawables, int imageWidth, int imageHeight, int offsetX, int offsetY, string imagePath = null)
        {
            Drawables = drawables;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            OffsetX = offsetX;
            OffsetY = offsetY;
            ImagePath = imagePath;
        }
    }
}
