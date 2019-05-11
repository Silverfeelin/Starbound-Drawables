using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace Silverfeelin.StarboundDrawables
{
    /// <summary>
    /// Class used to generate Starbound Drawables from an image.
    /// Each image should use a new instance of this class for drawable generation.
    /// </summary>
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

        /// <summary>
        /// Sets the image to generate drawables for. A valid image path is expected.
        /// </summary>
        /// <seealso cref="SetImage(Bitmap)"/>
        /// <param name="imagePath">File path to the image.</param>
        public void SetImage(string imagePath)
        {
            var bytes = File.ReadAllBytes(imagePath);
            var ms = new MemoryStream(bytes);
            Bitmap b = (Bitmap)System.Drawing.Image.FromStream(ms);

            ImagePath = imagePath;
            SetImage(b);
        }

        /// <summary>
        /// Sets the image to generate drawables for. The Bitmap is not cloned; a reference is set.
        /// Does not set ImagePath.
        /// </summary>
        /// <seealso cref="SetImage(string)"/>
        /// <param name="bitmap">Bitmap to generate drawables for.</param>
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

            var ignore = IgnoreColor;

            var b = (Bitmap)Image.Clone();
            b.RotateFlip(RotateFlipStyle);

            var frameCount = new Point(
                (int)Math.Ceiling((decimal)b.Width / DrawableWidth),
                (int)Math.Ceiling((decimal)b.Height / DrawableHeight));

            var drawables = new Drawable[frameCount.X, frameCount.Y];
            var template = GetTemplate();
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
                            if (x >= b.Width || y >= b.Height)
                                continue;

                            var imageColor = b.GetPixel(Convert.ToInt32(x), Convert.ToInt32(y));

                            // Pixel color is invisible or ignored.
                            if ((ignore.HasValue && imageColor.Equals(ignore)) || (imageColor.A == 0 && !ReplaceBlank))
                            {
                                continue;
                            }
                            else if (ReplaceWhite && imageColor.ToArgb() == whiteArgb)
                            {
                                imageColor = Color.FromArgb(255, 254, 254, 254);
                            }

                            var templateColor = template[i, j];

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

        /// <summary>
        /// Generates a new <see cref="DrawablesOutput"/> for this <see cref="Image"/>, using all the set properties.
        /// Uses a gradient scaling method.
        /// </summary>
        /// <returns>DrawablesOutput containing the Drawables for this image.</returns>
        public DrawablesOutput GenerateScale()
        {
            if (Image == null)
                throw new DrawableException("Attempted to generate drawables for a disposed image object.");

            var b = (Bitmap)Image.Clone();
            b.RotateFlip(RotateFlipStyle);

            const string template = "?setcolor=fff?replace;fff0=fff" +
                "?crop;0;0;2;2" +
                "?blendmult=/items/active/weapons/protectorate/aegisaltpistol/beamend.png;0;0" +
                "?replace;A355C0A5={BottomLeft};A355C07B={BottomRight};FFFFFFA5={TopLeft};FFFFFF7B={TopRight}" +
                "?scale={ScaleWidth};{ScaleHeight}" +
                "?crop;1;1;{Width};{Height}";

            var frameCount = new Point(
                (int)Math.Ceiling((decimal)b.Width / 256),
                (int)Math.Ceiling((decimal)b.Height / 256));

            var drawables = new Drawable[frameCount.X, frameCount.Y];
            var bottomLeft = Color.FromArgb(0, 0, 1, 0);

            var right = b.Width - 1;
            var top = b.Height - 1;

            var ignore = IgnoreColor;
            var whiteArgb = Color.White.ToArgb();
            var subWhite = Color.FromArgb(255, 254, 254, 254);

            for (int i = 0; i < frameCount.X; i++)
            {
                for (int j = 0; j < frameCount.Y; j++)
                {
                    var sb = new StringBuilder(template);

                    var r = Math.Min(right, 255);
                    var t = Math.Min(top, 255);

                    var bottomRight = Color.FromArgb(0, r, 1, 0);
                    var topLeft = Color.FromArgb(0, 0, 1, t);
                    var topRight = Color.FromArgb(0, r, 1, t);

                    sb.Replace("{BottomLeft}", bottomLeft.ToRGBAHexString());
                    sb.Replace("{BottomRight}", bottomRight.ToRGBAHexString());
                    sb.Replace("{TopLeft}", topLeft.ToRGBAHexString());
                    sb.Replace("{TopRight}", topRight.ToRGBAHexString());

                    sb.Replace("{Width}", (r + 2).ToString());
                    sb.Replace("{Height}", (t + 2).ToString());
                    sb.Replace("{ScaleWidth}", (r + 1).ToString());
                    sb.Replace("{ScaleHeight}", (t + 1).ToString());

                    sb.Append("?replace");

                    var xs = i * 256;
                    var ys = j * 256;

                    

                    for (int x = 0; x < r + 1; x++)
                    {
                        for (int y = 0; y < t + 1; y++)
                        {
                            if (xs + x > b.Width - 1 || ys + y > b.Height - 1) continue;

                            var color = b.GetPixel(xs + x, ys + y);
                            if (color.A <= 1) continue;


                            // Pixel color is invisible or ignored.
                            if ((ignore.HasValue && color.Equals(ignore)) || (color.A == 0 && !ReplaceBlank))
                                continue;

                            if (ReplaceWhite && color.ToArgb() == whiteArgb)
                                color = subWhite;

                            sb.AppendFormat(";{0:X2}01{1:X2}00={2}", x, y, color.ToRGBAHexString());
                        }
                    }

                    drawables[i, j] = new Drawable(sb.ToString(), xs, ys, "/assetMissing.png");

                    top -= 256;
                }
                top = b.Height - 1;
                right -= 256;
            }

            return new DrawablesOutput(drawables, Image.Width, Image.Height, OffsetX, OffsetY);
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
            var template = new Color[DrawableWidth, DrawableHeight];

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
