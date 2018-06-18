using System;
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
        private readonly Bitmap bmp1;
        private readonly Bitmap bmp2;
        private Bitmap bmp0;

        public ImageSource() { }

        public ImageSource(int w, int h, Color[] palette, byte[] pixels, int scalef)
        {
            this.pixels = pixels;
            this.w = w * scalef;
            this.h = h * scalef;
            this.scalef = scalef;

            this.palette = palette;
            pix = new int[w * h];
            pixs = new int[this.w * this.h]; // scaled 2x
            bmp1 = new Bitmap(this.w, this.h, PixelFormat.Format32bppArgb);
            bmp2 = new Bitmap(this.w, this.h, PixelFormat.Format32bppArgb);
            bmp0 = bmp1;
        }

        public void UpdateImage()
        {
            for (int i = 0; i < pix.Length; i++)
                Map(i, palette[pixels[i]].ToArgb());

            try
            {
                BitmapData bmpData = bmp1.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(pixs, 0, bmpData.Scan0, w * h);
                bmp1.UnlockBits(bmpData);
                bmp0 = bmp1;
            }
            catch
            {
                BitmapData bmpData = bmp2.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(pixs, 0, bmpData.Scan0, w * h);
                bmp2.UnlockBits(bmpData);
                bmp0 = bmp2;
            }
        }

        public void Render(Graphics gfx)
        {
            gfx.DrawImage(bmp0, 0, 0);
        }

        private void Map(int index, int color)
        {
            int x = (index % (w / scalef)) * scalef;
            int y = (index / (w / scalef)) * scalef;
            int c = pix[index] = color;

            for (int i = 0; i < scalef; ++i)
                for (int j = 0; j < scalef; ++j)
                    pixs[GetIndex(x + i, y + j)] = c;
        }

        private int GetIndex(int x, int y)
        {
            return x + y * w;
        }
    }
}
