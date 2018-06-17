using System.Text;
using System.Threading;

namespace Digger
{
    public class Scores
    {
        private readonly Game game;
        public object[][] scores;
        public int bonusscore = 20000;

        private readonly char[] highbuf = new char[10];
        private readonly int[] scorehigh = new int[12];
        private readonly string[] scoreinit = new string[11];
        private readonly char[] scorebuf = new char[512];
        private int scoret = 0, score1 = 0, score2 = 0, nextbs1 = 0, nextbs2 = 0;
        private string hsbuf;
        private bool gotinitflag = false;

        public Scores(Game game)
        {
            this.game = game;
        }

        public void UpdateScores(object[][] o)
        {
            // TODO: implement
            if (o == null)
                return;

            string[] @in = new string[10];
            int[] sc = new int[10];
            for (int i = 0; i < 10; i++)
            {
                @in[i] = (string)o[i][0];
                sc[i] = (int)o[i][1];
            }
            for (int i = 0; i < 10; i++)
            {
                scoreinit[i + 1] = @in[i];
                scorehigh[i + 2] = sc[i];
            }
        }

        public void AddScore(int score)
        {
            if (game.GetCurrentPlayer() == 0)
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
                        game.Drawing.DrawLives();
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
                        game.Drawing.DrawLives();
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
            if (game.GetCurrentPlayer() == 0)
                scoret = score1;
            else
                scoret = score2;
            if (scoret > scorehigh[11])
            {
                game.Video.Clear();
                DrawScores();
                game.pldispbuf = "PLAYER ";
                if (game.GetCurrentPlayer() == 0)
                    game.pldispbuf += "1";
                else
                    game.pldispbuf += "2";
                game.Drawing.TextOut(game.pldispbuf, 108, 0, 2, true);
                game.Drawing.TextOut(" NEW HIGH SCORE ", 64, 40, 2, true);
                GetInitials();
                ShuffleHigh();
                //	savescores();
                game.start = false;
            }
            else
            {
                game.ClearTopLine();
                game.Drawing.TextOut("GAME OVER", 104, 0, 3, true);
                // TODO: _updatescores(_submit("...", (int)scoret));
                game.Sound.killsound();
                for (int j = 0; j < 20; j++) /* Number of times screen flashes * 2 */
                    for (int i = 0; i < 2; i++)
                    { //i<8;i++) {
                        game.Sprite.SetRetr(true);
                        //		game.Pc.ginten(1);
                        game.Sprite.SetRetr(false);
                        for (int z = 0; z < 111; z++) ; /* A delay loop */
                        //		game.Pc.ginten(0);
                        game.Video.SetIntensity(1 - i & 1);
                        game.NewFrame();
                    }
                game.Sound.setupsound();
                game.Drawing.TextOut("         ", 104, 0, 3, true);
                game.Sprite.SetRetr(true);
            }
        }

        public void FlashyWait(int n)
        {
            /*  int i,gt,cx,p=0,k=1;
              int gap=19;
              game.Sprite.setretr(false);
              for (i=0;i<(n<<1);i++) {
                for (cx=0;cx<game.Sound.volume;cx++) {
                  game.Pc.gpal(p=1-p);
                  for (gt=0;gt<gap;gt++);
                }
                } */

            Thread.Sleep(n * 2);
        }

        public int GetInitial(int x, int y)
        {
            game.Input.keyPressed = 0;
            game.Video.Write(x, y, '_', 3, true);
            game.NewFrame();
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 40; i++)
                {
                    if ((game.Input.keyPressed & 0x80) == 0 && game.Input.keyPressed != 0)
                        return game.Input.keyPressed;
                    FlashyWait(15);
                }

                for (int i = 0; i < 40; i++)
                {
                    if ((game.Input.keyPressed & 0x80) == 0 && game.Input.keyPressed != 0)
                    {
                        game.Video.Write(x, y, '_', 3, true);
                        game.NewFrame();
                        return game.Input.keyPressed;
                    }
                    FlashyWait(15);
                }
            }
            gotinitflag = true;
            return 0;
        }

        public void GetInitials()
        {
            game.Drawing.TextOut("ENTER YOUR", 100, 70, 3, true);
            game.Drawing.TextOut(" INITIALS", 100, 90, 3, true);
            game.Drawing.TextOut("_ _ _", 128, 130, 3, true);
            scoreinit[0] = "...";
            game.Sound.killsound();

            for (int j = 0, i = 0; j < 20; j++) /* Number of times screen flashes * 2 */
            {
                for (int z = 0; z < 111; z++)
                {
                    /* A delay loop */
                }
                game.Video.SetIntensity(i = 1 - i & 1);
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
                    k = game.Input.GetAsciiKey(k);
                }
                if (k != 0)
                {
                    game.Video.Write(i * 24 + 128, 130, k, 1, true);
                    StringBuilder sb = new StringBuilder(scoreinit[0]);
                    sb[i] = (char)k;
                    scoreinit[0] = sb.ToString();
                }
            }
            game.Input.keyPressed = 0;
            game.NewFrame();
            for (int i = 0; i < 20; i++)
                FlashyWait(15);
            game.Sound.setupsound();
            game.Video.Clear();
            game.Video.SetIntensity(0);
            game.NewFrame();
            game.Sprite.SetRetr(true);
        }

        public void InitScores()
        {
            AddScore(0);
        }

        public void LoadScores()
        {
            int p = 1, i, x;
            //readscores();
            for (i = 1; i < 11; i++)
            {
                for (x = 0; x < 3; x++)
                    scoreinit[i] = "..."; //  scorebuf[p++];	--- zmienic
                p += 2;
                for (x = 0; x < 6; x++)
                    highbuf[x] = scorebuf[p++];
                scorehigh[i + 1] = 0; //atol(highbuf);
            }
            if (scorebuf[0] != 's')
                for (i = 0; i < 11; i++)
                {
                    scorehigh[i + 1] = 0;
                    scoreinit[i] = "...";
                }
        }

        public string NumToString(long n)
        {
            return string.Format("{0,-6:d}", n);
        }

        public void Run()
        {
            // TODO: ???
            //try
            //{
            //    URL u = new URL(game.subaddr + '?' + substr);
            //    URLConnection uc = u.openConnection();
            //    uc.setUseCaches(false);
            //    uc.connect();
            //    BufferedReader br = new BufferedReader(new InputStreamReader(uc.getInputStream()));
            //    object[][] sc = new object[10][2];
            //    for (int i = 0; i < 10; i++)
            //    {
            //        sc[i][0] = br.readLine();
            //        sc[i][1] = new Integer(br.readLine());
            //    }
            //    br.close();
            //    scores = sc;
            //}
            //catch (Exception e)
            //{
            //}
        }

        public void ScoreBonus()
        {
            AddScore(1000);
        }

        public void ScoreEatMonster()
        {
            AddScore(game.Digger.eatmsc * 200);
            game.Digger.eatmsc <<= 1;
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
            game.Drawing.TextOut("HIGH SCORES", 16, 25, 3);
            col = 2;
            for (i = 1; i < 11; i++)
            {
                hsbuf = scoreinit[i] + "  " + NumToString(scorehigh[i + 1]);
                game.Drawing.TextOut(hsbuf, 16, 31 + 13 * i, col);
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
            if (game.GetCurrentPlayer() == 0)
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
                    game.Video.Write(xp, y, d + '0', c, false);    //true
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