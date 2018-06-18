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

        private const string FilterStr = "Recorded game files|*.drf";
        private const int ScaleFactor = 2;

        private readonly Game game;
        private readonly DlgLevel dlgLevel = new DlgLevel();

        public Window(Game game)
        {
            ClientSize = new Size(320 * ScaleFactor, 200 * ScaleFactor);
            Text = "Digger";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            DoubleBuffered = true;
            this.game = game;
            game.video.Render = Render;
            game.video.InitVideo = InitVideo;
            palettes = new Color[][] { npalette, ipalette };

            Menu = new MainMenu();
            var miGame = new MenuItem("Game");
            var miLevel = new MenuItem("Level...", LoadLevel);
            var miPlayRecordedGame = new MenuItem("Record playback...", PlayRecordedGame);
            var miSaveRecordedGame = new MenuItem("Save record...", SaveRecordedGame);
            var miExit = new MenuItem("Exit", (_, __) => Close());
            miGame.MenuItems.Add(miLevel);
            miGame.MenuItems.Add("-");
            miGame.MenuItems.Add(miPlayRecordedGame);
            miGame.MenuItems.Add(miSaveRecordedGame);
            miGame.MenuItems.Add("-");
            miGame.MenuItems.Add(miExit);
            Menu.MenuItems.Add(miGame);

            KeyUp += (_, e) => game.KeyUp(e.KeyValue);
            KeyDown += (_, e) => game.KeyDown(e.KeyValue);

            game.SetRecordSave = (value) => miSaveRecordedGame.Enabled = value;
            game.SetRecordPlay = (value) => miPlayRecordedGame.Enabled = value;
            game.SetLevelLoad = (value) => miLevel.Enabled = value;

            dlgLevel.Owner = this;
        }

        public void InitVideo(byte[] pixels, byte[][] npal, byte[][] ipal)
        {
            for (int i = 0; i < 3; i++)
            {
                npalette[i + 1] = Color.FromArgb(0xFF, npal[i][0], npal[i][1], npal[i][2]);
                ipalette[i + 1] = Color.FromArgb(0xFF, ipal[i][0], ipal[i][1], ipal[i][2]);
            }

            screen = new ImageSource(Video.WIDTH, Video.HEIGHT, npalette, pixels, ScaleFactor);
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
                screen.Render(e.Graphics);
        }

        private void PlayRecordedGame(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = FilterStr, };
            if (dlg.ShowDialog() == DialogResult.OK)
                game.recorder.OpenPlay(dlg.FileName);
        }

        private void SaveRecordedGame(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = game.recorder.GetDefaultFileName(),
                Filter = FilterStr
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                game.recorder.SaveRecordFile(dlg.FileName);
        }

        private void LoadLevel(object sender, EventArgs e)
        {
            if (dlgLevel.ShowDialog() == DialogResult.OK)
            {
                game.startingLevel = Convert.ToInt32(dlgLevel.cbLevel.SelectedItem);
                if (!string.IsNullOrWhiteSpace(dlgLevel.LevelFilePath))
                    game.level.ReadFromFile(dlgLevel.LevelFilePath);
                else
                    game.level.RestoreData();
            }
        }
    }
}
