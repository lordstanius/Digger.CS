using System;
using System.Threading;
using Digger.Interface;

namespace Digger
{
    public class Game
    {
        private struct GameData
        {
            public int lives, level;
            public bool dead, levdone;
        }

        private readonly int[] digsprorder = { 14, 13, 7, 6, 5, 4, 3, 2, 1, 12, 11, 10, 9, 8, 15, 0 };   // [16]

        GameData[] gamedat = new GameData[2];

        private Thread gameThread;

        public Bags Bags;
        public Digger Digger;
        public Sound Sound;
        public Monster Monster;
        public Scores Scores;
        public Sprite Sprite;
        public Drawing Drawing;
        public Input Input;
        public Video Video;
        public Recorder Recording;
        public ITimer Timer;
        // -----

        public string pldispbuf = "";
        public int curplayer = 0, playerCount = 0, penalty = 0;
        public int fps = 13;
        public bool levnotdrawn, flashplayer, start;

        private int randv;

        public Game(ITimer timer)
        {
            Bags = new Bags(this);
            Digger = new Digger(this);
            Sound = new Sound(this);
            Monster = new Monster(this);
            Scores = new Scores(this);
            Sprite = new Sprite(this);
            Drawing = new Drawing(this);
            Input = new Input(this);
            Video = new Video();
            Recording = new Recorder(this);
            Timer = timer;
        }

        public void InitLevel()
        {
            gamedat[curplayer].levdone = false;
            Drawing.MakeField();
            Digger.MakeEmeraldField();
            Bags.InitBags();
            levnotdrawn = true;
        }

        public int Level => gamedat[curplayer].level;

        public int ReverseDir(int dir)
        {
            switch (dir)
            {
                case 0: return 4;
                case 4: return 0;
                case 2: return 6;
                case 6: return 2;
            }
            return dir;
        }

        public void KeyDown(int key)
        {
            switch (key)
            {
                case Input.KEY_LEFT: Input.ProcessKey(0x4b); break;
                case Input.KEY_RIGHT: Input.ProcessKey(0x4d); break;
                case Input.KEY_UP: Input.ProcessKey(0x48); break;
                case Input.KEY_DOWN: Input.ProcessKey(0x50); break;
                case Input.KEY_F1: Input.ProcessKey(0x3b); break;
                case Input.KEY_F10: Input.ProcessKey(0x78); break;
                default:
                    if ((key >= 65) && (key <= 90))
                        key += (97 - 65);
                    Input.ProcessKey(key); break;
            }
        }

        public void KeyUp(int key)
        {
            switch (key)
            {
                case Input.KEY_LEFT: Input.ProcessKey(0xcb); break;
                case Input.KEY_RIGHT: Input.ProcessKey(0xcd); break;
                case Input.KEY_UP: Input.ProcessKey(0xc8); break;
                case Input.KEY_DOWN: Input.ProcessKey(0xd0); break;
                case Input.KEY_F1: Input.ProcessKey(0xbb); break;
                case Input.KEY_F10: Input.ProcessKey(0xf8); break;
                default:
                    if ((key >= 65) && (key <= 90))
                        key += (97 - 65);
                    Input.ProcessKey(0x80 | key); break;
            }
        }

        public void AddLife(int pl)
        {
            gamedat[pl - 1].lives++;
            Sound.sound1up();
        }

        public void CheckLevelDone()
        {
            gamedat[curplayer].levdone = (Digger.EmeraldCount() == 0 || Monster.MonstersLeftCount() == 0) && Digger.digonscr;
        }

        public void ClearTopLine()
        {
            Drawing.TextOut("                          ", 0, 0, 3);
            Drawing.TextOut(" ", 308, 0, 3);
        }

        public void DrawScreen()
        {
            Drawing.CreateMonsterBagSprites();
            Drawing.DrawStatics();
            Bags.DrawBags();
            Digger.DrawEmeralds();
            Digger.InitDigger();
            Monster.InitMonsters();
        }

        public int GetCurrentPlayer()
        {
            return curplayer;
        }

        public int GetLives(int pl)
        {
            return gamedat[pl - 1].lives;
        }

        public void IncrementPenalty()
        {
            penalty++;
        }

        public void InitChars()
        {
            Drawing.InitMonsterSpriteBuffer();
            Digger.InitDigger();
            Monster.InitMonsters();
        }

        public void Start()
        {
            if (gameThread != null)
                gameThread.Abort();

            gameThread = new Thread(Run);
            gameThread.Start();
        }

