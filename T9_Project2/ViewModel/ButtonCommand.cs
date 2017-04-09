/// Author: Pooja Shah
/// Date of Creation: March 15, 2017
/// This program is used to deal with the button commands using MVVM design pattern
using System;
using System.Windows.Input;
using ViewModel;

namespace T9_Project2.ViewModel
{
    /// <summary>
    /// A button command class to deal with the button clicks
    /// </summary>
    public class ButtonCommand : ICommand
    {
        /// <summary>
        /// Creates the object of the view model
        /// </summary>
        private T9ViewModel viewModel;

        public ButtonCommand(T9ViewModel vm)
        {
            viewModel = vm;
        }

        /// <summary>
        /// Returns true to execute the button click
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Tells the function from view model to be called on button click
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            var buttonClick = parameter.ToString();
            if (buttonClick.Equals("*"))
            {
                viewModel.getText(10);
            }
            else if (buttonClick.Equals("#"))
            {
                viewModel.getText(11);
            }
            else
            {
                viewModel.getText(Int32.Parse(buttonClick));
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
