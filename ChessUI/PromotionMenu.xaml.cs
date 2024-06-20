using ChessLogic;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChessUI
{
    public partial class PromotionMenu : UserControl
    {
        public event Action<PieceType> PieceSelected;
        public PromotionMenu(Player player, bool isRussian)
        {
            InitializeComponent();
            QueenImg.Source = Images.GetImage(player, PieceType.Queen);
            BishopImg.Source = Images.GetImage(player, PieceType.Bishop);
            RookImg.Source = Images.GetImage(player, PieceType.Rook);
            KnightImg.Source = Images.GetImage(player, PieceType.Knight);
            PromotionTextBlock.Text = isRussian ? "Выберите фигуру:" : "Select a piece:";
        }

        private void QueenImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Queen);
        }

        private void BishopImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Bishop);

        }

        private void RookImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Rook);

        }

        private void KnightImg_MouseDown(object sender, MouseButtonEventArgs e)
        {
            PieceSelected?.Invoke(PieceType.Knight);

        }
    }
}
