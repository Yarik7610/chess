using ChessLogic;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;


namespace ChessUI
{
    public partial class GameOverMenu : UserControl
    {
        public event Action<Option> OptionSelected;
        public Result result;
        public GameOverMenu(GameState gameState, bool isRussian)
        {
            InitializeComponent();
            Result result = gameState.Result;
            WinnerText.Text = GetWinnerText(result.Winner, isRussian);
            ReasonText.Text = GetReasonText(result.Reason, gameState.CurrentPlayer, result.Winner, isRussian);
            GameOverMenuExitBtn.Content = isRussian ? "Выйти" : "Exit";
            GameOverMenuRestartBtn.Content = isRussian ? "Перезапустить" : "Restart";
        }

        private static string GetWinnerText(Player winner, bool isRussian)
        {
            if (isRussian)
            {
                return winner switch
                {
                    Player.White => "Победа белых!",
                    Player.Black => "Победа черных!",
                    _ => "Ничья!"
                };
            }
            return winner switch
            {
                Player.White => "White wins!",
                Player.Black => "Black wins!",
                _ => "It's a draw!"
            };
        }

        private static string PlayerString(Player player, bool isRussian)
        {
            if (isRussian)
            {
                return player switch
                {
                    Player.White => "Белые",
                    Player.Black => "Черные",
                    _ => ""
                };
            }
            return player switch
            {
                Player.White => "White",
                Player.Black => "Black",
                _ => ""
            };
        }
        private static string GetOpponentInRussian(Player opponent)
        {
            return opponent switch
            {
                Player.Black => "черных",
                Player.White => "белых",
                _ => ""
            };
        }
        private static string GetReasonText(EndReason reason, Player currentPlayer, Player winner, bool isRussian)
        {
            if (isRussian)
            {
                return reason switch
                {
                    EndReason.Stalemate => $"Пат - {PlayerString(currentPlayer, isRussian)} не могут двигаться",
                    EndReason.Checkmate => $"Мат - {PlayerString(currentPlayer, isRussian)} нe могут двигаться",
                    EndReason.FiftyMoveRule => $"Правило 50 ходов",
                    EndReason.InsufficientMaterial => $"Недостаточность материала",
                    EndReason.ThreefoldRepetition => $"Троекратное повторение",
                    EndReason.Time => $"У {GetOpponentInRussian(winner.Opponent())} нету времени",
                    _ => ""
                };
            }
            return reason switch
            {
                EndReason.Stalemate => $"Stalemate - {PlayerString(currentPlayer, isRussian)} can't move",
                EndReason.Checkmate => $"Checkmate - {PlayerString(currentPlayer, isRussian)} can't move",
                EndReason.FiftyMoveRule => $"Fifty-move rule",
                EndReason.InsufficientMaterial => $"Insufficient material",
                EndReason.ThreefoldRepetition => $"Threefold repetition",
                EndReason.Time => $"{winner.Opponent()} have no time",
                _ => ""
            };
        }
        private void Restart_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Restart);
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            OptionSelected?.Invoke(Option.Exit);
        }
    }
}
