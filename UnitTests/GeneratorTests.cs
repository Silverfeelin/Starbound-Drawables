using System;
using System.IO;
using System.Drawing;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Silverfeelin.StarboundDrawables;

namespace UnitTests
{
    [TestClass]
    public class GeneratorUnitTest
    {
        [TestMethod]
        public void TestNullGenerator()
        {
            try
            {
                DrawablesGenerator dg = new DrawablesGenerator((string)null);
            }
            catch (ArgumentException) { }
            catch (FileNotFoundException) { }
            catch (Exception)
            {
                Assert.Fail();
            }
        }

        private string CreateTempBitmap(int w = 32, int h = 8)
        {
            string path = Path.GetTempFileName();
            Bitmap bmp = new Bitmap(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int r = i + 1, b = j + 1;
                    Color c = Color.FromArgb(255, r < 256 ? r : 255, 0, b < 256 ? b : 0);
                    bmp.SetPixel(i, j, c);
                }
            }
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            bmp.Dispose();

            return path;
        }

        [TestMethod]
        public void TestValidGenerator()
        {
            string path = CreateTempBitmap();

            try
            {
                DrawablesGenerator dg = new DrawablesGenerator(path);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }

        }

        [TestMethod]
        public void TestGenerate()
        {
            int w = 4, h = 4;
            string path = CreateTempBitmap(w * 32, h * 8);

            try
            {
                DrawablesGenerator dg = new DrawablesGenerator(path);
                DrawablesOutput dOutput = dg.Generate();

                for (int i = 0; i < w; i++)
                {
                    for (int j = 0; j < h; j++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            for (int y = 0; y < h; y++)
                            {
                                Drawable drawable = dOutput.Drawables[i, j];
                                StringBuilder debuilder = new StringBuilder(drawable.Directives);
                                debuilder.Remove(0, 8);

                                while (debuilder.Length >= 18)
                                {
                                    string from = debuilder.ToString(1, 6),
                                        to = debuilder.ToString(10, 6);

                                    int rFrom = Convert.ToInt32(from.Substring(0, 2)),
                                        bFrom = Convert.ToInt32(from.Substring(4, 2));

                                    int rTo = ColorConversions.HexToInt(to.Substring(0, 2)),
                                        bTo = ColorConversions.HexToInt(to.Substring(4, 2));

                                    if (rTo > 32)
                                    {
                                        rTo = --rTo % 32;
                                        rTo++;
                                    }

                                    if (bTo > 8)
                                    {
                                        bTo = --bTo % 8;
                                        bTo++;
                                    }

                                    Assert.AreEqual(rFrom, rTo);
                                    Assert.AreEqual(bFrom, bTo);

                                    debuilder.Remove(0, 18);
                                }
                            }
                        }
                    }
                }

                dg.IgnoreColor = Color.Blue;
                dg.OffsetX = 5;
                dg.OffsetY = 3;
                dg.Generate();

                dg.ReplaceBlank = true;
                dg.ReplaceWhite = true;
                dg.Generate();

                dg.RotateFlipStyle = RotateFlipType.Rotate180FlipY;
                dg.Generate();
            }
            catch (DrawableException exc)
            {
                Assert.Fail(exc.Message);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [TestMethod]
        public void TestOutput()
        {
            int w = 2, h = 1;
            string path = CreateTempBitmap(w * 32, h * 8);

            try
            {
                DrawablesGenerator dg = new DrawablesGenerator(path);
                dg.OffsetX = 5;
                dg.OffsetY = 3;

                DrawablesOutput dOutput = dg.Generate();

                // Checking output
                Assert.AreEqual(64, dOutput.ImageWidth);
                Assert.AreEqual(8, dOutput.ImageHeight);

                // Checking drawables in output
                Assert.AreEqual(w, dOutput.Drawables.GetLength(0));
                Assert.AreEqual(h, dOutput.Drawables.GetLength(1));

                Drawable d = dOutput.Drawables[0, 0];
                Drawable d2 = dOutput.Drawables[1, 0];

                // Texture and ResultImage
                Assert.AreEqual(d.Texture, dg.DrawableTexture);
                Assert.AreEqual(d.Texture, d2.Texture);
                Assert.AreEqual(d.ResultImage, d.Texture + d.Directives);
                Assert.AreNotEqual(d.ResultImage, d2.ResultImage);

                // Offset matching
                Assert.AreEqual(dg.OffsetX, dOutput.OffsetX);
                Assert.AreEqual(dg.OffsetY, dOutput.OffsetY);

                // Drawable positioning
                Assert.AreEqual(0, d.X);
                Assert.AreEqual(0, d.Y);
                Assert.AreEqual(32, d2.X);
                Assert.AreEqual(0, d2.Y);
                Assert.AreEqual(0d, d.BlockX);
                Assert.AreEqual(4d, d2.BlockX);
                Assert.AreEqual(0d, d.BlockY);
                Assert.AreEqual(d.BlockY, d2.BlockY);
            }
            catch (DrawableException exc)
            {
                Assert.Fail(exc.Message);
            }
            finally
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        private TestContext testContextInstance;
        public TestContext TestContext { get { return testContextInstance; } set { testContextInstance = value; } }

        [TestMethod]
        public void TestSmall()
        {
            // Well spotted, this isn't a proper unit test! It's a sample for Degranon, cleverly disguised as a unit test.
            string path = @"F:\Users\Silver\Pictures\grid.png";

            DrawablesGenerator generator = new DrawablesGenerator(path)
            {
                RotateFlipStyle = RotateFlipType.RotateNoneFlipY // you may need to set this to RotateNoneFlipY, depending on where you apply the results.
            };

            var result = generator.GenerateScale();
            foreach (var item in result.Drawables)
            {
                TestContext.WriteLine(item.Directives);
            }
        }
    }
}
