using Hydr10n.Collections;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Sliding_Puzzle
{
    class PuzzleGenerator
    {
        public SquareArray<int> SolvedPuzzle { get; private set; }
        public SquareArray<int> Puzzle { get; private set; }
        public Cell EmptySpace { get; private set; }

        public PuzzleGenerator(int countPerSide, int threshold)
        {
            SolvedPuzzle = GenerateSolvedPuzzle(countPerSide);
            Puzzle = SolvedPuzzle.Clone() as SquareArray<int>;
            EmptySpace = new Cell(SolvedPuzzle.SideLength - 1, SolvedPuzzle.SideLength - 1);
            Random random = new Random();
            for (int count = 0; count < threshold; count++)
                Move((Direction)random.Next(0, 4));
        }

        public PuzzleGenerator(SquareArray<int> puzzle)
        {
            SolvedPuzzle = GenerateSolvedPuzzle(puzzle.SideLength);
            int[] array = new int[puzzle.SideLength * puzzle.SideLength], array2 = new int[array.Length];
            Buffer.BlockCopy(SolvedPuzzle.ToArray(), 0, array, 0, sizeof(int) * array.Length);
            Buffer.BlockCopy(puzzle.ToArray(), 0, array2, 0, sizeof(int) * array2.Length);
            Array.Sort(array2);
            if (!array.OrderBy((a) => a).ToArray().SequenceEqual(array2))
                throw new InvalidDataException();
            Puzzle = puzzle.Clone() as SquareArray<int>;
            for (int i = 0; i < puzzle.SideLength; i++)
                for (int j = 0; j < puzzle.SideLength; j++)
                    if (puzzle[i, j] == 0)
                    {
                        EmptySpace = new Cell(i, j);
                        break;
                    }
        }

        private static SquareArray<int> GenerateSolvedPuzzle(int countPerSide)
        {
            SquareArray<int> squareArray = new SquareArray<int>(countPerSide);
            for (int count = 0; count < squareArray.SideLength * squareArray.SideLength - 1; count++)
                squareArray[count / squareArray.SideLength, count % squareArray.SideLength] = count + 1;
            return squareArray;
        }

        public Cell Move(Direction direction)
        {
            int rowOffset = 0, columnOffset = 0;
            switch (direction)
            {
                case Direction.Left: columnOffset = 1; break;
                case Direction.Up: rowOffset = 1; break;
                case Direction.Right: columnOffset = -1; break;
                case Direction.Down: rowOffset = -1; break;
                default: throw new InvalidEnumArgumentException();
            }
            int row = EmptySpace.Row + rowOffset, column = EmptySpace.Column + columnOffset;
            if (row >= 0 && row < Puzzle.SideLength && column >= 0 && column < Puzzle.SideLength)
            {
                Puzzle[EmptySpace.Row, EmptySpace.Column] = Puzzle[row, column];
                Puzzle[row, column] = 0;
                Cell cell = EmptySpace;
                EmptySpace = new Cell(row, column);
                return cell;
            }
            else
                return EmptySpace;
        }

        public bool IsPuzzleSovled
        {
            get
            {
                for (int i = 0; i < Puzzle.SideLength; i++)
                    for (int j = 0; j < Puzzle.SideLength; j++)
                        if (Puzzle[i, j] != SolvedPuzzle[i, j])
                            return false;
                return true;
            }
        }
    }
}