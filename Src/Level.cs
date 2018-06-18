using System.IO;
using System.Text;

namespace Digger
{
    public class Level
    {
        private readonly Game game;

        public bool isUsingLevelFile;
        public string levelFilePath;

        private static readonly string[,] LevelData = {
            {"S   B     HHHHS",
            "V  CC  C  V B  ",
            "VB CC  C  V    ",
            "V  CCB CB V CCC",
            "V  CC  C  V CCC",
            "HH CC  C  V CCC",
            " V    B B V    ",
            " HHHH     V    ",
            "C   V     V   C",
            "CC  HHHHHHH  CC"},
            {"SHHHHH  B B  HS",
            " CC  V       V ",
            " CC  V CCCCC V ",
            "BCCB V CCCCC V ",
            "CCCC V       V ",
            "CCCC V B  HHHH ",
            " CC  V CC V    ",
            " BB  VCCCCV CC ",
            "C    V CC V CC ",
            "CC   HHHHHH    "},
            {"SHHHHB B BHHHHS",
            "CC  V C C V BB ",
            "C   V C C V CC ",
            " BB V C C VCCCC",
            "CCCCV C C VCCCC",
            "CCCCHHHHHHH CC ",
            " CC  C V C  CC ",
            " CC  C V C     ",
            "C    C V C    C",
            "CC   C H C   CC"},
            {"SHBCCCCBCCCCBHS",
            "CV  CCCCCCC  VC",
            "CHHH CCCCC HHHC",
            "C  V  CCC  V  C",
            "   HHH C HHH   ",
            "  B  V B V  B  ",
            "  C  VCCCV  C  ",
            " CCC HHHHH CCC ",
            "CCCCC CVC CCCCC",
            "CCCCC CHC CCCCC"},
            {"SHHHHHHHHHHHHHS",
            "VBCCCCBVCCCCCCV",
            "VCCCCCCV CCBC V",
            "V CCCC VCCBCCCV",
            "VCCCCCCV CCCC V",
            "V CCCC VBCCCCCV",
            "VCCBCCCV CCCC V",
            "V CCBC VCCCCCCV",
            "VCCCCCCVCCCCCCV",
            "HHHHHHHHHHHHHHH"},
            {"SHHHHHHHHHHHHHS",
            "VCBCCV V VCCBCV",
            "VCCC VBVBV CCCV",
            "VCCCHH V HHCCCV",
            "VCC V CVC V CCV",
            "VCCHH CVC HHCCV",
            "VC V CCVCC V CV",
            "VCHHBCCVCCBHHCV",
            "VCVCCCCVCCCCVCV",
            "HHHHHHHHHHHHHHH"},
            {"SHCCCCCVCCCCCHS",
            " VCBCBCVCBCBCV ",
            "BVCCCCCVCCCCCVB",
            "CHHCCCCVCCCCHHC",
            "CCV CCCVCCC VCC",
            "CCHHHCCVCCHHHCC",
            "CCCCV CVC VCCCC",
            "CCCCHH V HHCCCC",
            "CCCCCV V VCCCCC",
            "CCCCCHHHHHCCCCC"},
            {"HHHHHHHHHHHHHHS",
            "V CCBCCCCCBCC V",
            "HHHCCCCBCCCCHHH",
            "VBV CCCCCCC VBV",
            "VCHHHCCCCCHHHCV",
            "VCCBV CCC VBCCV",
            "VCCCHHHCHHHCCCV",
            "VCCCC V V CCCCV",
            "VCCCCCV VCCCCCV",
            "HHHHHHHHHHHHHHH"}};

        public string[,] Data = (string[,])LevelData.Clone();

        public Level(Game game)
        {
            this.game = game;
        }

        public int GetChar(int x, int y, int l)
        {
            if (l == 0)
                l++;

            return Data[l - 1, y][x];
        }

        public int LevelPlan()
        {
            int l = game.Level;
            if (l > 8)
                return (l & 3) + 5; /* Level plan: 12345678, 678, (5678) 247 times, 5 forever */

            return l;
        }

        public int LevelOf10()
        {
            int level = game.Level;
            return level > 10 ? 10 : level;
        }

        public void ReadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                path += ".DLF";
                if (!File.Exists(path))
                    throw new FileNotFoundException($"File '{path}' cannot be found.");
            }

            using (var levf = File.OpenRead(path))
            {
                using (var br = new BinaryReader(levf, Encoding.ASCII, true))
                {
                    game.scores.bonusscore = br.ReadInt32();
                }

                byte[] buff = new byte[15];
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        levf.Read(buff, 0, 15);
                        Data[i, j] = Encoding.ASCII.GetString(buff);
                    }
                }
            }

            levelFilePath = path;
            isUsingLevelFile = true;
        }

        public void RestoreData()
        {
            for (int i = 0; i < LevelData.GetLength(0); ++i)
                for (int j = 0; j < LevelData.GetLength(1); ++j)
                    Data[i, j] = LevelData[i, j];
        }
    }
}