        private void Run()
        {
            int frame, t, x = 0;

            randv = Timer.Time;
            Sprite.SetRetr(true);
            Video.Init();
            Scores.LoadScores();
            Sound.initsound();

            Scores.Run(); // TODO: ??
            Scores.UpdateScores(Scores.scores);

            playerCount = 1;
            do
            {
                Sound.soundstop();
                Sprite.SetSpriteOrder(digsprorder);
                Drawing.CreateMonsterBagSprites();
                Video.Clear();
                Video.DrawTitleScreen();
                Drawing.TextOut("D I G G E R", 100, 0, 3);
                ShowNumOfPlayers();
                Scores.ShowTable();
                frame = 0;

                Timer.Start();

                while (!start && !Input.escape)
                {
                    start = Input.TestIfStarted();
                    if (Input.aKeyPressed == 27)
                    {  //	esc
                        SwitchNumOfPlayers();
                        ShowNumOfPlayers();
                        Input.aKeyPressed = 0;
                        Input.keyPressed = 0;
                    }
                    if (frame == 0)
                        for (t = 54; t < 174; t += 12)
                            Drawing.TextOut("            ", 164, t, 0);
                    if (frame == 50)
                    {
                        Sprite.MoveDrawSprite(8, 292, 63);
                        x = 292;
                    }
                    if (frame > 50 && frame <= 77)
                    {
                        x -= 4;
                        Drawing.DrawMonster(0, true, 4, x, 63);
                    }
                    if (frame > 77)
                        Drawing.DrawMonster(0, true, 0, 184, 63);
                    if (frame == 83)
                        Drawing.TextOut("NOBBIN", 216, 64, 2);
                    if (frame == 90)
                    {
                        Sprite.MoveDrawSprite(9, 292, 82);
                        Drawing.DrawMonster(1, false, 4, 292, 82);
                        x = 292;
                    }
                    if (frame > 90 && frame <= 117)
                    {
                        x -= 4;
                        Drawing.DrawMonster(1, false, 4, x, 82);
                    }
                    if (frame > 117)
                        Drawing.DrawMonster(1, false, 0, 184, 82);
                    if (frame == 123)
                        Drawing.TextOut("HOBBIN", 216, 83, 2);
                    if (frame == 130)
                    {
                        Sprite.MoveDrawSprite(0, 292, 101);
                        Drawing.DrawDigger(4, 292, 101, true);
                        x = 292;
                    }
                    if (frame > 130 && frame <= 157)
                    {
                        x -= 4;
                        Drawing.DrawDigger(4, x, 101, true);
                    }
                    if (frame > 157)
                        Drawing.DrawDigger(0, 184, 101, true);
                    if (frame == 163)
                        Drawing.TextOut("DIGGER", 216, 102, 2);
                    if (frame == 178)
                    {
                        Sprite.MoveDrawSprite(1, 184, 120);
                        Drawing.DrawGold(1, 0, 184, 120);
                    }
                    if (frame == 183)
                        Drawing.TextOut("GOLD", 216, 121, 2);
                    if (frame == 198)
                        Drawing.DrawEmerald(184, 141);
                    if (frame == 203)
                        Drawing.TextOut("EMERALD", 216, 140, 2);
                    if (frame == 218)
                        Drawing.DrawBonus(184, 158);
                    if (frame == 223)
                        Drawing.TextOut("BONUS", 216, 159, 2);
                    NewFrame();
                    frame++;
                    if (frame > 250)
                        frame = 0;
                }

                gamedat[0].level = 1;
                gamedat[0].lives = 3;
                if (playerCount == 2)
                {
                    gamedat[1].level = 1;
                    gamedat[1].lives = 3;
                }
                else
                    gamedat[1].lives = 0;
                Video.Clear();
                curplayer = 0;
                InitLevel();
                curplayer = 1;
                InitLevel();
                Scores.ZeroScores();
                Digger.bonusvisible = true;
                if (playerCount == 2)
                    flashplayer = true;
                curplayer = 0;

                if (Input.escape)
                    break;

                //if (recording)
                //    Recording.recputinit();
                while ((gamedat[0].lives != 0 || gamedat[1].lives != 0) && !Input.escape)
                {
                    gamedat[curplayer].dead = false;
                    while (!gamedat[curplayer].dead && gamedat[curplayer].lives != 0 && !Input.escape)
                    {
                        Drawing.InitMonsterSpriteBuffer();
                        Play();
                    }
                    if (gamedat[1 - curplayer].lives != 0)
                    {
                        curplayer = 1 - curplayer;
                        flashplayer = levnotdrawn = true;
                    }
                }
                Input.escape = false;
            } while (true);
            /*  Sound.soundoff();
            restoreint8();
            restorekeyb();
            graphicsoff(); */
            Exit();
        }

