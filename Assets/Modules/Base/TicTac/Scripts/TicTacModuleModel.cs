using System;
using CodeBase.Core.Patterns.Architecture.MVP;
using System.Collections.Generic;
using Stateless;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;

namespace Modules.Base.TicTac.Scripts
{
    public enum TicTacGameStates { Tutorial, Game, Result }
    public enum TicTacGameTriggers { InitializeTutorial, StartGame, PlayerWon, GameDraw, Restart, Exit }

    public readonly struct TicTacCommands
    {
        public readonly ReactiveCommand<Unit> StartGameCommand;
        public readonly ReactiveCommand<Unit> RestartCommand;
        public readonly ReactiveCommand<Unit> ExitCommand;
        public readonly ReactiveCommand<int[]> CellClickCommand;

        public TicTacCommands(
            ReactiveCommand<Unit> startGameCommand,
            ReactiveCommand<Unit> restartCommand,
            ReactiveCommand<Unit> exitCommand,
            ReactiveCommand<int[]> cellClickCommand)
        {
            StartGameCommand = startGameCommand;
            RestartCommand = restartCommand;
            ExitCommand = exitCommand;
            CellClickCommand = cellClickCommand;
        }
    }

    public class TicTacModel : IModel
    {
        private const int BoardSize = 3;
        private const char PlayerX = 'X';
        private const char PlayerO = 'O';

        // Throttle delays for anti-spam protection
        public int CommandThrottleDelay { get; } = 300;
        public int ModuleTransitionThrottleDelay { get; } = 500;

        private readonly StateMachine<TicTacGameStates, TicTacGameTriggers> _stateMachine;

        public StateMachine<TicTacGameStates, TicTacGameTriggers> StateMachine => _stateMachine;

        public char[,] Board { get; private set; }
        public char CurrentPlayer { get; private set; }
        public bool IsGameOver { get; private set; }
        public char Winner { get; private set; }

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

        public TicTacModel() 
        {
            _stateMachine = new StateMachine<TicTacGameStates, TicTacGameTriggers>(TicTacGameStates.Tutorial);
        }

        /// <summary>
        /// Configure FSM with state transition callbacks
        /// </summary>
        public void ConfigureStateMachine(
            Func<UniTask> onEnterTutorial, Func<UniTask> onExitTutorial,
            Func<UniTask> onEnterGame, Func<UniTask> onExitGame,
            Func<UniTask> onEnterResult, Func<UniTask> onExitResult)
        {
            _stateMachine.Configure(TicTacGameStates.Tutorial)
                .OnEntryAsync(async () => await onEnterTutorial())
                .OnExitAsync(async () => await onExitTutorial())
                .PermitReentry(TicTacGameTriggers.InitializeTutorial) // Allow re-entering Tutorial for initialization
                .Permit(TicTacGameTriggers.StartGame, TicTacGameStates.Game)
                .Ignore(TicTacGameTriggers.Exit) // Ignore exit in tutorial, handle at controller level
                .Ignore(TicTacGameTriggers.PlayerWon) // Ignore game triggers in tutorial
                .Ignore(TicTacGameTriggers.GameDraw) // Ignore game triggers in tutorial
                .Ignore(TicTacGameTriggers.Restart); // Ignore restart in tutorial

            _stateMachine.Configure(TicTacGameStates.Game)
                .OnEntryAsync(async () => await onEnterGame())
                .OnExitAsync(async () => await onExitGame())
                .Permit(TicTacGameTriggers.PlayerWon, TicTacGameStates.Result)
                .Permit(TicTacGameTriggers.GameDraw, TicTacGameStates.Result)
                .Ignore(TicTacGameTriggers.Exit) // Ignore exit in game, handle at controller level
                .Ignore(TicTacGameTriggers.InitializeTutorial); // Ignore tutorial trigger in game

            _stateMachine.Configure(TicTacGameStates.Result)
                .OnEntryAsync(async () => await onEnterResult())
                .OnExitAsync(async () => await onExitResult())
                .Permit(TicTacGameTriggers.Restart, TicTacGameStates.Game)
                .Permit(TicTacGameTriggers.Exit, TicTacGameStates.Tutorial)
                .Ignore(TicTacGameTriggers.PlayerWon) // Ignore if accidentally triggered in Result state
                .Ignore(TicTacGameTriggers.GameDraw) // Ignore if accidentally triggered in Result state
                .Ignore(TicTacGameTriggers.InitializeTutorial); // Ignore tutorial trigger in result
        }

        /// <summary>
        /// Change game state with validation
        /// </summary>
        public async UniTask ChangeState(TicTacGameTriggers trigger)
        {
            if (!_stateMachine.CanFire(trigger))
            {
                Debug.LogWarning($"Cannot fire trigger {trigger} from state {_stateMachine.State}");
                return;
            }

            await _stateMachine.FireAsync(trigger);
        }

        public void InitializeGame()
        {
            Board = new char[BoardSize, BoardSize];
            CurrentPlayer = PlayerX;
            IsGameOver = false;
            Winner = '\0';
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
                    Winner = Board[pos[0], pos[1]];
                    return Winner;
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