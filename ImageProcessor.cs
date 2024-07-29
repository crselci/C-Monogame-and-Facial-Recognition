using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace ImageProcess
{
    public class ImageProcessor
    {
        public static Texture2D SaveCircularImage(GraphicsDevice graphicsDevice, Image image, int radius)
        {
            int diameter = radius * 2;
            Bitmap circularImage = new Bitmap(diameter, diameter);

            using (Graphics g = Graphics.FromImage(circularImage))
            {
                g.Clear(Color.Transparent);
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, diameter, diameter);
                g.SetClip(path);

                // Calculate the size to resize the image
                Size resizedSize = new Size(diameter, diameter);
                if (image.Width > image.Height)
                {
                    resizedSize.Height = (int)((float)image.Height / image.Width * diameter);
                }
                else
                {
                    resizedSize.Width = (int)((float)image.Width / image.Height * diameter);
                }

                // Resize and center the image
                Image resizedImage = new Bitmap(image, resizedSize);
                int x = (diameter - resizedSize.Width) / 2;
                int y = (diameter - resizedSize.Height) / 2;
                g.DrawImage(resizedImage, x, y);

                using (MemoryStream ms = new MemoryStream())
                {
                    circularImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    return Texture2D.FromStream(graphicsDevice, ms);
                }
            }
        }
    }
}
