/// Author: Pooja Shah
/// Date of Creation: March 15, 2017
/// This program is used to create a ViewModel for T9 Messager using MVVM design pattern

using Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Windows.Input;
using T9_Project2.ViewModel;

namespace ViewModel
{
    /// <summary>
    /// A ViewModel class for T9 Messager
    /// </summary>
    public class T9ViewModel : INotifyPropertyChanged
    {
        private T9Model model = new T9Model();
        /// <summary>
        /// Stores the time when button is clicked in non-predictive mode
        /// </summary>
        long time;

        /// <summary>
        /// Binds the rich text box in view with the view model for non predictive mode
        /// </summary>
        private string textBoxText { get; set; }

        /// <summary>
        /// Binds the rich text box in view with the view model for predictive mode
        /// </summary>
        private string predictedText { get; set; }

        /// <summary>
        /// Binds the predictive check box in view with the view model
        /// </summary>
        private bool _isChecked;

        /// <summary>
        /// Binds the button in view with view model for click event
        /// </summary>
        private ButtonCommand buttonCommand;

        /// <summary>
        /// Index to get the predictive word from list
        /// </summary>
        int count = 1;

        /// <summary>
        /// To apply paging to the list to fetch 10 words from the list
        /// </summary>
        int page = 0;

        /// <summary>
        /// Keeps track of the button click numbers 
        /// </summary>
        string key = "";

        /// <summary>
        /// Returns the list of words that are predicted on button click
        /// </summary>
        List<string> predictedWords;

        /// <summary>
        /// Keeps track of the words that are in the textbox
        /// </summary>
        string[] textBoxWords;

        /// <summary>
        /// Binds the checkbox in the view with the view model
        /// </summary>
        bool checkBoxChecked = false;

        /// <summary>
        /// Used on click of backspace after the word has been accepted
        /// </summary>
        bool isWord = false;

        /// <summary>
        /// Keeps track of the current word so if user presses backspace it becomes current word
        /// </summary>
        string currentWord = "";

        /// <summary>
        /// Property that binds the textbox in the view with the view model
        /// Notifies the property change if textbox is updated
        /// </summary>
        public string Text
        {
            get
            {
                return textBoxText;
            }
            set
            {
                textBoxText = value;
                NotifyPropertyChanged("Text");
            }
        }

        /// <summary>
        /// Property that binds the predicted text in the view with the view model
        /// Notifies the property change if predicted text is updated
        /// </summary>
        public string PredictedText
        {
            get
            {
                return predictedText;
            }
            set
            {
                predictedText = value;
                NotifyPropertyChanged("PredictedText");
            }
        }

