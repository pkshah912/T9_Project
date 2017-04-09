
using System;
/// Author: Pooja Shah
/// Date of Creation: March 15, 2017
/// This program is used to create a Model for T9 Messager using MVVM design pattern
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Model
{
    /// <summary>
    /// A Model class for T9 Messager
    /// </summary>
    class T9Model
    {
        /// <summary>
        /// This is used to store the characters that appear in the keypad when in non-predictive mode
        /// </summary>
        Dictionary<int, List<char>> numpad;

        /// <summary>
        /// This is used to determine the time limit for multiple clicks of a button
        /// </summary>
        int waitTimeBetweenClicks = 1000;

        /// <summary>
        /// It keeps track of when was the last time button was clicked
        /// </summary>
        long lastClickTime = 0;

        /// <summary>
        /// Keeps track of the count of the number of clicks
        /// </summary>
        int numberOfClicks = 0;

        int lastButtonClick = 0;

        /// <summary>
        /// This is used to keep the dictionary to map the characters to corresponding number on keypad
        /// </summary>
        Dictionary<char, string> mappingCharacters = new Dictionary<char, string>
        {
            {'a', "2" }, {'b', "2" }, {'c', "2" },
            {'d', "3" }, {'e', "3" }, {'f', "3" },
            {'g', "4" }, {'h', "4" }, {'i', "4" },
            {'j', "5" }, {'k', "5" }, {'l', "5" },
            {'m', "6" }, {'n', "6" }, {'o', "6" },
            {'p', "7" }, {'q', "7" }, {'r', "7" }, {'s', "7" },
            {'t', "8" }, {'u', "8" }, {'v', "8" },
            {'w', "9" }, {'x', "9" }, {'y', "9" }, {'z', "9" }
        };

        /// <summary>
        /// Used to store the key combinations of each english word from the file
        /// </summary>
        Dictionary<string, string> wordDictionary;

        /// <summary>
        /// This is a constructor
        /// </summary>
        public T9Model()
        {
            /// Stores the corresponding characters of the keypad at a particular position
            numpad = new Dictionary<int, List<char>>();
            numpad[2] = new List<char> { 'a', 'b', 'c', '2' };
            numpad[3] = new List<char> { 'd', 'e', 'f', '3' };
            numpad[4] = new List<char> { 'g', 'h', 'i', '4' };
            numpad[5] = new List<char> { 'j', 'k', 'l', '5' };
            numpad[6] = new List<char> { 'm', 'n', 'o', '6' };
            numpad[7] = new List<char> { 'p', 'q', 'r', 's', '7' };
            numpad[8] = new List<char> { 't', 'u', 'v', '8' };
            numpad[9] = new List<char> { 'w', 'x', 'y', 'z', '9' };

            /// Reads the file
            string[] wordLines = File.ReadAllLines(".\\english-words.txt");
            wordDictionary = new Dictionary<string, string>();

            /// Add each word and its corresponding numbers in the dictionary
            foreach (string word in wordLines)
            {
                string wordKey = generateKey(word);
                wordDictionary.Add(word, wordKey);
            }
        }

        /// <summary>
        /// Generate the number combination of word
        /// </summary>
        /// <param name="word"></param>
        /// <returns>
        /// wordKey => returns the number combination of a particular word
        /// </returns>
        public string generateKey(string word)
        {
            string wordKey = "";
            for (int i = 0; i < word.Length; i++)
            {
                wordKey += mappingCharacters[word[i]];
            }
            return wordKey;
        }

        /// <summary>
        /// Used for non-predictive mode
        /// </summary>
        /// <param name="currTime"></param>
        /// <param name="buttonClicked"></param>
        /// <param name="text"></param>
        /// <returns>
        /// nonPredictiveWord => Returns the non predictive word for display
        /// </returns>
        public string nonPredictiveClick(long currTime, int buttonClicked, string text)
        {
            /// Initialize the non predictive word to be displayed
            string nonPredictiveWord = "";

            /// Checks if the time between two clicks is greater than 1000 ms or if it is the first click
            if (currTime - lastClickTime > waitTimeBetweenClicks || lastClickTime == 0)
            {
                /// Checks if a particular button clicked number is present in the dictionary,
                /// If yes, then it takes appends the text to the non predictive word by taking
                /// the first word from the list in the dictionary
                /// It also initializes the number of clicks to zero 
                /// and updates the last click time to current timer

                if (numpad.ContainsKey(buttonClicked))
                {
                    checkLastButtonClick(buttonClicked);
                    numberOfClicks++;
                    lastClickTime = currTime;
                    List<char> characters = numpad[buttonClicked];
                    nonPredictiveWord = text + characters.ElementAt(0);
                }
            }
            /// Checks if the time between two clicks is less than 1000 ms
            /// If yes, then it increments the number of clicks and 
            /// takes the mod of the number of characters a clicked button has
            /// to make a cycle for clicks
            else
            {
                if (numpad.ContainsKey(buttonClicked))
                {
                    checkLastButtonClick(buttonClicked);
                    List<char> characters = numpad[buttonClicked];
                    numberOfClicks = (numberOfClicks) % characters.Count;
                    lastClickTime = currTime;
                    nonPredictiveWord = text.Substring(0, text.Length - 1) + characters.ElementAt(numberOfClicks);
                    numberOfClicks++;
                }
            }
            return nonPredictiveWord;
        }

        /// <summary>
        /// Checks for the last button clicks and current button click
        /// and resets the numberOfClicks counter to 0 
        /// to keep a track of the letter to be displayed
        /// </summary>
        /// <param name="buttonClick"></param>
        public void checkLastButtonClick(int buttonClick)
        {
            if (lastButtonClick == 0)
            {
                lastButtonClick = buttonClick;
            }
            else if (buttonClick != lastButtonClick && lastButtonClick != 0)
            {
                numberOfClicks = 0;
                lastButtonClick = buttonClick;
            }
        }

        /// <summary>
        /// Gets the predictive words based on the button click
        /// </summary>
        /// <param name="buttonNumberKey"></param>
        /// <param name="page"></param>
        /// <returns>
        /// Returns an array of the words that matches or begins with the buttons clicked
        /// </returns>
        public List<string> predictiveClick(string buttonNumberKey, int page)
        {
            List<string> tempList = new List<string>();
            List<string> predictedWordsList = new List<string>();
            /// Get the words that matches the buttons clicked
            var getExactMatchWords = from words in wordDictionary
                                     where words.Equals(buttonNumberKey)
                                     select words.Key;

            tempList.AddRange(getExactMatchWords);

            /// Get the words that starts with the buttons clicked
            var getPrefixWords = from words in wordDictionary
                                 where words.Value.StartsWith(buttonNumberKey)
                                 select words.Key;

            tempList.AddRange(getPrefixWords);

            /// Arranges the list in order by length and lexicographically
            predictedWordsList = tempList.OrderBy(s => s.Length).ThenBy(s => s).ToList();

            return predictedWordsList.Skip(page * 10).Take(10).ToList();
        }

        /// <summary>
        /// Gets the words that begins with a particular word
        /// </summary>
        /// <param name="word"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<string> getWords(string word, int page)
        {
            List<string> tempList = new List<string>();
            List<string> predictedWordsList = new List<string>();

            var getPrefixWords = from words in wordDictionary
                                 where words.Key.StartsWith(word)
                                 select words.Key;
            tempList.AddRange(getPrefixWords);

            predictedWordsList = tempList.OrderBy(s => s.Length).ThenBy(s => s).ToList();

            return predictedWordsList.Skip(page * 10).Take(10).ToList();
        }

        /// <summary>
        /// Gets the words when dealing with backspace and then clicking the button
        /// </summary>
        /// <param name="key"></param>
        /// <param name="word"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public List<string> getElements(string key, string word, int page)
        {
            List<string> list = new List<string>();
            List<string> tempList = new List<string>();
            List<string> predictedWordsList = new List<string>();

            /// Gets the characters that matches the button number clicked
            var getChar = mappingCharacters.Where(p => p.Value == key).Select(p => p.Key);

            /// Appends the button click characters to the word to get the matches and prefix matches
            foreach (var character in getChar)
            {
                list.Add(word + character);
            }

            foreach (string temp in list)
            {
                var getPrefixWords = from words in wordDictionary
                                     where words.Key.StartsWith(temp)
                                     select words.Key;
                tempList.AddRange(getPrefixWords);
            }
            predictedWordsList = tempList.OrderBy(s => s.Length).ThenBy(s => s).ToList();

            return predictedWordsList.Skip(page * 10).Take(10).ToList();
        }
    }
}
