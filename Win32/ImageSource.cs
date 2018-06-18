using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Digger.Win32
{
    public class ImageSource
    {
        public Color[] palette;

        private readonly byte[] pixels;
        private readonly int[] pix;
        private readonly int[] pixs;
        private readonly int w;
        private readonly int h;
        private readonly int scalef;
        private readonly Bitmap bmp;

        public ImageSource() { }

        public ImageSource(int w, int h, Color[] palette, byte[] pixels, int scalef)
        {
            this.pixels = pixels;
            this.w = w;
            this.h = h;
            this.scalef = scalef;

            this.palette = palette;
            pix = new int[w * h];
            pixs = new int[w * h * scalef * scalef]; // scaled 2x
            bmp = new Bitmap(w * scalef, h * scalef, PixelFormat.Format32bppArgb);
        }

        public void UpdateImage()
        {
            for (int i = 0; i < pix.Length; i++)
                Map(i, palette[pixels[i]].ToArgb());

            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixs, 0, bmpData.Scan0, scalef * w * scalef * h);
            bmp.UnlockBits(bmpData);
        }

        public void Render(Graphics gfx)
        {
            gfx.DrawImage(bmp, 0, 0);
        }

        private void Map(int index, int color)
        {
            int x = (index % w) * scalef;
            int y = (index / w) * scalef;
            int c = pix[index] = color;

            for (int i = 0; i < scalef; ++i)
                for (int j = 0; j < scalef; ++j)
                    pixs[GetIndex(x + i, y + j)] = c;
        }

        private int GetIndex(int x, int y)
        {
            return x + y * (scalef * w);
        }
    }
}
