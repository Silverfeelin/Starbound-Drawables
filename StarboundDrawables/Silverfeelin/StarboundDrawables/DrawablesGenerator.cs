using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Silverfeelin.StarboundDrawables
{
    public class DrawablesGenerator
    {
        private Bitmap _image;
        /// <summary>
        /// Returns the path to the given image; the image to create drawables for.
        /// </summary>
        public Bitmap Image
        {
            get
            {
                return _image;
            }
        }

        /// <summary>
        /// Gets the path to the given image; or null if no path was given.
        /// </summary>
        public string ImagePath { get; private set; } = null;

        /// <summary>
        /// Gets or sets a color that will be ignored on the <see cref="Image"/> when generating drawables.
        /// No colors will be ignored if this value is null.
        /// </summary>
        public Color? IgnoreColor { get; set; } = null;

        /// <summary>
        /// Gets or sets a horizontal offset that will be applied to all Drawable parts of the output.
        /// The offset should be set in game pixels.
        /// </summary>
        public int OffsetX { get; set; } = 0;

        /// <summary>
        /// Gets or sets a vertical offset that will be applied to all Drawable parts of the output.
        /// The offset should be set in game pixels.
        /// </summary>
        public int OffsetY { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether 'blank' pixels should be replaced.
        /// Blank pixels are pixels with an alpha value of 0.
        /// This value should be true for single-texture directives, and false otherwise to conserve data usage.
        /// </summary>
        public bool ReplaceBlank { get; set; } = false;

        /// <summary>
        ///  Gets or sets a value indicating whether white ('#FFFFFFFF') pixels should be replaced with #FEFEFEFF.
        ///  Settings this value to true is recommended when generating drawables for a single texture.
        /// </summary>
        public bool ReplaceWhite { get; set; } = false;

        /// <summary>
        /// Gets or sets the RotateFlipStyle to apply to the <see cref="Image"/> before generating drawables.
        /// RotateNoneFlipY may be necessary to prevent upside down drawables, though it depends purely on where you're applying the drawables.
        /// </summary>
        public RotateFlipType RotateFlipStyle { get; set; } = RotateFlipType.RotateNoneFlipNone;

        /// <summary>
        /// Gets or sets the drawable texture to use for every <see cref="Drawable"/> in the output.
        /// You probably don't want to change this.
        /// </summary>
        public string DrawableTexture { get; set; } = "/objects/outpost/customsign/signplaceholder.png";

        /// <summary>
        /// Gets or sets the width of the <see cref="DrawableTexture"/> to use.
        /// You probably don't want to change this. Default is 32.
        /// </summary>
        public int DrawableWidth { get; set; } = 32;

        /// <summary>
        /// Gets or sets the height of the <see cref="DrawableTexture"/> to use.
        /// You probably don't want to change this. Default is 8.
        /// </summary>
        public int DrawableHeight { get; set; } = 8;

        /// <summary>
        /// Creates a generator for the given image. The image is cloned, so the resource is never locked.
        /// The instance can then be used to generate a <see cref="DrawablesOutput"/> for this image.
        /// </summary>
        /// <param name="imagePath">Full path to the image to create drawables for.</param>
        /// <exception cref="FileNotFoundException">Thrown when the given image file could not be found.</exception>
        /// <exception cref="ArgumentNullException">Thrown when no path was given.</exception>
        /// <exception cref="ArgumentException">Thrown when the given path is not valid.</exception>
        public DrawablesGenerator(string imagePath)
        {
            SetImage(imagePath);
        }

        /// <summary>
        /// Creates a generator for the given image.
        /// The instance can then be used to generate a <see cref="DrawablesOutput"/> for this image.
        /// This image is not disposed by this class if any exceptions occur.
        /// </summary>
        /// <param name="image">Image to generate drawables for</param>
        public DrawablesGenerator(Bitmap image)
        {
            SetImage(image);
        }

        public void SetImage(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);

            var ms = new MemoryStream(bytes);
            
            Bitmap b = (Bitmap)System.Drawing.Image.FromStream(ms);
                
            ImagePath = imagePath;
            _image = b;
        }

        public void SetImage(Bitmap bitmap)
        {
            _image = bitmap;
        }

        /// <summary>
        /// Generates a new <see cref="DrawablesOutput"/> for this <see cref="Image"/>, using all the set properties.
        /// </summary>
        /// <returns>DrawablesOutput containing the Drawables for this image.</returns>
        public DrawablesOutput Generate()
        {
            if (Image == null)
                throw new DrawableException("Attempted to generate drawables for a disposed image object.");

            Color? ignore = IgnoreColor;
            
            // Copy into 32bppARGB to read bytes correctly.
            Bitmap b = (Bitmap)Image.Clone(new Rectangle(0, 0, Image.Width, Image.Height), PixelFormat.Format32bppArgb);

            // Flip in desired direction.
            b.RotateFlip(RotateFlipStyle);

            // Copy data for reading. Much faster than GetPixel (thanks Degranon for making me put extra effort into this old code).
            BitmapData bData = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.ReadWrite, b.PixelFormat);
            byte[] bPixels = new byte[bData.Width * bData.Height * 4];
            Marshal.Copy(bData.Scan0, bPixels, 0, bPixels.Length);
            b.UnlockBits(bData);

            Point frameCount = new Point(
                (int)Math.Ceiling((decimal)b.Width / DrawableWidth),
                (int)Math.Ceiling((decimal)b.Height / DrawableHeight));

            Drawable[,] drawables = new Drawable[frameCount.X, frameCount.Y];

            Color[,] template = GetTemplate();

            int whiteArgb = Color.White.ToArgb();

            Point imagePixel = new Point(0, 0);

            // Add a drawable for every signplaceholder needed.
            for (int frameWidth = 0; frameWidth < frameCount.X; frameWidth++)
            {
                for (int frameHeight = 0; frameHeight < frameCount.Y; frameHeight++)
                {
                    imagePixel.X = frameWidth * DrawableWidth;
                    imagePixel.Y = frameHeight * DrawableHeight;

                    bool containsPixels = false;

                    StringBuilder directives = new StringBuilder("?replace");

                    for (int i = 0; i < DrawableWidth; i++)
                    {
                        for (int j = 0; j < DrawableHeight; j++)
                        {
                            int x = imagePixel.X,
                                y = imagePixel.Y++;

                            // Pixel falls within template but is outside of the supplied image.
                            if ((x > b.Width - 1 || y > b.Height - 1))
                            {
                                continue;
                            }

                            Color imageColor = GetPixel(bPixels, b.Width, Convert.ToInt32(x), Convert.ToInt32(y));

                            // Pixel color is invisible or ignored.
                            if ((ignore.HasValue && imageColor.Equals(ignore)) || (imageColor.A == 0 && !ReplaceBlank))
                            {
                                continue;
                            }
                            else if (ReplaceWhite && imageColor.ToArgb() == whiteArgb)
                            {
                                imageColor = Color.FromArgb(255, 254, 254, 254);
                            }

                            Color templateColor = template[i, j];

                            directives.AppendFormat(";{0}={1}", templateColor.ToRGBAHexString(), imageColor.ToRGBAHexString());

                            if (imageColor.A > 1)
                                containsPixels = true;
                        }

                        imagePixel.X++;
                        imagePixel.Y = frameHeight * DrawableHeight;
                    }

                    int xb = Convert.ToInt32(frameWidth * DrawableWidth),
                        yb = Convert.ToInt32(frameHeight * DrawableHeight);

                    if (containsPixels)
                        drawables[frameWidth, frameHeight] = new Drawable(directives.ToString(), xb, yb, DrawableTexture);
                }
            }

            return new DrawablesOutput(drawables, Image.Width, Image.Height, OffsetX, OffsetY);
        }

        private Color GetPixel(byte[] data, int width, int x, int y)
        {
            int offset = width * y * 4 + x * 4;
            return Color.FromArgb(
                data[offset + 3],
                data[offset + 2],
                data[offset + 1],
                data[offset]
                );
        }

        /// <summary>
        /// Returns a two dimensional array containing the Color data of the signplaceholder asset.
        /// The hexadecimal digits A/F are ignored in the template;
        /// decimal increment from 9 to 10 corresponds to the hexadecimal increment from 9 to 10.
        /// <see cref="DrawableWidth"/> <see cref="DrawableHeight"/>.
        /// </summary>
        /// <returns>Two dimensional color array.</returns>
        private Color[,] GetTemplate()
        {
            Color[,] template = new Color[DrawableWidth, DrawableHeight];

            for (int i = 0; i < DrawableWidth; i++)
            {
                for (int j = 0; j < DrawableHeight; j++)
                {
                    int xi = i, yj = j;

                    // Compensate for missing hexadecimal values (A/F).
                    for (int amnt = 0; amnt < (int)Math.Floor((i + 1) / 10.0); amnt++)
                        xi += 6;

                    for (int amnt = 0; amnt < (int)Math.Floor((j + 1) / 10.0); amnt++)
                        yj += 6;

                    template[i, j] = Color.FromArgb(1, xi + 1, 0, yj + 1);
                }
            }

            return template;
        }
    }
}
