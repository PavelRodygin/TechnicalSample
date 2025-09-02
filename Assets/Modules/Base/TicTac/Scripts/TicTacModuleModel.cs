using CodeBase.Core.Patterns.Architecture.MVP;
using System.Collections.Generic;

namespace Modules.Base.TicTac.Scripts
{
    public class TicTacModel : IModel
    {
        private const int BoardSize = 3;
        private const char PlayerX = 'X';
        private const char PlayerO = 'O';

        public char[,] Board { get; private set; }
        public char CurrentPlayer { get; private set; }
        public bool IsGameOver { get; private set; }

        private static readonly int[][] WinPositions = {
            new[] {0, 0, 0, 1, 0, 2},
            new[] {1, 0, 1, 1, 1, 2}, 
            new[] {2, 0, 2, 1, 2, 2}, 
            new[] {0, 0, 1, 0, 2, 0}, 
            new[] {0, 1, 1, 1, 2, 1}, 
            new[] {0, 2, 1, 2, 2, 2}, 
            new[] {0, 0, 1, 1, 2, 2},
            new[] {0, 2, 1, 1, 2, 0}  
        };

        public TicTacModel() { }

        public void InitializeGame()
        {
            Board = new char[BoardSize, BoardSize];
            CurrentPlayer = PlayerX;
            IsGameOver = false;
        }

        public void MakeMove(int x, int y)
        {
            if (Board[x, y] == '\0' && !IsGameOver)
            {
                Board[x, y] = CurrentPlayer;
                CurrentPlayer = CurrentPlayer == PlayerX ? PlayerO : PlayerX;
            }
        }

        public char CheckWinner()
        {
            foreach (var pos in WinPositions)
            {
                if (Board[pos[0], pos[1]] == Board[pos[2], pos[3]] &&
                    Board[pos[2], pos[3]] == Board[pos[4], pos[5]] &&
                    Board[pos[0], pos[1]] != '\0')
                {
                    IsGameOver = true;
                    return Board[pos[0], pos[1]];
                }
            }
            return '\0';
        }

        public bool IsBoardFull()
        {
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (Board[i, j] == '\0')
                        return false;
                }
            }
            IsGameOver = true;
            return true;
        }
        
        public int[][] GetWinningPositions()
        {
            var winningPositions = new List<int[]>();
            
            foreach (var pos in WinPositions)
            {
                if (Board[pos[0], pos[1]] == Board[pos[2], pos[3]] &&
                    Board[pos[2], pos[3]] == Board[pos[4], pos[5]] &&
                    Board[pos[0], pos[1]] != '\0')
                {
                    winningPositions.Add(pos);
                }
            }
            
            return winningPositions.ToArray();
        }
        
        public void Dispose() {}
    }
}