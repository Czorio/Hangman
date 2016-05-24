using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangman
{
    class ComputerPlayer
    {
        // Experimental prediction
        private Dictionary<char, int> occurences;
        private const float guessTreshold = .75f;
        private string knownPattern;

        private List<string> possibleWords;
        private List<char> usedLetters;
        private int lengthOfWord;

        private const bool useExperimental = true;

        public ComputerPlayer(string filePath)
        {
            occurences = new Dictionary<char, int>();
            possibleWords = new List<string>();
            usedLetters = new List<char>();
            readWordsFromFile(filePath);
        }

        /// <summary>
        /// Plays a turn
        /// </summary>
        public void playTurn()
        {
            // Checks to see if there is only one word left
            if (possibleWords.Count == 1)
            {
                Console.WriteLine("Is your word: {0}? Y/N", possibleWords[0]);
                switch (Console.ReadKey().KeyChar)
                {
                    case 'y':
                        Hangman.gs.endGame("Found your word!");
                        return;
                    case 'n':
                        Hangman.gs.endGame("Could not find your word!");
                        return;
                    default:
                        Console.WriteLine("Wrong input");
                        throw new IOException();
                }
            }

            if (findMaxSimilarity().Value > guessTreshold)
            {
                Console.WriteLine("Is your word: {0}? Y/N", possibleWords[findMaxSimilarity().Key]);
                switch (Console.ReadKey().KeyChar)
                {
                    case 'y':
                        Hangman.gs.endGame("Found your word!");
                        return;
                    case 'n':
                        Console.WriteLine("Shame!");
                        possibleWords.RemoveAt(findMaxSimilarity().Key);
                        return;
                    default:
                        Console.WriteLine("Wrong input");
                        throw new IOException();
                }
            }

            // Ask the user if a certain character is in the word they are looking for
            char letter = getLettertoRequest();
            Console.WriteLine("Does your word contain the letter {0}? Y/N", letter);
            switch (Console.ReadKey().KeyChar)
            {
                case 'y':
                    filterByHavingLetter(letter);
                    Console.WriteLine("\nPlease type the position of the letter. Ex: r.r. You do not have to repeat previous letters.");
                    filterByPattern(Console.ReadLine());
                    break;
                case 'n':
                    filterByNotHavingLetter(letter);
                    break;
                default:
                    Console.WriteLine("Wrong input");
                    throw new IOException();
            }
        }

        private KeyValuePair<int, float> findMaxSimilarity()
        {
            float max = .0f;
            int index = -1;
            for (int i = 0; i < possibleWords.Count; i++)
            {
                if (calcSimilarityToKnown(possibleWords[i]) > max)
                {
                    max = calcSimilarityToKnown(possibleWords[i]);
                    index = i;
                }
            }
            return new KeyValuePair<int, float>(index, max);
        }

        private float calcSimilarityToKnown(string word)
        {
            int matches = 0;
            char[] kp = knownPattern.ToCharArray();
            char[] w = word.ToCharArray();

            for (int i = 0; i < kp.Length; i++)
            {
                if (kp[i] == w[i])
                {
                    matches++;
                }
            }

            return matches / knownPattern.Length;
        }

        /// <summary>
        /// Reads a list of words from a file
        /// </summary>
        /// <param name="filePath"></param>
        private void readWordsFromFile(string filePath)
        {
            StreamReader sr;
            try
            {
                 sr = new StreamReader(filePath);
            }
            catch (IOException e)
            {

                throw e;
            }
            Regex regex = new Regex("[a-zA-Z]+");

            string tmp;

            while ((tmp = sr.ReadLine()) != null)
            {
                if (regex.IsMatch(tmp))
                {
                    possibleWords.Add(tmp.ToLower()); 
                }
            }

            countOccurences();
        }

        /// <summary>
        /// Counts the amount of times a certain letter appears in all words.
        /// </summary>
        private void countOccurences()
        {
            occurences.Clear();
            foreach (string item in possibleWords)
            {
                for (int i = 0; i < item.Length; i++) // O(n^2), oh dear
                {
                    if (occurences.ContainsKey(item.ToCharArray()[i]))
                    {
                        occurences[item.ToCharArray()[i]]++;
                    }
                    else
                    {
                        occurences.Add(item.ToCharArray()[i], 1);
                    }
                }
            }
        }

        /// <summary>
        /// Removes all words from the possible list that are not the required length
        /// </summary>
        /// <param name="length">Length of words that are not removed</param>
        public void filterByLength(int length)
        {
            if (knownPattern == null)
            {
                knownPattern = "";
                for (int i = 0; i < length; i++)
                {
                    knownPattern += ".";
                }
            }

            lengthOfWord = length;
            possibleWords.RemoveAll(x => x.Length != length);
            countOccurences();
        }

        /// <summary>
        /// Removes all words that do not fit the pattern.
        /// An example of a pattern: ..c..c
        /// </summary>
        /// <param name="pattern">pattern to filter on</param>
        public void filterByPattern(string pattern)
        {
            // trims the pattern if it is too long
            if (pattern.Length != lengthOfWord)
            {
                string tmp = "";
                for (int i = 0; i < lengthOfWord - 1; i++)
                {
                    tmp += pattern.ToCharArray()[i];
                }
                pattern = tmp;
            }

            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern.ToCharArray()[i] != '.')
                {
                    possibleWords.RemoveAll(x => x.ToCharArray()[i] != pattern.ToCharArray()[i]);
                }
            }
            countOccurences();
            addToKnownPattern(pattern);
        }

        /// <summary>
        /// Combines the user provided pattern with the known pattern
        /// </summary>
        /// <param name="pattern">pattern to add to known pattern</param>
        private void addToKnownPattern(string pattern)
        {
            char[] kp = knownPattern.ToCharArray();
            char[] p = pattern.ToCharArray();
            for (int i = 0; i < knownPattern.Length; i++)
            {
                if (kp[i] == '.' && p[i] != '.')
                {
                    kp[i] = p[i];
                }
            }
        }

        /// <summary>
        /// Removes all words that contain the letter
        /// </summary>
        /// <param name="c">letter that is not in the word to be found</param>
        public void filterByNotHavingLetter(char c)
        {
            possibleWords.RemoveAll(x => x.Contains(c));
            countOccurences();
        }

        /// <summary>
        /// removes all words that do not contain the letter
        /// </summary>
        /// <param name="c">letter that is in the word to be found</param>
        public void filterByHavingLetter(char c)
        {
            possibleWords.RemoveAll(x => !x.Contains(c));
            countOccurences();
        }

        /// <summary>
        /// Finds an unasked letter from a random word in the list; if this fails will return '0'
        /// </summary>
        /// <returns>A not yet asked letter from a random word. '0' if fail</returns>
        public char getLettertoRequest()
        {
            if (useExperimental)
            {
                char c = '0';
                int max = 0;
                foreach (KeyValuePair<char, int> item in occurences)
                {
                    if (occurences[item.Key] > max && !usedLetters.Contains(item.Key))
                    {
                        c = item.Key;
                        max = occurences[item.Key];
                        usedLetters.Add(item.Key);
                    }
                }
                countOccurences();
                return c;
            }
            else
            {
                Random r = new Random();
                string word = possibleWords[r.Next(possibleWords.Count - 1)];

                for (int i = 0; i < word.Length; i++)
                {
                    if (!usedLetters.Contains(word.ToCharArray()[i]))
                    {
                        usedLetters.Add(word.ToCharArray()[i]);
                        return word.ToCharArray()[i];
                    }
                }
                countOccurences();
                return '0';
            }
        }
    }
}