        private void Play()
        {
            int t, c;
            if (Recording.IsPlaying)
                randv = Recording.PlayGetRand();
            else
                randv = Timer.Time;

            Recording.PutRandom(randv);

            if (levnotdrawn)
            {
                levnotdrawn = false;
                DrawScreen();
                Timer.Start();
                if (flashplayer)
                {
                    flashplayer = false;
                    pldispbuf = "PLAYER ";
                    if (curplayer == 0)
                        pldispbuf += "1";
                    else
                        pldispbuf += "2";
                    ClearTopLine();
                    for (t = 0; t < 15; t++)
                    {
                        for (c = 1; c <= 3; c++)
                        {
                            Drawing.TextOut(pldispbuf, 108, 0, c);
                            Scores.WriteCurrentScore(c);
                            /* olddelay(20); */
                            NewFrame();
                            if (Input.escape)
                                return;
                        }
                    }
                    Scores.DrawScores();
                    Scores.AddScore(0);
                }
            }
            else
                InitChars();
            Input.keyPressed = 0;
            Drawing.TextOut("        ", 108, 0, 3);
            Scores.InitScores();
            Drawing.DrawLives();
            Sound.music(1);

            Input.ReadDirection();
            Timer.Start();

            while (!gamedat[curplayer].dead && !gamedat[curplayer].levdone && !Input.escape)
            {
                NewFrame();
                penalty = 0;
                Digger.DoDigger();
                Monster.DoMonsters();
                Bags.DoBags();
                if (penalty > 8)
                    Monster.IncreaseMonsterTime(penalty - 8);

                TestIfPaused();
                CheckLevelDone();
            }
            Digger.EraseDigger();
            Sound.musicoff();
            t = 20;
            while ((Bags.GetMovingBagsCount() != 0 || t != 0) && !Input.escape)
            {
                if (t != 0)
                    t--;
                penalty = 0;
                Bags.DoBags();
                Digger.DoDigger();
                Monster.DoMonsters();
                if (penalty < 8)
                    /*    for (t=(8-penalty)*5;t>0;t--)
                                olddelay(1); */
                    t = 0;
                NewFrame();
            }
            Sound.soundstop();
            Digger.KillFire();
            Digger.EraseBonus();
            Bags.CleanupBags();
            Drawing.SaveField();
            Monster.EraseMonsters();

            //Recording.PutEndOfLevel();
            if (Recording.IsPlaying)
                Recording.PlaySkipEOL();

            NewFrame();
            if (gamedat[curplayer].levdone)
                Sound.soundlevdone();

            if (Digger.EmeraldCount() == 0)
            {
                gamedat[curplayer].level++;
                if (gamedat[curplayer].level > 1000)
                    gamedat[curplayer].level = 1000;
                InitLevel();
            }
            if (gamedat[curplayer].dead)
            {
                gamedat[curplayer].lives--;
                Drawing.DrawLives();
                if (gamedat[curplayer].lives == 0 && !Input.escape)
                    Scores.EndOfGame();
            }
            if (gamedat[curplayer].levdone)
            {
                gamedat[curplayer].level++;
                if (gamedat[curplayer].level > 1000)
                    gamedat[curplayer].level = 1000;
                InitLevel();
            }
        }

        public void NewFrame()
        {
            Input.CheckKeyBuffer();
            Timer.SyncFrame(fps);
            Video.UpdateImage();
        }

        public void Exit()
        {
            Environment.Exit(0);
        }

        public short RandNo(int n)
        {
            randv = randv * 0x15a4e35 + 1;
            return (short)((randv & 0x7fffffff) % n);
        }

        public void SetDead(bool bp6)
        {
            gamedat[curplayer].dead = bp6;
        }

        public void ShowNumOfPlayers()
        {
            if (playerCount == 1)
            {
                Drawing.TextOut("ONE", 220, 25, 3);
                Drawing.TextOut(" PLAYER ", 192, 39, 3);
            }
            else
            {
                Drawing.TextOut("TWO", 220, 25, 3);
                Drawing.TextOut(" PLAYERS", 184, 39, 3);
            }
        }

        public void SwitchNumOfPlayers()
        {
            playerCount = 3 - playerCount;
        }

        public void TestIfPaused()
        {
            if (Input.isPaused)
            {
                Sound.soundpause();
                Sound.sett2val(40);
                Sound.setsoundt2();
                ClearTopLine();
                Drawing.TextOut("PRESS ANY KEY", 80, 0, 1);
                NewFrame();
                Input.keyPressed = 0;
                while (Input.keyPressed == 0)
                    Thread.Sleep(50);

                ClearTopLine();
                Scores.DrawScores();
                Scores.AddScore(0);
                Drawing.DrawLives();
                Timer.Start();
                NewFrame();
                Input.isPaused = false;
            }
            else
                Sound.soundpauseoff();
        }

        public void ParseCmdLine(string[] args)
        {
            var parser = new Args("p*", args);
            if (parser.Has('p'))
            {
                try
                {
                    Recording.OpenPlay(parser.GetString('p'));
                    start = true;
                }
                catch { Input.escape = true; }
            }
        }
    }
}