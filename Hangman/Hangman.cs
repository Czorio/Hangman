using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman
{
    class Hangman
    {
        public static GameState gs;
        static void Main(string[] args)
        {
            gs = new GameState("nl_NL.txt");
            gs.startGame();
            Console.Read();
        }
    }
}
