using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using ChessLogic;

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private readonly Image[,] pieceImages = new Image[8,8];
        private readonly Rectangle[,] highlights = new Rectangle[8, 8];
        private readonly Dictionary<Position, Move> moveCache = new Dictionary<Position, Move>(); //key possible square, ToPos == key and Move is object with FromPos = selectedPos 

        private bool AutoRotate = false;
        public bool isRussian = false;

        private DispatcherTimer dtWhite = new DispatcherTimer();
        private DispatcherTimer dtBlack = new DispatcherTimer();


        public SoundPlayer[] soundPlayers = { new SoundPlayer(), new SoundPlayer(), new SoundPlayer(), new SoundPlayer(), new SoundPlayer()};
       
        private GameState gameState;
        private Position selectedPos = null; //the figure that was chosen
        public MainWindow()
        {
            LoadAudios(new string[] { "Assets\\MoveSelf.wav", "Assets\\Capture.wav", "Assets\\Promote.wav", "Assets\\Check.wav", "Assets\\Castle.wav"});
            InitializeDispatcherTimes();
            InitializeComponent();
            InitializeBoard(); //create image tags in xaml
            gameState = new GameState(Player.White, Board.Initial()); //create logical pieces, not ui
            DrawBoard(gameState.Board); //connect image tags with logical pieces 
            SetCursor(gameState.CurrentPlayer);
            InitializeTimeContent();

        }

        private void InitializeTimeContent()
        {
            var comboBoxItem = TimeComboBox.SelectedItem;
            string unparsedTime = ((ComboBoxItem)comboBoxItem).Content.ToString();
            gameState.whiteLeftTime = ParseStringToSeconds(unparsedTime);
            gameState.blackLeftTime = ParseStringToSeconds(unparsedTime);
            RenderTimeContent(gameState.whiteLeftTime, gameState.blackLeftTime);
        }
        private void InitializeDispatcherTimes()
        {
            dtWhite.Interval = TimeSpan.FromSeconds(1);
            dtWhite.Tick += DtWhiteTicker;
            dtBlack.Interval = TimeSpan.FromSeconds(1);
            dtBlack.Tick += DtBlackTicker;
           
        }

        private void LoadAudios(string[] relativePaths)
        {
            for (int i = 0; i < relativePaths.Length; i++)
            {
                soundPlayers[i].SoundLocation = GetAbsolutePath(relativePaths[i]);
                soundPlayers[i].Load();
            }
        }
        private string GetAbsolutePath(string relativePath)
        {
            string startDir = AppDomain.CurrentDomain.BaseDirectory; 
            string finalDir = startDir;
#if DEBUG
            for (int i = 0; i < 2; i++)
            {
                FileInfo fileInfo = new FileInfo(finalDir);
                DirectoryInfo parentDir = fileInfo.Directory.Parent;
                finalDir = parentDir.FullName;
            }
# endif

            return System.IO.Path.Combine(finalDir, relativePath);
        }
        private void InitializeBoard() 
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Image image = new Image();
                    pieceImages[r, c] = image;
                    PieceGrid.Children.Add(image);
                    
                    Rectangle highlight = new Rectangle();
                    highlights[r,c] = highlight;
                    HighlightGrid.Children.Add(highlight);
                }
            }
           
        }
        private void DrawBoard(Board board) 
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board[r, c];
                    pieceImages[r, c].Source = Images.GetImage(piece);
                }
            }
        }

        private void BoardGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMenuOnScreen()) return;
            Point point = e.GetPosition(BoardGrid);
            Position pos = ToSquarePosition(point);
            if (selectedPos == null) OnFromPositionSelected(pos);
            else OnToPositionSelected(pos);
        }

        private Position ToSquarePosition(Point point)
        {
            double squareSize = BoardGrid.ActualWidth / 8;
            int row = (int)(point.Y / squareSize);
            int column = (int)(point.X / squareSize);
            return new Position(row, column); 
        }

        private void OnFromPositionSelected(Position pos)
        {
            IEnumerable<Move> moves = gameState.LegalMovesForPiece(pos);
            if (moves.Any())
            {
                selectedPos = pos;
                CacheMoves(moves);
                ShowHighlights();
            }
        }

        private void OnToPositionSelected(Position pos)
        {
            HideHighlights();
            selectedPos = null;
            if (moveCache.TryGetValue(pos, out Move move)) //if click was on green square than we do a move and rerender the board
            {
                if (move.Type == MoveType.PawnPromotion) HandlePromotion(move.FromPos, move.ToPos);
                else HandleMove(move);
            }
        }
        private void HandlePromotion(Position from, Position to)
        {
            pieceImages[to.Row, to.Column].Source = Images.GetImage(gameState.CurrentPlayer, PieceType.Pawn);
            pieceImages[from.Row, from.Column].Source = null;
            PromotionMenu promMenu = new PromotionMenu(gameState.CurrentPlayer, isRussian);
            MenuContainer.Content = promMenu;
            promMenu.PieceSelected += type =>
            {
                MenuContainer.Content = null;
                Move promMove = new PawnPromotion(from, to, type);
                HandleMove(promMove);
            };

        }
        private void HandleMove(Move move)
        {
           
            bool didCaptureFigure = gameState.MakeMove(move);
            PlaySound(move, didCaptureFigure);
            DrawBoard(gameState.Board);
            SetCursor(gameState.CurrentPlayer);

            TimeCheckbox.IsEnabled = false;
            TimeComboBox.IsEnabled = false;

            if (gameState.withTime)
            {
                if (gameState.whiteMoved && gameState.blackMoved)
                {
                    if (gameState.CurrentPlayer == Player.Black)
                    {
                        dtWhite.Stop();
                        dtBlack.Start();
                    }
                    else if (gameState.CurrentPlayer == Player.White)
                    {
                        dtBlack.Stop();
                        dtWhite.Start();
                    }
                }
            }
            if (gameState.IsGameOver()) ShowGameOver();
            if (AutoRotate) HandleRotateBoard();
        }
        private void PlaySound(Move move, bool didCaptureFigure)
        {
            if (gameState.Board.IsInCheck(gameState.CurrentPlayer)) soundPlayers[3].Play();
            else if (move.Type == MoveType.PawnPromotion) soundPlayers[2].Play();
            else if (move.Type == MoveType.CastleKS || move.Type == MoveType.CastleQS) soundPlayers[4].Play();
            else if (didCaptureFigure || move.Type == MoveType.EnPassant) soundPlayers[1].Play(); 
            else soundPlayers[0].Play();
        }
        private void CacheMoves(IEnumerable<Move> moves)
        {
            moveCache.Clear();
            foreach (Move move in moves)
            {
                moveCache[move.ToPos] = move;
            }
        }

        private void ShowHighlights()
        {

            Color MovesColor = Color.FromRgb(125, 255, 125);
            Color SelectedFigureColor = Color.FromRgb(125, 255, 125);
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = new SolidColorBrush(MovesColor);
                highlights[to.Row, to.Column].Opacity = 0.5;
            }
            //do it after loop because code in the loop overwrites posTo, and after that it becomes posFrom, so i do it right here
            highlights[selectedPos.Row, selectedPos.Column].Fill = new SolidColorBrush(SelectedFigureColor);
            highlights[selectedPos.Row, selectedPos.Column].Opacity = 1;

        }
        private void HideHighlights()
        {
            if (selectedPos != null) highlights[selectedPos.Row, selectedPos.Column].Fill = Brushes.Transparent;
            foreach (Position to in moveCache.Keys)
            {
                highlights[to.Row, to.Column].Fill = Brushes.Transparent;
            }
        }

        private void SetCursor(Player player)
        {
            if (player == Player.White) Cursor = ChessCursors.WhiteCursor;
            else Cursor = ChessCursors.BlackCursor;
        }

        private bool IsMenuOnScreen()
        {
            return MenuContainer.Content != null;
        }

        private void ShowGameOver()
        {
            GameOverMenu gameOverMenu = new GameOverMenu(gameState, isRussian);
            MenuContainer.Content = gameOverMenu;

            dtBlack.Stop();
            dtWhite.Stop();

            gameOverMenu.OptionSelected += option =>
            {
                if (option == Option.Restart)
                {
                    MenuContainer.Content = null;
                    RestartGame();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            };
        }
        private void RestartGame()
        {
            HideHighlights();
            selectedPos = null;
            moveCache.Clear();
            dtWhite.Stop();
            dtBlack.Stop();
            TimeCheckbox.IsEnabled = true;
            TimeComboBox.IsEnabled = true;
            bool withTime = gameState.withTime;
            gameState = new GameState(Player.White, Board.Initial());
            gameState.withTime = withTime;
            DrawBoard(gameState.Board);
            InitializeTimeContent();
            SetCursor(gameState.CurrentPlayer);
        }
        private void Rotate_Board(object sender, RoutedEventArgs e)
        {
            HandleRotateBoard();
        }

        private void HandleRotateBoard()
        {
            if (IsMenuOnScreen()) return;
            int whiteLeftTime = gameState.whiteLeftTime;
            int blackLeftTime = gameState.blackLeftTime;
            bool withTime = gameState.withTime;
            bool blackMoved = gameState.blackMoved;
            bool whiteMoved = gameState.whiteMoved;
            int noCaptureOrPawnMoves = gameState.noCaptureOrPawnMoves;
            Dictionary<string, int> stateHistory = gameState.ConvertStateHistory();

            gameState = new GameState(gameState.CurrentPlayer, gameState.Board.Rotate());
            gameState.withTime = withTime;
            gameState.blackMoved = blackMoved;
            gameState.whiteMoved = whiteMoved;
            gameState.whiteLeftTime = whiteLeftTime;
            gameState.blackLeftTime = blackLeftTime;
            gameState.noCaptureOrPawnMoves = noCaptureOrPawnMoves;
            gameState.stateHistory = stateHistory;

            DrawBoard(gameState.Board);
            HideHighlights();
            moveCache.Clear();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!IsMenuOnScreen() && e.Key == Key.Escape)
            {
                ShowPauseMenu();
            }
        }


        private void ShowPauseMenu()
        {
            PauseMenu pauseMenu = new PauseMenu(isRussian);
            MenuContainer.Content = pauseMenu;
            DispatcherTimer lastTimer = null;
            if (gameState.withTime)
            {
                if (dtBlack.IsEnabled)
                {
                    lastTimer = dtBlack;
                    dtBlack.Stop();
                }
                else
                {
                    lastTimer = dtWhite;
                    dtWhite.Stop();
                }
            }
            pauseMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;
                if (option == Option.Continue)
                {
                    if (lastTimer != null && gameState.whiteMoved && gameState.blackMoved) lastTimer.Start();
                }
                if (option == Option.Restart)
                {
                    RestartGame();
                }
            };
        }

        private void RotateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            AutoRotate = true;
        }
        private void RotateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            AutoRotate = false;
        }

        private void DtWhiteTicker(object sender, EventArgs e)
        {
            gameState.whiteLeftTime--;
            WhiteTimeLabel.Content = ParseSecondsToString(gameState.whiteLeftTime);
            if (gameState.whiteLeftTime == 0)
            {
                dtWhite.Stop();
                gameState.CheckForGameOver();
                ShowGameOver();
            }
        }

        private void DtBlackTicker(object sender, EventArgs e)
        {
            gameState.blackLeftTime--;
            BlackTimeLabel.Content = ParseSecondsToString(gameState.blackLeftTime);
            if (gameState.blackLeftTime == 0)
            {
                dtBlack.Stop();
                gameState.CheckForGameOver();
                ShowGameOver();
            }

        }

        public int ParseStringToSeconds(string s)
        {
            if (TimeSpan.TryParseExact(s, "m':'ss", null, out TimeSpan time))
            {
                return (int)time.TotalSeconds;
            }
            else
            {
                throw new FormatException("Invalid time format");
            }
        }
        public string ParseSecondsToString(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            string parsedSeconds = time.Seconds.ToString();
            if (time.Seconds < 10) parsedSeconds = "0" + time.Seconds.ToString();
            return time.Minutes.ToString() + ":" + parsedSeconds;
        }

        private void TimeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            gameState.withTime = true;
        }

        private void TimeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            gameState.withTime = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gameState == null) return;
            ComboBox comboBox = (ComboBox)sender;
            string selectedValue = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
            gameState.whiteLeftTime = ParseStringToSeconds(selectedValue);
            gameState.blackLeftTime = ParseStringToSeconds(selectedValue);
            RenderTimeContent(gameState.whiteLeftTime, gameState.blackLeftTime);
        }

        private void RenderTimeContent(int whiteLeftTime, int blackLeftTime)
        {
            WhiteTimeLabel.Content = ParseSecondsToString(whiteLeftTime);
            BlackTimeLabel.Content = ParseSecondsToString(blackLeftTime);
        }

        private void RulesButton_Click(object sender, RoutedEventArgs e)
        {
            if (MenuContainer.Content != null) return;
            RulesMenu rulesMenu = new RulesMenu(isRussian);
            MenuContainer.Content = rulesMenu;
            DispatcherTimer lastTimer = null;
            if (gameState.withTime)
            {
                if (dtBlack.IsEnabled)
                {
                    lastTimer = dtBlack;
                    dtBlack.Stop();
                }
                else
                {
                    lastTimer = dtWhite;
                    dtWhite.Stop();
                }
            }
            rulesMenu.OptionSelected += option =>
            {
                MenuContainer.Content = null;
                if (option == Option.Continue)
                {
                    if (lastTimer != null && gameState.whiteMoved && gameState.blackMoved) lastTimer.Start();
                }
            };
        }

        private void RuButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMenuOnScreen()) return;
            isRussian = true;
            RulesBtn.Content = "Правила";
            TimeCheckbox.Content = "На время?";
            TimeLabel.Content = "Выберите время:";
            BlackSideLabel.Content = "Черные:";
            WhiteSideLabel.Content = "Белые:";
            AutoRotateCheckbox.Content = "Авто";
        }

        private void EnButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMenuOnScreen()) return;
            isRussian = false;
            RulesBtn.Content = "Rules";
            TimeCheckbox.Content = "With time?";
            TimeLabel.Content = "Choose time value:";
            BlackSideLabel.Content = "Black:";
            WhiteSideLabel.Content = "White:";
            AutoRotateCheckbox.Content = "Auto";
        }
    }
}
