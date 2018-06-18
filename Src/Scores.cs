using System.IO;
using System.Text;
using System.Threading;

namespace Digger
{
    public class Scores
    {
        public const string SCORE_FILE_NAME = "DIGGER.SCO";

        private readonly Game game;

        public int scoret = 0;
        public int bonusscore = 20000;
        public readonly string[] scoreinit = new string[11];

        private readonly int[] scorehigh = new int[12];
        private readonly byte[] scorebuf = new byte[512];
        private int score1 = 0, score2 = 0, nextbs1 = 0, nextbs2 = 0;
        private bool gotinitflag = false;

        public Scores(Game game)
        {
            this.game = game;
        }

        public void AddScore(int score)
        {
            if (game.currentPlayer == 0)
            {
                score1 += score;
                if (score1 > 999999)
                    score1 = 0;
                WriteNumber(score1, 0, 0, 6, 1);
                if (score1 >= nextbs1)
                {
                    if (game.GetLives(1) < 5)
                    {
                        game.AddLife(1);
                        game.drawing.DrawLives();
                    }
                    nextbs1 += bonusscore;
                }
            }
            else
            {
                score2 += score;
                if (score2 > 999999)
                    score2 = 0;
                if (score2 < 100000)
                    WriteNumber(score2, 236, 0, 6, 1);
                else
                    WriteNumber(score2, 248, 0, 6, 1);
                if (score2 > nextbs2)
                {   /* Player 2 doesn't get the life until >20,000 ! */
                    if (game.GetLives(2) < 5)
                    {
                        game.AddLife(2);
                        game.drawing.DrawLives();
                    }
                    nextbs2 += bonusscore;
                }
            }
            game.IncrementPenalty();
            game.IncrementPenalty();
            game.IncrementPenalty();
        }

        public void DrawScores()
        {
            WriteNumber(score1, 0, 0, 6, 3);
            if (game.playerCount == 2)
                if (score2 < 100000)
                    WriteNumber(score2, 236, 0, 6, 3);
                else
                    WriteNumber(score2, 248, 0, 6, 3);
        }

        public void EndOfGame()
        {
            AddScore(0);
            if (game.currentPlayer == 0)
                scoret = score1;
            else
                scoret = score2;
            if (scoret > scorehigh[11])
            {
                game.video.Clear();
                DrawScores();
                game.pldispbuf = "PLAYER ";
                if (game.currentPlayer == 0)
                    game.pldispbuf += "1";
                else
                    game.pldispbuf += "2";
                game.drawing.TextOut(game.pldispbuf, 108, 0, 2, true);
                game.drawing.TextOut(" NEW HIGH SCORE ", 64, 40, 2, true);
                GetInitials();
                ShuffleHigh();
                SaveScores();
            }
            else
            {
                game.ClearTopLine();
                game.drawing.TextOut("GAME OVER", 104, 0, 3, true);
                game.sound.killsound();
                for (int j = 0; j < 20; j++) /* Number of times screen flashes * 2 */
                    for (int i = 0; i < 2; i++)
                    { //i<8;i++) {
                        game.sprite.SetRetr(true);
                        //		game.Pc.ginten(1);
                        game.sprite.SetRetr(false);
                        for (int z = 0; z < 111; z++) ; /* A delay loop */
                        //		game.Pc.ginten(0);
                        game.video.SetIntensity(1 - i & 1);
                        game.NewFrame();
                    }
                game.sound.setupsound();
                game.drawing.TextOut("         ", 104, 0, 3, true);
                game.sprite.SetRetr(true);
            }
        }

        public void FlashyWait(int n)
        {
            Thread.Sleep(n);
        }

        public int GetInitial(int x, int y)
        {
            game.input.keyPressed = 0;
            game.video.Write(x, y, '_', 3, true);
            game.NewFrame();
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 40; i++)
                {
                    if ((game.input.keyPressed & 0x80) == 0 && game.input.keyPressed != 0)
                        return game.input.keyPressed;
                    FlashyWait(15);
                }