        /// <summary>
        /// Property that binds the checkbox in the view with the view model
        /// Notifies the property change on toggling of check box
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                _isChecked = value;
                NotifyPropertyChanged("IsChecked");
                if (checkBoxChecked != _isChecked)
                {
                    Text = "";
                    PredictedText = "";
                    key = "";
                    isWord = false;
                }
            }
        }

        /// <summary>
        /// ICommand property that binds the button in the view with the view model
        /// </summary>
        public ICommand ButtonClickCommand
        {
            get
            {
                return buttonCommand;
            }
        }


        /// <summary>
        /// A default constructor
        /// </summary>
        public T9ViewModel()
        {
            Text = "";
            PredictedText = "";
            buttonCommand = new ButtonCommand(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Used to notify the property changed if any of the controls being binded is changed
        /// </summary>
        /// <param name="propertyName"></param>
        private void NotifyPropertyChanged(String propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the words when in predictive mode on click of a button
        /// </summary>
        public void callPredictiveClick()
        {
            /// Splits the text box into words
            textBoxWords = Text.Split(' ');

            /// Gets the previous text except the last word from the text box
            string previousText = "";
            for (int i = 0; i < textBoxWords.Length - 1; i++)
            {
                previousText += textBoxWords[i] + " ";
            }

            /// Keeps track of the last word in the text box
            currentWord = textBoxWords.Last();

            /// Checks if there were any predicted words based on the button clicks
            if (predictedWords.Count > 0)
            {
                /// Gets the first word from the predicted words dictionary
                string firstWord = predictedWords.ElementAt(0);

                /// Checks if the first word length is greater than the button clicks length,
                /// If yes, then it displays some part of the text as predicted text (gray)
                /// and some part of the text as actual text that the user presed (black)
                if (firstWord.Length > key.Length)
                {
                    int length;
                    int substringLength;

                    /// If the key is blank, it takes the first word of the predicted words list
                    /// and divides that word into actual text and predicted text
                    if (key == "")
                    {
                        length = firstWord.Length - currentWord.Length;
                        substringLength = currentWord.Length;
                        Text = previousText + firstWord.Substring(0, substringLength);
                        PredictedText = firstWord.Substring(substringLength, length);
                    }
                    else
                    {
                        /// Checks if the user pressed space and then pressed backspace
                        if (isWord == true)
                        {
                            /// Checks if the current word was invalid,
                            /// then takes the first word and the key to get the words
                            /// that starts with the first word + the key entered
                            if (currentWord.Contains('-'))
                            {
                                predictedWords = model.getElements(key, firstWord, page);
                            }
                            /// Checks if the current word was valid,
                            /// then takes the first word and the key to get the words
                            /// that starts with the current word + the key entered
                            else
                            {
                                predictedWords = model.getElements(key, currentWord, page);
                            }

                            /// If the list contains the predicted words after concatenating the word with button click,
                            /// it divides the text into different parts and 
                            /// displays the word into actual text and predicted text
                            if (predictedWords.Count > 0)
                            {
                                firstWord = predictedWords.ElementAt(0);
                                substringLength = key.Length + currentWord.Length;
                                Text = previousText + firstWord.Substring(0, substringLength);
                                length = firstWord.Length - substringLength;
                                PredictedText = firstWord.Substring(substringLength, length);
                            }

                            /// If the predicted list doesn't contain after concatenating the word with the button click,
                            /// it displays hyphen which tells the user that the word is invalid
                            else
                            {
                                int len = key.Length + currentWord.Length;
                                string hyphen = "";
                                for (int i = 0; i < len; i++)
                                {
                                    hyphen += "-";
                                }
                                Text = previousText + hyphen;
                                PredictedText = "";
                            }
                        }
                        else
                        {
                            length = firstWord.Length - key.Length;
                            substringLength = key.Length;
                            Text = previousText + firstWord.Substring(0, substringLength);
                            PredictedText = firstWord.Substring(substringLength, length);
                        }
                    }
                }
                /// If the first word length is equal to the button clicks length, 
                /// then it displays the actual word (black)
                else
                {
                    Text = previousText + firstWord;
                    PredictedText = "";
                }
            }

            /// If no predicted words based on the buttons user clicked,
            /// generates hyphen for the whole word to make the word invalid
            else
            {
                int length;
                if (isWord == true)
                {
                    length = Text.Split(' ').Last().Length + 1;
                }
                else
                {
                    length = key.Length;
                }

                string hyphen = "";
                for (int i = 0; i < length; i++)
                {
                    hyphen += "-";
                }
                Text = previousText + hyphen;
                PredictedText = "";
            }
        }

        /// <summary>
        /// Predicts the words based on button clicks
        /// </summary>
        /// <param name="buttonNumber"></param>
        public void predictWords(int buttonNumber)
        {
            /// Checks if the last character of the text box contains hyphen,
            /// then the system generates a beep
            if (Text.Length > 0 && Text[Text.Length - 1] == '-')
            {
                SystemSounds.Beep.Play();
            }
            else
            {
                /// Keeps track of the button clicks
                key += buttonNumber;

                /// Checks if the user typed space and then backspace
                if (isWord == true)
                {
                    /// Checks if the key length is greater than 1,
                    /// if yes, then fetch only the last character of the key
                    if (key.Length > 1)
                    {
                        key = key[key.Length - 1].ToString();
                    }

                    /// If the current word contains an hyphen,
                    /// then it gets the predicted word list based on the last word on the text + key
                    if (currentWord.Contains('-'))
                    {
                        predictedWords = model.getElements(key, Text.Split(' ').Last(), page);
                    }
                    /// Else it gets the predicted word list based on the last word on the current word + key
                    else
                    {
                        predictedWords = model.getElements(key, currentWord, page);
                    }
                }
                /// Else it gets the predicted word list based on the last word on the button clicks
                else
                {
                    predictedWords = model.predictiveClick(key, page);
                }
                callPredictiveClick();
            }
        }

        /// <summary>
        /// Gets the word to display on the text box
        /// </summary>
        /// <param name="buttonClicked"></param>
        public void getText(int buttonClicked)
        {
            /// Used for non-predictive mode
            if (IsChecked == false)
            {
                checkBoxChecked = false;
                /// Checks for the button click '1'
                if (buttonClicked == 1)
                {
                    Text += "1";
                }
                /// Checks for the button click '0'
                else if (buttonClicked == 0)
                {
                    Text += "0";
                }
                /// Checks for the button click '#'
                else if (buttonClicked == 11)
                {
                    Text += " ";
                }
                /// Checks for the button click '*'
                /// and display appropriate character on the textbox
                else if (buttonClicked == 10)
                {
                    if (Text != "")
                    {
                        Text = Text.Substring(0, Text.Length - 1);
                    }
                }
                /// Calls the non-predictive mode for the buttons clicked
                /// and display appropriate character on the textbox
                else
                {
                    time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    Text = model.nonPredictiveClick(time, buttonClicked, Text);
                }
            }
            /// Predictive mode
            else
            {
                checkBoxChecked = true;
                /// If button clicked is any button except backspace, next and space buttons,
                /// it predicts the words based on button clicks
                if (buttonClicked != 0 && buttonClicked != 10 && buttonClicked != 11)
                {
                    predictWords(buttonClicked);
                }
                /// Displays the next word in predicted words list
                else if (buttonClicked == 0)
                {
                    textBoxWords = Text.Split(' ');
                    string previousText = "";
                    for (int i = 0; i < textBoxWords.Length - 1; i++)
                    {
                        previousText += textBoxWords[i] + " ";
                    }

                    /// Checks if user pressed space and then backspace
                    if (isWord == true)
                    {
                        /// If key length is greater than 1,
                        /// gets the last character of the key
                        if (key.Length > 1)
                        {
                            key = key[key.Length - 1].ToString();
                        }

                        /// Gets the predicted words list based on the key value
                        if (key != "")
                        {
                            predictedWords = model.getElements(key, currentWord, page);
                        }
                        else
                        {
                            if (currentWord.Contains('-'))
                            {
                                predictedWords = model.getWords(Text.Split(' ').Last(), page);
                            }
                            else
                            {
                                predictedWords = model.getWords(currentWord, page);
                            }
                        }
                    }
                    else
                    {
                        predictedWords = model.predictiveClick(key, page);
                    }
                    if (predictedWords.Count > count)
                    {
                        /// Gets the next word in predicted words list
                        string firstWord = predictedWords.ElementAt(count);
                        string word = textBoxWords.Last() + predictedText;
                        if (word == firstWord)
                        {
                            count++;
                            if (predictedWords.Count > count)
                            {
                                firstWord = predictedWords.ElementAt(count);
                            }
                        }

                        /// Displays the word on the text box based on the part that is actual word 
                        /// and the part that is prediccted word
                        if (firstWord.Length > key.Length)
                        {
                            int length;
                            int substringLength;
                            if (key == "")
                            {
                                length = firstWord.Length - currentWord.Length;
                                substringLength = currentWord.Length;
                                Text = previousText + firstWord.Substring(0, substringLength);
                                PredictedText = firstWord.Substring(substringLength, length);
                            }
                            else
                            {
                                if (isWord == true)
                                {
                                    substringLength = currentWord.Length + key.Length;
                                    Text = previousText + firstWord.Substring(0, substringLength);
                                    length = firstWord.Length - substringLength;
                                    PredictedText = firstWord.Substring(substringLength, length);
                                }
                                else
                                {
                                    length = firstWord.Length - key.Length;
                                    substringLength = key.Length;
                                    Text = previousText + firstWord.Substring(0, substringLength);
                                    PredictedText = firstWord.Substring(substringLength, length);
                                }
                            }
                        }
                        /// If first word text length is the same as the button click length,
                        /// then displays the actual word
                        else
                        {
                            Text = previousText + firstWord;
                            PredictedText = "";
                        }

                        /// Increments the count to get the next element of the list
                        count++;
                        /// If the count reaches the 10th position in the predicted word list,
                        /// it goes to the next page and displays the next 10 elements in the list
                        if (count > 9)
                        {
                            page++;
                            count = 0;
                            if (isWord == true)
                            {
                                currentWord = textBoxWords.Last();
                                predictedWords = model.getWords(currentWord, page);
                            }
                            else
                            {
                                predictedWords = model.predictiveClick(key, page);
                            }
                        }
                    }
                }

                /// Deals with the backspace
                else if (buttonClicked == 10)
                {
                    /// Resets all the variables and predicted words list to default values
                    /// if the text box length is less than or equal to 1
                    if (Text.Length <= 1)
                    {
                        key = "";
                        Text = "";
                        PredictedText = "";
                        isWord = false;
                        currentWord = "";
                        count = 1;
                        page = 0;
                        predictedWords = new List<string>();
                        textBoxWords = null;
                    }
                    else if (Text.Length > 1)
                    {
                        /// Deletes the last character of the text box
                        Text = Text.Substring(0, Text.Length - 1);
                        if (key != "")
                        {
                            /// Delets the last character of the key
                            key = key.Substring(0, key.Length - 1);
                            PredictedText = "";
                            count = 0;
                            page = 0;
                            if (currentWord.Length > key.Length)
                            {
                                predictedWords = model.getWords(currentWord, page);
                                isWord = true;
                            }
                            else
                            {
                                predictedWords = model.predictiveClick(key, page);
                                isWord = false;
                            }
                            callPredictiveClick();
                        }
                        else
                        {
                            textBoxWords = Text.Split(' ');
                            currentWord = textBoxWords.Last();
                            PredictedText = "";
                            predictedWords = model.getWords(currentWord, page);
                            if (predictedWords.Count > 0)
                            {
                                if (currentWord == predictedWords.ElementAt(0))
                                {
                                    count = 1;
                                }
                                else
                                {
                                    count = 0;
                                }
                                page = 0;
                                isWord = true;
                                callPredictiveClick();
                            }
                        }
                    }
                }

                /// If the user presses space,it resets to default
                /// and the text box text will contain the text present followed by space
                else if (buttonClicked == 11)
                {
                    if (Text.Length > 0)
                    {
                        key = "";
                        Text = Text + PredictedText + " ";
                        PredictedText = "";
                        isWord = false;
                        currentWord = "";
                        count = 1;
                        page = 0;
                        predictedWords = new List<string>();
                        textBoxWords = null;
                    }
                }
            }
        }
    }
}
