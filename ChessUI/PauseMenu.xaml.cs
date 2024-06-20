using System;
using System.Windows;
using System.Windows.Controls;

namespace ChessUI
{
    public partial class PauseMenu : UserControl
    {

        public event Action<Option> OptionSelected;

        public PauseMenu(bool isRussian)
        {
            InitializeComponent();
            PauseTextBlock.Text = isRussian ? "Перезапустить игру?" : "Restart game?";
            PauseContinueBtn.Content = isRussian ? "Продолжить" : "Continue";
            PauseRestartBtn.Content = isRussian ? "Перезапустить" : "Restart";
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Continue);
            
        }
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }
    }
}
