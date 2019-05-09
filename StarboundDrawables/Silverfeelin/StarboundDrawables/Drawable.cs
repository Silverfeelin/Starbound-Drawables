using System;

namespace Silverfeelin.StarboundDrawables
{
    /// <summary>
    /// Represents a drawable image with directives and position data.
    /// </summary>
    public class Drawable
    {
        /// <summary>
        /// Gets or sets the texture (image) of this drawable.
        /// Should be a valid asset path.
        /// Defaults to the signplaceholder asset.
        /// </summary>
        public string Texture { get; set; } = "/objects/outpost/customsign/signplaceholder.png";

        /// <summary>
        /// Gets or sets the directives for this Drawable.
        /// These directives are applied to the texture by calling <see cref="ResultImage"/>.
        /// </summary>
        public string Directives { get; set; } = string.Empty;

        /// <summary>
        /// Gets the <see cref="Texture"/> plus <see cref="Directives"/>, which form this Drawable.
        /// </summary>
        public string ResultImage => Texture + Directives;

        /// <summary>
        /// Gets or sets the horizontal position for this Drawable, in game pixels.
        /// </summary>
        public int X { get; set; } = 0;

        /// <summary>
        /// Gets or sets the vertical position for this Drawable, in game pixels.
        /// </summary>
        public int Y { get; set; } = 0;

        /// <summary>
        /// Gets or sets the horizontal position for this Drawable, in blocks.
        /// A block is 8 game pixels by default.
        /// </summary>
        public double BlockX
        {
            get => Math.Round(X / 8d, 3);
            set => X = Convert.ToInt32(value * 8d);
        }

        /// <summary>
        /// Gets or sets the vertical position for this Drawable, in blocks.
        /// A block is 8 game pixels by default.
        /// </summary>
        public double BlockY
        {
            get => Math.Round(Y / 8d, 3);
            set => Y = Convert.ToInt32(value * 8d);
        }

        /// <summary>
        /// Instantiates a Drawable using default values.
        /// Default values are:
        /// Texture: "/objects/outpost/customsign/signplaceholder.png"
        /// Directives: ""
        /// X: 0 Y: 0
        /// </summary>
        public Drawable() { }

        /// <summary>
        /// Instantiates a generated Drawable.
        /// </summary>
        /// <param name="directives">Directives to apply to the texture.</param>
        /// <param name="x">Horizontal position of the drawable, in game pixels.</param>
        /// <param name="y">Position of the drawable, in game pixels.</param>
        /// <param name="texture">Optional path to use as the <see cref="Texture"/>. Generally you'll want to leave this out.</param>
        public Drawable(string directives, int x, int y, string texture = null)
        {
            if (!string.IsNullOrWhiteSpace(texture))
                Texture = texture;

            Directives = directives;
            X = x;
            Y = y;
        }
    }
}
