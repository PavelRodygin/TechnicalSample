using System;
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

        public ReactiveCommand<int[]> CellCommand => _cellCommand;
        public ReactiveCommand<Unit> RestartCommand => _restartCommand;
        public ReactiveCommand<Unit> MainMenuCommand => _mainMenuCommand;

        public TicTacGameStatePresenter(TicTacGameStateView gameStateView, TicTacModel gameModel)
        {
            _gameStateView = gameStateView ?? throw new ArgumentNullException(nameof(gameStateView));
            _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
            
            SubscribeToCommands();
        }

        public async UniTask Enter(object param)
        {
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

        private void OnCellClicked(int x, int y)
        {
            // Cell click logic is now handled by the main controller
            // This method is kept for compatibility but doesn't contain game logic
        }

        private void OnRestartButtonClicked()
        {
            // Restart logic is now handled by the main controller
        }

        private void OnMainMenuButtonClicked()
        {
            // Main menu logic is now handled by the main controller
        }
    }
}
