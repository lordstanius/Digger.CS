namespace Digger
{
    public class Bags
    {
        private struct Bag
        {
            public int x, y, h, v, xr, yr, dir, wt, gt, fallh;
            public bool wobbling, unfallen, exist;
        }

        private readonly Bag[] bagdat = new Bag[8];
        private readonly Bag[] bagdat1 = new Bag[8];
        private readonly Bag[] bagdat2 = new Bag[8];

        int pushcount = 0, goldtime = 0;

        private readonly int[] wblanim = { 2, 0, 1, 0 }; // [4]
        private Game game;

        public Bags(Game game)
        {
            this.game = game;
        }

        public int BagBits()
        {
            int bag, b, bags = 0;
            for (bag = 1, b = 2; bag < 8; bag++, b <<= 1)
                if (bagdat[bag].exist)
                    bags |= b;
            return bags;
        }

        public void BagHitGround(int bag)
        {
            int bn, b, clbits;
            if (bagdat[bag].dir == 6 && bagdat[bag].fallh > 1)
                bagdat[bag].gt = 1;
            else
                bagdat[bag].fallh = 0;
            bagdat[bag].dir = -1;
            bagdat[bag].wt = 15;
            bagdat[bag].wobbling = false;
            clbits = game.drawing.DrawGold(bag, 0, bagdat[bag].x, bagdat[bag].y);
            game.IncrementPenalty();
            for (bn = 1, b = 2; bn < 8; bn++, b <<= 1)
                if ((b & clbits) != 0)
                    RemoveBag(bn);
        }

        public int BagY(int bag)
        {
            return bagdat[bag].y;
        }

        public void CleanupBags()
        {
            int bpa;
            game.sound.soundfalloff();
            for (bpa = 1; bpa < 8; bpa++)
            {
                if (bagdat[bpa].exist && ((bagdat[bpa].h == 7 && bagdat[bpa].v == 9) ||
                    bagdat[bpa].xr != 0 || bagdat[bpa].yr != 0 || bagdat[bpa].gt != 0 ||
                    bagdat[bpa].fallh != 0 || bagdat[bpa].wobbling))
                {
                    bagdat[bpa].exist = false;
                    game.sprite.EraseSprite(bpa);
                }
                if (game.currentPlayer == 0)
                    bagdat1[bpa] = bagdat[bpa];
                else
                    bagdat2[bpa] = bagdat[bpa];
            }
        }

        public void DoBags()
        {
            int bag;
            bool soundfalloffflag = true, soundwobbleoffflag = true;
            for (bag = 1; bag < 8; bag++)
                if (bagdat[bag].exist)
                {
                    if (bagdat[bag].gt != 0)
                    {
                        if (bagdat[bag].gt == 1)
                        {
                            game.sound.soundbreak();
                            game.drawing.DrawGold(bag, 4, bagdat[bag].x, bagdat[bag].y);
                            game.IncrementPenalty();
                        }
                        if (bagdat[bag].gt == 3)
                        {
                            game.drawing.DrawGold(bag, 5, bagdat[bag].x, bagdat[bag].y);
                            game.IncrementPenalty();
                        }
                        if (bagdat[bag].gt == 5)
                        {
                            game.drawing.DrawGold(bag, 6, bagdat[bag].x, bagdat[bag].y);
                            game.IncrementPenalty();
                        }
                        bagdat[bag].gt++;
                        if (bagdat[bag].gt == goldtime)
                            RemoveBag(bag);
                        else
                          if (bagdat[bag].v < 9 && bagdat[bag].gt < goldtime - 10)
                            if ((game.monster.GetField(bagdat[bag].h, bagdat[bag].v + 1) & 0x2000) == 0)
                                bagdat[bag].gt = goldtime - 10;
                    }
                    else
                        UpdateBag(bag);
                }
            for (bag = 1; bag < 8; bag++)
            {
                if (bagdat[bag].dir == 6 && bagdat[bag].exist)
                    soundfalloffflag = false;
                if (bagdat[bag].dir != 6 && bagdat[bag].wobbling && bagdat[bag].exist)
                    soundwobbleoffflag = false;
            }
            if (soundfalloffflag)
                game.sound.soundfalloff();
            if (soundwobbleoffflag)
                game.sound.soundwobbleoff();
        }

        public void DrawBags()
        {
            int bag;
            for (bag = 1; bag < 8; bag++)
            {
                if (game.currentPlayer == 0)
                    bagdat[bag] = bagdat1[bag];
                else
                    bagdat[bag] = bagdat2[bag];
                if (bagdat[bag].exist)
                    game.sprite.MoveDrawSprite(bag, bagdat[bag].x, bagdat[bag].y);
            }
        }

        public int GetBagDir(int bag)
        {
            if (bagdat[bag].exist)
                return bagdat[bag].dir;
            return -1;
        }

        public void GetGold(int bag)
        {
            int clbits;
            clbits = game.drawing.DrawGold(bag, 6, bagdat[bag].x, bagdat[bag].y);
            game.IncrementPenalty();
            if ((clbits & 1) != 0)
            {
                game.scores.ScoreGold();
                game.sound.soundgold();
                game.digger.time = 0;
            }
            else
                game.monster.MonsterGetGold();
            RemoveBag(bag);
        }

        public int GetMovingBagsCount()
        {
            int bag, n = 0;
            for (bag = 1; bag < 8; bag++)
                if (bagdat[bag].exist && bagdat[bag].gt < 10 &&
                    (bagdat[bag].gt != 0 || bagdat[bag].wobbling))
                    n++;
            return n;
        }

        public void InitBags()
        {
            int bag, x, y;
            pushcount = 0;
            goldtime = 150 - game.level.LevelOf10() * 10;
            for (bag = 1; bag < 8; bag++)
                bagdat[bag].exist = false;
            bag = 1;
            for (x = 0; x < 15; x++)
            {
                for (y = 0; y < 10; y++)
                {
                    if (game.level.GetChar(x, y, game.level.LevelPlan()) == 'B')
                    {
                        if (bag < 8)
                        {
                            bagdat[bag].exist = true;
                            bagdat[bag].gt = 0;
                            bagdat[bag].fallh = 0;
                            bagdat[bag].dir = -1;
                            bagdat[bag].wobbling = false;
                            bagdat[bag].wt = 15;
                            bagdat[bag].unfallen = true;
                            bagdat[bag].x = x * 20 + 12;
                            bagdat[bag].y = y * 18 + 18;
                            bagdat[bag].h = x;
                            bagdat[bag].v = y;
                            bagdat[bag].xr = 0;
                            bagdat[bag++].yr = 0;
                        }
                    }
                }
            }

            if (game.currentPlayer == 0)
                for (int i = 1; i < 8; i++)
                    bagdat1[i] = bagdat[i];
            else
                for (int i = 1; i < 8; i++)
                    bagdat2[i] = bagdat[i];
        }

        public bool PushBag(int bag, int dir)
        {
            int x, y, h, v, ox, oy, clbits;
            bool push = true;
            ox = x = bagdat[bag].x;
            oy = y = bagdat[bag].y;
            h = bagdat[bag].h;
            v = bagdat[bag].v;
            if (bagdat[bag].gt != 0)
            {
                GetGold(bag);
                return true;
            }
            if (bagdat[bag].dir == 6 && (dir == 4 || dir == 0))
            {
                clbits = game.drawing.DrawGold(bag, 3, x, y);
                game.IncrementPenalty();
                if (((clbits & 1) != 0) && (game.digger.y >= y))
                    game.digger.KillDigger(1, bag);
                if ((clbits & 0x3f00) != 0)
                    game.monster.SquashMonsters(bag, clbits);
                return true;
            }
            if ((x == 292 && dir == 0) || (x == 12 && dir == 4) || (y == 180 && dir == 6) ||
                (y == 18 && dir == 2))
                push = false;
            if (push)
            {
                switch (dir)
                {
                    case 0:
                        x += 4;
                        break;
                    case 4:
                        x -= 4;
                        break;
                    case 6:
                        if (bagdat[bag].unfallen)
                        {
                            bagdat[bag].unfallen = false;
                            game.drawing.DrawSquareBlob(x, y);
                            game.drawing.DrawTopBlob(x, y + 21);
                        }
                        else
                            game.drawing.DrawFurryBlob(x, y);
                        game.drawing.EatField(x, y, dir);
                        game.digger.KillEmerald(h, v);
                        y += 6;
                        break;
                }
                switch (dir)
                {
                    case 6:
                        clbits = game.drawing.DrawGold(bag, 3, x, y);
                        game.IncrementPenalty();
                        if (((clbits & 1) != 0) && game.digger.y >= y)
                            game.digger.KillDigger(1, bag);
                        if ((clbits & 0x3f00) != 0)
                            game.monster.SquashMonsters(bag, clbits);
                        break;
                    case 0:
                    case 4:
                        bagdat[bag].wt = 15;
                        bagdat[bag].wobbling = false;
                        clbits = game.drawing.DrawGold(bag, 0, x, y);
                        game.IncrementPenalty();
                        pushcount = 1;
                        if ((clbits & 0xfe) != 0)
                            if (!PushBags(dir, clbits))
                            {
                                x = ox;
                                y = oy;
                                game.drawing.DrawGold(bag, 0, ox, oy);
                                game.IncrementPenalty();
                                push = false;
                            }
                        if (((clbits & 1) != 0) || ((clbits & 0x3f00) != 0))
                        {
                            x = ox;
                            y = oy;
                            game.drawing.DrawGold(bag, 0, ox, oy);
                            game.IncrementPenalty();
                            push = false;
                        }
                        break;
                }
                if (push)
                    bagdat[bag].dir = dir;
                else
                    bagdat[bag].dir = game.ReverseDir(dir);

                bagdat[bag].x = x;
                bagdat[bag].y = y;
                bagdat[bag].h = (x - 12) / 20;
                bagdat[bag].v = (y - 18) / 18;
                bagdat[bag].xr = (x - 12) % 20;
                bagdat[bag].yr = (y - 18) % 18;
            }
            return push;
        }

        public bool PushBags(int dir, int bits)
        {
            int bag, bit;
            bool push = true;
            for (bag = 1, bit = 2; bag < 8; bag++, bit <<= 1)
                if ((bits & bit) != 0)
                    if (!PushBag(bag, dir))
                        push = false;
            return push;
        }

        public bool PushUpdatedBags(int bits)
        {
            bool push = true;
            for (int bag = 1, b = 2; bag < 8; bag++, b <<= 1)
                if ((bits & b) != 0)
                    if (bagdat[bag].gt != 0)
                        GetGold(bag);
                    else
                        push = false;

            return push;
        }

        public void RemoveBag(int bag)
        {
            if (bagdat[bag].exist)
            {
                bagdat[bag].exist = false;
                game.sprite.EraseSprite(bag);
            }
        }

        public void RemoveBags(int bits)
        {
            int bag, b;
            for (bag = 1, b = 2; bag < 8; bag++, b <<= 1)
                if ((bagdat[bag].exist) && ((bits & b) != 0))
                    RemoveBag(bag);
        }

        public void UpdateBag(int bag)
        {
            int x = bagdat[bag].x;
            int h = bagdat[bag].h;
            int xr = bagdat[bag].xr;
            int y = bagdat[bag].y;
            int v = bagdat[bag].v;
            int yr = bagdat[bag].yr;
            switch (bagdat[bag].dir)
            {
                case -1:
                    if (y < 180 && xr == 0)
                    {
                        if (bagdat[bag].wobbling)
                        {
                            if (bagdat[bag].wt == 0)
                            {
                                bagdat[bag].dir = 6;
                                game.sound.soundfall();
                                break;
                            }
                            bagdat[bag].wt--;
                            int wbl = bagdat[bag].wt % 8;
                            if (!((wbl & 1) != 0))
                            {
                                game.drawing.DrawGold(bag, wblanim[wbl >> 1], x, y);
                                game.IncrementPenalty();
                                game.sound.soundwobble();
                            }
                        }
                        else
                          if ((game.monster.GetField(h, v + 1) & 0xfdf) != 0xfdf)
                            if (!game.digger.IsDiggerUnderBag(h, v + 1))
                                bagdat[bag].wobbling = true;
                    }
                    else
                    {
                        bagdat[bag].wt = 15;
                        bagdat[bag].wobbling = false;
                    }
                    break;
                case 0:
                case 4:
                    if (xr == 0)
                        if (y < 180 && (game.monster.GetField(h, v + 1) & 0xfdf) != 0xfdf)
                        {
                            bagdat[bag].dir = 6;
                            bagdat[bag].wt = 0;
                            game.sound.soundfall();
                        }
                        else
                            BagHitGround(bag);
                    break;
                case 6:
                    if (yr == 0)
                        bagdat[bag].fallh++;
                    if (y >= 180)
                        BagHitGround(bag);
                    else
                      if ((game.monster.GetField(h, v + 1) & 0xfdf) == 0xfdf)
                        if (yr == 0)
                            BagHitGround(bag);
                    game.monster.CheckMonsterScared(bagdat[bag].h);
                    break;
            }
            if (bagdat[bag].dir != -1)
                if (bagdat[bag].dir != 6 && pushcount != 0)
                    pushcount--;
                else
                    PushBag(bag, bagdat[bag].dir);
        }
    }
}