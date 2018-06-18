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

        public Bags bags;
        public Digger digger;
        public Sound sound;
        public Monster monster;
        public Scores scores;
        public Sprite sprite;
        public Drawing drawing;
        public Input input;
        public Video video;
        public Recorder recorder;
        public Level level;
        public ITimer timer;

        public string pldispbuf = "";
        public int currentPlayer = 0, playerCount = 0, penalty = 0;
        public int fps = 20;
        public int startingLevel = 1;
        public bool levnotdrawn, flashplayer, start;

        private int randv;

        public Game(ITimer timer)
        {
            bags = new Bags(this);
            digger = new Digger(this);
            sound = new Sound(this);
            monster = new Monster(this);
            scores = new Scores(this);
            sprite = new Sprite(this);
            drawing = new Drawing(this);
            input = new Input(this);
            video = new Video();
            recorder = new Recorder(this);
            level = new Level(this);
            this.timer = timer;
        }

        public void InitLevel()
        {
            gamedat[currentPlayer].levdone = false;
            drawing.MakeField();
            digger.MakeEmeraldField();
            bags.InitBags();
            levnotdrawn = true;
        }

        public int Level => gamedat[currentPlayer].level;

        public Action<bool> SetRecordSave { get; internal set; }
        public Action<bool> SetRecordPlay { get; internal set; }
        public Action<bool> SetLevelLoad { get; internal set; }

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
                case Input.KEY_LEFT: input.ProcessKey(0x4b); break;
                case Input.KEY_RIGHT: input.ProcessKey(0x4d); break;
                case Input.KEY_UP: input.ProcessKey(0x48); break;
                case Input.KEY_DOWN: input.ProcessKey(0x50); break;
                case Input.KEY_F1: input.ProcessKey(0x3b); break;
                case Input.KEY_F10: input.ProcessKey(0x78); break;
                default:
                    if ((key >= 65) && (key <= 90))
                        key += (97 - 65);
                    input.ProcessKey(key); break;
            }
        }

        public void KeyUp(int key)
        {
            switch (key)
            {
                case Input.KEY_LEFT: input.ProcessKey(0xcb); break;
                case Input.KEY_RIGHT: input.ProcessKey(0xcd); break;
                case Input.KEY_UP: input.ProcessKey(0xc8); break;
                case Input.KEY_DOWN: input.ProcessKey(0xd0); break;
                case Input.KEY_F1: input.ProcessKey(0xbb); break;
                case Input.KEY_F10: input.ProcessKey(0xf8); break;
                default:
                    if ((key >= 65) && (key <= 90))
                        key += (97 - 65);
                    input.ProcessKey(0x80 | key); break;
            }
        }

        public void AddLife(int pl)
        {
            gamedat[pl - 1].lives++;
            sound.sound1up();
        }

        public void CheckLevelDone()
        {
            gamedat[currentPlayer].levdone = (digger.EmeraldCount() == 0 || monster.MonstersLeftCount() == 0) && digger.digonscr;
        }

        public void ClearTopLine()
        {
            drawing.TextOut("                          ", 0, 0, 3);
            drawing.TextOut(" ", 308, 0, 3);
        }

        public void DrawScreen()
        {
            drawing.CreateMonsterBagSprites();
            drawing.DrawStatics();
            bags.DrawBags();
            digger.DrawEmeralds();
            digger.InitDigger();
            monster.InitMonsters();
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
            drawing.InitMonsterSpriteBuffer();
            digger.InitDigger();
            monster.InitMonsters();
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

            randv = timer.Time;
            sprite.SetRetr(true);
            video.Init();
            scores.LoadScores();
            sound.initsound();
            SetRecordSave?.Invoke(false);

            playerCount = 1;
            do
            {
                sound.soundstop();
                sprite.SetSpriteOrder(digsprorder);
                drawing.CreateMonsterBagSprites();
                video.Clear();
                video.DrawTitleScreen();
                drawing.TextOut("D I G G E R", 100, 0, 3);
                ShowNumOfPlayers();
                scores.ShowTable();
                frame = 0;

                timer.Start();

                while (!start && !input.escape)
                {
                    start = input.TestIfStarted();
                    if (input.aKeyPressed == 27)
                    {  //	esc
                        SwitchNumOfPlayers();
                        ShowNumOfPlayers();
                        input.aKeyPressed = 0;
                        input.keyPressed = 0;
                    }
                    if (frame == 0)
                        for (t = 54; t < 174; t += 12)
                            drawing.TextOut("            ", 164, t, 0);

                    if (frame == 50)
                    {
                        sprite.MoveDrawSprite(8, 292, 63);
                        x = 292;
                    }

                    if (frame > 50 && frame <= 77)
                    {
                        x -= 4;
                        drawing.DrawMonster(0, true, 4, x, 63);
                    }

                    if (frame > 77)
                        drawing.DrawMonster(0, true, 0, 184, 63);

                    if (frame == 83)
                        drawing.TextOut("NOBBIN", 216, 64, 2);

                    if (frame == 90)
                    {
                        sprite.MoveDrawSprite(9, 292, 82);
                        drawing.DrawMonster(1, false, 4, 292, 82);
                        x = 292;
                    }

                    if (frame > 90 && frame <= 117)
                    {
                        x -= 4;
                        drawing.DrawMonster(1, false, 4, x, 82);
                    }

                    if (frame > 117)
                        drawing.DrawMonster(1, false, 0, 184, 82);

                    if (frame == 123)
                        drawing.TextOut("HOBBIN", 216, 83, 2);

                    if (frame == 130)
                    {
                        sprite.MoveDrawSprite(0, 292, 101);
                        drawing.DrawDigger(4, 292, 101, true);
                        x = 292;
                    }

                    if (frame > 130 && frame <= 157)
                    {
                        x -= 4;
                        drawing.DrawDigger(4, x, 101, true);
                    }

                    if (frame > 157)
                        drawing.DrawDigger(0, 184, 101, true);

                    if (frame == 163)
                        drawing.TextOut("DIGGER", 216, 102, 2);

                    if (frame == 178)
                    {
                        sprite.MoveDrawSprite(1, 184, 120);
                        drawing.DrawGold(1, 0, 184, 120);
                    }

                    if (frame == 183)
                        drawing.TextOut("GOLD", 216, 121, 2);

                    if (frame == 198)
                        drawing.DrawEmerald(184, 141);

                    if (frame == 203)
                        drawing.TextOut("EMERALD", 216, 140, 2);

                    if (frame == 218)
                        drawing.DrawBonus(184, 158);

                    if (frame == 223)
                        drawing.TextOut("BONUS", 216, 159, 2);

                    NewFrame();
                    frame++;
                    if (frame > 250)
                        frame = 0;
                }

                if (input.escape)
                    break;

                gamedat[0].level = startingLevel;
                gamedat[0].lives = 3;
                if (playerCount == 2)
                {
                    gamedat[1].level = startingLevel;
                    gamedat[1].lives = 3;
                }
                else
                    gamedat[1].lives = 0;

                recorder.isRecording = false;
                if (!recorder.isPlaying)
                    recorder.StartRecording();

                video.Clear();
                currentPlayer = 0;
                InitLevel();
                currentPlayer = 1;
                InitLevel();
                scores.ZeroScores();
                digger.bonusvisible = true;
                if (playerCount == 2)
                    flashplayer = true;

                SetLevelLoad?.Invoke(false);
                SetRecordPlay?.Invoke(false);
                SetRecordSave?.Invoke(false);
                currentPlayer = 0;
                while ((gamedat[0].lives != 0 || gamedat[1].lives != 0) && !input.escape)
                {
                    gamedat[currentPlayer].dead = false;
                    while (!gamedat[currentPlayer].dead && gamedat[currentPlayer].lives != 0 && !input.escape)
                    {
                        drawing.InitMonsterSpriteBuffer();
                        Play();
                    }
                    if (gamedat[1 - currentPlayer].lives != 0)
                    {
                        currentPlayer = 1 - currentPlayer;
                        flashplayer = levnotdrawn = true;
                    }
                }
                input.escape = false;
                SetLevelLoad?.Invoke(true);
                SetRecordPlay?.Invoke(true);
                SetRecordSave?.Invoke(recorder.isRecording);
            } while (true);

            Exit();
        }

        private void Play()
        {
            int t, c;
            if (recorder.isPlaying)
                randv = recorder.PlayGetRand();
            else
                randv = timer.Time;

            recorder.PutRandom(randv);

            if (levnotdrawn)
            {
                levnotdrawn = false;
                DrawScreen();
                timer.Start();
                if (flashplayer)
                {
                    flashplayer = false;
                    pldispbuf = "PLAYER ";
                    if (currentPlayer == 0)
                        pldispbuf += "1";
                    else
                        pldispbuf += "2";
                    ClearTopLine();
                    for (t = 0; t < 15; t++)
                    {
                        for (c = 1; c <= 3; c++)
                        {
                            drawing.TextOut(pldispbuf, 108, 0, c);
                            scores.WriteCurrentScore(c);
                            /* olddelay(20); */
                            NewFrame();
                            if (input.escape)
                                return;
                        }
                    }
                    scores.DrawScores();
                    scores.AddScore(0);
                }
            }
            else
                InitChars();

            input.keyPressed = 0;
            drawing.TextOut("        ", 108, 0, 3);
            scores.InitScores();
            drawing.DrawLives();
            sound.music(1);

            input.ReadDirection();
            timer.Start();

            while (!gamedat[currentPlayer].dead && !gamedat[currentPlayer].levdone && !input.escape)
            {
                NewFrame();
                penalty = 0;
                digger.DoDigger();
                monster.DoMonsters();
                bags.DoBags();
                if (penalty > 8)
                    monster.IncreaseMonsterTime(penalty - 8);

                TestIfPaused();
                CheckLevelDone();
            }
            digger.EraseDigger();
            sound.musicoff();
            t = 20;
            while ((bags.GetMovingBagsCount() != 0 || t != 0) && !input.escape)
            {
                if (t != 0)
                    t--;
                penalty = 0;
                bags.DoBags();
                digger.DoDigger();
                monster.DoMonsters();
                if (penalty < 8)
                    t = 0;
                NewFrame();
            }
            sound.soundstop();
            digger.KillFire();
            digger.EraseBonus();
            bags.CleanupBags();
            drawing.SaveField();
            monster.EraseMonsters();

            recorder.PutEndOfLevel();
            if (recorder.isPlaying)
                recorder.PlaySkipEOL();

            if (input.escape)
            {
                recorder.PutEndOfGame();
                if (recorder.isPlaying)
                {
                    recorder.isPlaying = false;
                    start = false;
                }
            }

            if (gamedat[currentPlayer].levdone)
                sound.soundlevdone();

            if (digger.EmeraldCount() == 0)
            {
                gamedat[currentPlayer].level++;
                if (gamedat[currentPlayer].level > 1000)
                    gamedat[currentPlayer].level = 1000;
                InitLevel();
            }
            if (gamedat[currentPlayer].dead)
            {
                gamedat[currentPlayer].lives--;
                drawing.DrawLives();
                if (gamedat[currentPlayer].lives == 0 && !input.escape)
                {
                    if (recorder.isPlaying)
                        recorder.isPlaying = false;
                    else
                        scores.EndOfGame();

                    start = false;
                }
            }
            if (gamedat[currentPlayer].levdone)
            {
                gamedat[currentPlayer].level++;
                if (gamedat[currentPlayer].level > 1000)
                    gamedat[currentPlayer].level = 1000;
                InitLevel();
            }
        }

        public void NewFrame()
        {
            input.CheckKeyBuffer();
            timer.SyncFrame(fps);
            video.UpdateImage();
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
            gamedat[currentPlayer].dead = bp6;
        }

        public void ShowNumOfPlayers()
        {
            if (playerCount == 1)
            {
                drawing.TextOut("ONE", 220, 25, 3);
                drawing.TextOut(" PLAYER ", 192, 39, 3);
            }
            else
            {
                drawing.TextOut("TWO", 220, 25, 3);
                drawing.TextOut(" PLAYERS", 184, 39, 3);
            }
        }

        public void SwitchNumOfPlayers()
        {
            playerCount = 3 - playerCount;
        }

        public void TestIfPaused()
        {
            if (input.isPaused)
            {
                sound.soundpause();
                sound.sett2val(40);
                sound.setsoundt2();
                ClearTopLine();
                drawing.TextOut("PRESS ANY KEY", 80, 0, 1);
                NewFrame();
                input.keyPressed = 0;
                while (input.keyPressed == 0)
                    Thread.Sleep(50);

                ClearTopLine();
                scores.DrawScores();
                scores.AddScore(0);
                drawing.DrawLives();
                timer.Start();
                NewFrame();
                input.isPaused = false;
            }
            else
                sound.soundpauseoff();
        }

        public void ParseCmdLine(string[] args)
        {
            var parser = new Args("p*", args);
            if (parser.Has('p'))
            {
                try
                {
                    recorder.OpenPlay(parser.GetString('p'));
                }
                catch { input.escape = true; }
            }
        }
    }
}