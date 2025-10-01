using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Patterns.Architecture.MVP;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.TicTac.Scripts.GameState
{
    /// <summary>
    /// Presenter for the game state that handles game logic and UI interactions
    /// </summary>
    public class TicTacGameStatePresenter : IPresenter
    {
        private readonly TicTacGameStateView _gameStateView;
        private readonly TicTacModel _gameModel;
        private readonly CompositeDisposable _disposables = new();

        private readonly ReactiveCommand<int[]> _cellCommand = new();
        private readonly ReactiveCommand<Unit> _restartCommand = new();
        private readonly ReactiveCommand<Unit> _mainMenuCommand = new();

        private ReactiveCommand<ModulesMap> _openNewModuleCommand;

        public ReactiveCommand<int[]> CellCommand => _cellCommand;
        public ReactiveCommand<Unit> RestartCommand => _restartCommand;
        public ReactiveCommand<Unit> MainMenuCommand => _mainMenuCommand;

        public TicTacGameStatePresenter(TicTacGameStateView gameStateView, TicTacModel gameModel)
        {
            _gameStateView = gameStateView ?? throw new ArgumentNullException(nameof(gameStateView));
            _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
        }

        public async UniTask Enter(object param)
        {
            _openNewModuleCommand = param as ReactiveCommand<ModulesMap> ?? throw new ArgumentException("Expected ReactiveCommand<ModulesMap>", nameof(param));
            
            SubscribeToCommands();
            
            _gameModel.InitializeGame();
            var commands = new TicTacCommands(_restartCommand, _restartCommand, _mainMenuCommand, _cellCommand);
            _gameStateView.SetupEventListeners(commands);
            await _gameStateView.Show();
            _gameStateView.ClearBoard();
            _gameStateView.UnblockBoard();
        }

        public async UniTask Exit()
        {
            if (_gameStateView.isActiveAndEnabled) 
                await _gameStateView.Hide();
        }

        public void HideStateInstantly() => _gameStateView.HideInstantly();

        public void UpdateBoardDisplay(char[,] board)
        {
            _gameStateView.UpdateBoard(board);
        }

        public void MarkWinningCells(int[][] winningPositions)
        {
            _gameStateView.MarkWinningCells(winningPositions);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToCommands()
        {
            _cellCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(position => OnCellClicked(position[0], position[1]))
                .AddTo(_disposables);
            
            _restartCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(_ => OnRestartButtonClicked())
                .AddTo(_disposables);
            
            _mainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);
        }

        private async void OnCellClicked(int x, int y)
        {
            if (_gameModel.StateMachine.State != TicTacGameStates.Game) return;
            
            _gameModel.MakeMove(x, y);
            UpdateBoardDisplay(_gameModel.Board);
                
            char winner = _gameModel.CheckWinner();
            
            if (winner != '\0')
            {
                var winningPositions = _gameModel.GetWinningPositions();
                MarkWinningCells(winningPositions);
                await _gameModel.ChangeState(TicTacGameTriggers.PlayerWon);
            }
            else if (_gameModel.IsBoardFull()) 
            {
                await _gameModel.ChangeState(TicTacGameTriggers.GameDraw);
            }
        }

        private async void OnRestartButtonClicked() => 
            await _gameModel.ChangeState(TicTacGameTriggers.Restart);

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand?.Execute(ModulesMap.MainMenu);
        }
    }
}
