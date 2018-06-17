using System.Windows.Forms;

namespace Digger.Win32
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game(new Timer());
            var window = new Window(game);

            game.ParseCmdLine(args);
            game.Start();
            Application.Run(window);
            game.Exit();
        }
    }
}
