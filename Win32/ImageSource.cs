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
        private readonly int[] pixs;
        private readonly int width;
        private readonly int height;
        private readonly int scalef;
        private readonly Bitmap bmp1;
        private readonly Bitmap bmp2;
        private Bitmap bmp0;

        public ImageSource() { }

        public ImageSource(int w, int h, Color[] palette, byte[] pixels, int scalef)
        {
            this.pixels = pixels;
            this.width = w * scalef;
            this.height = h * scalef;
            this.scalef = scalef;

            this.palette = palette;
            pixs = new int[width * height];
            bmp1 = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bmp2 = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            bmp0 = bmp1;
        }

        public void UpdateImage()
        {
            int size = (width * height) / (2 * scalef);
            for (int i = 0; i < size; i++)
                Map(i, palette[pixels[i]].ToArgb());

            try { bmp0 = UpdateBitmap(bmp1); }
            catch { bmp0 = UpdateBitmap(bmp2); }
        }

        public Bitmap UpdateBitmap(Bitmap bmp)
        {
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixs, 0, bmpData.Scan0, width * height);
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        private void Map(int index, int color)
        {
            int x = (index % (width / scalef)) * scalef;
            int y = (index / (width / scalef)) * scalef;

            for (int i = 0; i < scalef; ++i)
                for (int j = 0; j < scalef; ++j)
                {
                    index = (x + i) + (y + j) * width;
                    pixs[index] = color;
                }
        }

        public void Render(Graphics gfx)
        {
            gfx.DrawImage(bmp0, 0, 0);
        }
    }
}
