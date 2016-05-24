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
            Console.WriteLine("Please select a language (Program will stil display in English)");
            gs = new GameState(Console.ReadLine() + ".txt");
            gs.startGame();
            Console.Read();
        }
    }
}