                for (int i = 0; i < 40; i++)
                {
                    if ((game.input.keyPressed & 0x80) == 0 && game.input.keyPressed != 0)
                    {
                        game.video.Write(x, y, '_', 3, true);
                        game.NewFrame();
                        return game.input.keyPressed;
                    }
                    FlashyWait(15);
                }
            }
            gotinitflag = true;
            return 0;
        }

        public void GetInitials()
        {
            game.drawing.TextOut("ENTER YOUR", 100, 70, 3, true);
            game.drawing.TextOut(" INITIALS", 100, 90, 3, true);
            game.drawing.TextOut("_ _ _", 128, 130, 3, true);
            scoreinit[0] = "...";
            game.sound.killsound();

            for (int j = 0, i = 0; j < 20; j++) /* Number of times screen flashes * 2 */
            {
                FlashyWait(3);
                game.video.SetIntensity(i = 1 - i & 1);
                game.NewFrame();
            }

            gotinitflag = false;
            for (int i = 0; i < 3; i++)
            {
                int k = 0;
                while (k == 0 && !gotinitflag)
                {
                    k = GetInitial(i * 24 + 128, 130);
                    if (i != 0 && k == 8)
                        i--;
                    k = game.input.GetAsciiKey(k);
                }
                if (k != 0)
                {
                    game.video.Write(i * 24 + 128, 130, k, 1, true);
                    StringBuilder sb = new StringBuilder(scoreinit[0]);
                    sb[i] = (char)k;
                    scoreinit[0] = sb.ToString();
                }
            }
            game.input.keyPressed = 0;
            game.NewFrame();
            for (int i = 0; i < 20; i++)
                FlashyWait(15);
            game.sound.setupsound();
            game.video.Clear();
            game.video.SetIntensity(0);
            game.NewFrame();
            game.sprite.SetRetr(true);
        }

        public void InitScores()
        {
            AddScore(0);
        }

        public void LoadScores()
        {
            int p = 0;
            ReadScores();
            //if (game.isGauntletMode)
            //    p = 111;
            //if (game.diggerCount == 2)
            //    p += 222;
            if (scorebuf[p++] != 's')
            {
                for (int i = 0; i < 11; i++)
                {
                    scorehigh[i + 1] = 0;
                    scoreinit[i] = "...";
                }
            }
            else
            {
                for (int i = 1; i < 11; i++)
                {
                    scoreinit[i] = Encoding.ASCII.GetString(scorebuf, p, 3);
                    p += 5;
                    string highbuf = Encoding.ASCII.GetString(scorebuf, p, 6).TrimEnd();
                    if (int.TryParse(highbuf, out int highScore))
                        scorehigh[i + 1] = highScore;
                    p += 6;
                }
            }
        }

        private void ReadScores()
        {
            if (!game.level.isUsingLevelFile)
            {
                if (File.Exists(SCORE_FILE_NAME))
                {
                    using (var inFile = File.OpenRead(SCORE_FILE_NAME))
                    {
                        if (inFile.Read(scorebuf, 0, 512) == 0)
                            scorebuf[0] = 0;
                    }
                }
            }
            else
            {
                using (var inFile = File.OpenRead(game.level.levelFilePath))
                {
                    inFile.Seek(1202, SeekOrigin.Begin);
                    if (inFile.Read(scorebuf, 0, 512) == 0)
                        scorebuf[0] = 0;
                }
            }
        }

        private void SaveScores()
        {
            int p = 0;
            //if (game.isGauntletMode)
            //    p = 111;
            //if (game.diggerCount == 2)
            //    p += 222;
            scorebuf[p] = (byte)'s';
            for (int i = 1; i < 11; i++)
            {
                string hsbuf = $"{scoreinit[i]}  {NumToString(scorehigh[i + 1])}";
                for (int j = 0; j < 11; j++)
                    scorebuf[p + j + i * 11 - 10] = (byte)hsbuf[j];
            }
            WriteScores();
        }

        public string NumToString(long n)
        {
            return string.Format("{0,-6:d}", n);
        }

        private void WriteScores()
        {
            if (!game.level.isUsingLevelFile)
            {
                using (var inFile = File.OpenWrite(SCORE_FILE_NAME))
                {
                    inFile.Write(scorebuf, 0, 512);
                }
            }
            else
            {
                using (var inFile = File.OpenWrite(game.level.levelFilePath))
                {
                    inFile.Seek(1202, SeekOrigin.Begin);
                    inFile.Write(scorebuf, 0, 512);
                }
            }
        }

        public void ScoreBonus()
        {
            AddScore(1000);
        }

        public void ScoreEatMonster()
        {
            AddScore(game.digger.eatmsc * 200);
            game.digger.eatmsc <<= 1;
        }

        public void ScoreEmerald()
        {
            AddScore(25);
        }

        public void ScoreGold()
        {
            AddScore(500);
        }

        public void ScoreKill()
        {
            AddScore(250);
        }

        public void ScoreOctave()
        {
            AddScore(250);
        }

        public void ShowTable()
        {
            int i, col;
            game.drawing.TextOut("HIGH SCORES", 16, 25, 3);
            col = 2;
            for (i = 1; i < 11; i++)
            {
                string hsbuf = $"{scoreinit[i]}  {NumToString(scorehigh[i + 1])}";
                game.drawing.TextOut(hsbuf, 16, 31 + 13 * i, col);
                col = 1;
            }
        }

        public void ShuffleHigh()
        {
            int i, j;
            for (j = 10; j > 1; j--)
                if (scoret < scorehigh[j])
                    break;
            for (i = 10; i > j; i--)
            {
                scorehigh[i + 1] = scorehigh[i];
                scoreinit[i] = scoreinit[i - 1];
            }
            scorehigh[j + 1] = scoret;
            scoreinit[j] = scoreinit[0];
        }

        public void WriteCurrentScore(int bp6)
        {
            if (game.currentPlayer == 0)
                WriteNumber(score1, 0, 0, 6, bp6);
            else
              if (score2 < 100000)
                WriteNumber(score2, 236, 0, 6, bp6);
            else
                WriteNumber(score2, 248, 0, 6, bp6);
        }

        public void WriteNumber(long n, int x, int y, int w, int c)
        {
            int d, xp = (w - 1) * 12 + x;
            while (w > 0)
            {
                d = (int)(n % 10);
                if (w > 1 || d > 0)
                    game.video.Write(xp, y, d + '0', c, false);    //true
                n /= 10;
                w--;
                xp -= 12;
            }
        }

        public void ZeroScores()
        {
            score2 = 0;
            score1 = 0;
            scoret = 0;
            nextbs1 = bonusscore;
            nextbs2 = bonusscore;
        }
    }
}