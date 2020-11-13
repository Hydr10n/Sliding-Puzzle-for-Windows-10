using Hydr10n.Collections;
using Hydr10n.DataUtils;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Sliding_Puzzle
{
    enum Direction { Left, Up, Right, Down }

    sealed class GameManager
    {
        private const int MinTilesCountPerSide = 4;
        private const double GameLayoutPaddingScale = 0.06;

        private readonly Grid GameLayout;
        private readonly ViewModel ViewModel;
        private readonly DispatcherTimer DispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

        private bool isLayoutReady, hasMoved, isMovementFinished = true;
        private int tilesCountPerSide;
        private PuzzleGenerator puzzleGenerator;
        private SquareArray<Tile> tiles;

        private string GameSaveKeyTime => nameof(GameSaveKeyTime) + tilesCountPerSide;
        private string GameSaveKeyBestTime => nameof(GameSaveKeyBestTime) + tilesCountPerSide;
        private string GameSaveKeyTiles => nameof(GameSaveKeyTiles) + tilesCountPerSide;

        public GameManager(Grid gameLayout, ViewModel viewModel)
        {
            GameLayout = gameLayout;
            ViewModel = viewModel;
            DispatcherTimer.Tick += (sender, e) => AppData.Save(GameSaveKeyTime, ViewModel.Time += DispatcherTimer.Interval);
        }

        private void SaveGameProgress(bool reset)
        {
            TimeSpan time = TimeSpan.Zero;
            SquareArray<int> squareArray = null;
            if (!reset)
            {
                time = ViewModel.Time;
                squareArray = puzzleGenerator.Puzzle;
            }
            AppData.Save(GameSaveKeyTime, time);
            AppData.Save(GameSaveKeyBestTime, ViewModel.BestTime);
            AppData2D.Save(GameSaveKeyTiles, squareArray?.ToArray());
        }

        private bool LoadGameProgress()
        {
            AppData.Load(GameSaveKeyTime, out TimeSpan time, out _);
            ViewModel.Time = time;
            AppData.Load(GameSaveKeyBestTime, out TimeSpan bestTime, out _);
            ViewModel.BestTime = bestTime;
            AppData2D.Load(GameSaveKeyTiles, out int[][] numbers, out _);
            if (numbers != null)
            {
                for (int i = 0; i < tiles.SideLength; i++)
                    for (int j = 0; j < tiles.SideLength; j++)
                        if (numbers[i][j] != 0)
                            tiles[i, j] = AddTile(i, j, numbers[i][j]);
                puzzleGenerator = new PuzzleGenerator(SquareArray<int>.FromArray(numbers));
            }
            return numbers != null;
        }

        private Tile AddTile(int row, int column, int tileNumber) => new Tile(GameLayout, row, column, tileNumber);

        private void GeneratePuzzle()
        {
            puzzleGenerator = new PuzzleGenerator(tiles.SideLength, 1000);
            var puzzle = puzzleGenerator.Puzzle;
            for (int i = 0; i < tiles.SideLength; i++)
                for (int j = 0; j < tiles.SideLength; j++)
                    if (puzzle[i, j] != 0)
                        tiles[i, j] = AddTile(i, j, puzzle[i, j]);
        }

        private void MoveTile(Cell fromCell, Cell toCell)
        {
            Tile tile = tiles[fromCell.Row, fromCell.Column];
            tile.MoveTo(toCell.Row, toCell.Column);
            tiles[fromCell.Row, fromCell.Column] = null;
            tiles[toCell.Row, toCell.Column] = tile;
        }

        private void RemoveAllTiles()
        {
            for (int i = 0; i < tiles.SideLength; i++)
                for (int j = 0; j < tiles.SideLength; j++)
                    if (tiles[i, j] != null)
                    {
                        tiles[i, j].RemoveSelf();
                        tiles[i, j] = null;
                    }
        }

        public void MoveTiles(Direction direction)
        {
            if (!isMovementFinished || ViewModel.GameState != GameState.Started)
                return;
            Tile.Storyboard = new Storyboard();
            Cell cell = puzzleGenerator.Move(direction);
            if (cell == puzzleGenerator.EmptySpace)
                return;
            MoveTile(puzzleGenerator.EmptySpace, cell);
            if (!hasMoved)
            {
                hasMoved = true;
                DispatcherTimer.Start();
            }
            Tile.Storyboard.Completed += delegate
            {
                Tile.Storyboard.Stop();
                if (puzzleGenerator.IsPuzzleSovled)
                {
                    DispatcherTimer.Stop();
                    if (ViewModel.BestTime == TimeSpan.Zero || ViewModel.Time < ViewModel.BestTime)
                        ViewModel.BestTime = ViewModel.Time;
                    ViewModel.GameState = GameState.Won;
                }
                isMovementFinished = true;
                SaveGameProgress(ViewModel.GameState != GameState.Started);
            };
            Tile.Storyboard.Begin();
            isMovementFinished = false;
        }

        public void SetGameLayout(int gameLayoutSelectedIndex)
        {
            tilesCountPerSide = gameLayoutSelectedIndex + MinTilesCountPerSide;
            ViewModel.GameState = GameState.NotStarted;
            GameLayout.Children.Clear();
            double tileFullSideLength = GameLayout.ActualHeight / (tilesCountPerSide + GameLayoutPaddingScale * 2);
            GameLayout.Padding = new Thickness(tileFullSideLength * GameLayoutPaddingScale);
            RowDefinitionCollection rowDefinitions = GameLayout.RowDefinitions;
            rowDefinitions.Clear();
            ColumnDefinitionCollection columnDefinitions = GameLayout.ColumnDefinitions;
            columnDefinitions.Clear();
            for (int i = 0; i < tilesCountPerSide; i++)
            {
                rowDefinitions.Add(new RowDefinition { Height = new GridLength(tileFullSideLength) });
                columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileFullSideLength) });
                for (int j = 0; j < tilesCountPerSide; j++)
                    AddTile(i, j, 0);
            }
            tiles = new SquareArray<Tile>(tilesCountPerSide);
            if (LoadGameProgress())
            {
                hasMoved = true;
                DispatcherTimer.Start();
                ViewModel.GameState = GameState.Started;
            }
            else
            {
                hasMoved = false;
                DispatcherTimer.Stop();
                ViewModel.Time = TimeSpan.Zero;
            }
            isLayoutReady = true;
        }

        public void StartNewGame()
        {
            if (!isLayoutReady)
                return;
            hasMoved = false;
            DispatcherTimer.Stop();
            SaveGameProgress(true);
            RemoveAllTiles();
            GeneratePuzzle();
            ViewModel.Time = TimeSpan.Zero;
            ViewModel.GameState = GameState.Started;
        }
    }
}