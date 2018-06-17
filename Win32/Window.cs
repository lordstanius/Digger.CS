using System;
using System.Drawing;
using System.Windows.Forms;

namespace Digger.Win32
{
    public class Window : Form
    {
        private ImageSource screen;
        private readonly Color[] npalette = { Color.Black, Color.Black, Color.Black, Color.Black };
        private readonly Color[] ipalette = { Color.Black, Color.Black, Color.Black, Color.Black };
        private readonly Color[][] palettes;

        public Window(Game game)
        {
            ClientSize = new Size(320, 200);
            Text = "Digger";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            DoubleBuffered = true;
            game.Video.Render = Render;
            game.Video.InitVideo = InitVideo;
            palettes = new Color[][] { npalette, ipalette };

            Menu = new MainMenu();
            var mitGame = new MenuItem("&Game");
            var mitPlayRecordedGame = new MenuItem("&Record playback...", (_, __) => PlayRecordedGame(game));
            //var mitSaveRecordedGame = new MenuItem("&Save record...", (_, __) => SaveRecordedGame(game));
            var mitExit = new MenuItem("E&xit", (_, __) => Close());
            mitGame.MenuItems.Add(mitPlayRecordedGame);
            mitGame.MenuItems.Add("-");
            mitGame.MenuItems.Add(mitExit);
            Menu.MenuItems.Add(mitGame);

            KeyUp += (_, e) => game.KeyUp(e.KeyValue);
            KeyDown += (_, e) => game.KeyDown(e.KeyValue);
        }

        private void PlayRecordedGame(Game game)
        {
            var dlg = new OpenFileDialog { Filter = "Recorded game files|*.drf", };
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            game.PlayRecordFile(dlg.FileName);
        }

        private void SaveRecordedGame(Game game)
        {
            throw new NotImplementedException();
        }

        public void InitVideo(byte[] pixels, byte[][] npal, byte[][] ipal)
        {
            for (int i = 0; i < 3; i++)
            {
                npalette[i + 1] = Color.FromArgb(0xFF, npal[i][0], npal[i][1], npal[i][2]);
                ipalette[i + 1] = Color.FromArgb(0xFF, ipal[i][0], ipal[i][1], ipal[i][2]);
            }

            screen = new ImageSource(Video.WIDTH, Video.HEIGHT, npalette, pixels);
        }

        public void Render(byte[] pixels, int intensity)
        {
            screen.palette = palettes[intensity];
            screen.UpdateImage();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (screen != null)
                screen.Render(e.Graphics, ClientRectangle);
        }
    }
}
