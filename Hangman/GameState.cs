using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman
{
    class GameState
    {
        private ComputerPlayer CPU;
        private bool hasFoundWord;

        private const int maxTurns = 10;


        public GameState(string wordsFile)
        {
            CPU = new ComputerPlayer(wordsFile);
            hasFoundWord = false;
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        public void startGame()
        {
            // determine length of word
            Console.WriteLine("How long is your word?");
            int tmp;
            if (int.TryParse(Console.ReadLine(), out tmp))
            {
                CPU.filterByLength(tmp);
            }
            else
            {
                Console.WriteLine("Faulty input");
                throw new IOException();
            }

            int currentTurn = 0;

            // keep playing turns untill word is found or the limit of turns is reached
            while (!hasFoundWord && currentTurn < maxTurns)
            {
                turn(currentTurn);
                currentTurn++;
            }

            Console.WriteLine("Game Ended!");
        }

        /// <summary>
        /// plays a turn
        /// </summary>
        /// <param name="turn">which turn it is</param>
        private void turn(int turn)
        {
            Console.WriteLine("\nTurn {0}", turn + 1);
            CPU.playTurn();
        }

        /// <summary>
        /// Ends the game
        /// </summary>
        /// <param name="message">The message to display on end of game</param>
        public void endGame(string message)
        {
            Console.WriteLine(message);
            hasFoundWord = true;
        }
    }
}
