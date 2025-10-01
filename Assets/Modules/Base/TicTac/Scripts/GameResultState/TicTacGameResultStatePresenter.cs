using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Patterns.Architecture.MVP;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.TicTac.Scripts.GameResultState
{
    /// <summary>
    /// Presenter for the result state that shows game outcome and action buttons
    /// </summary>
    public class TicTacGameResultStatePresenter : IPresenter
    {
        private readonly TicTacGameResultStateView _resultView;
        private readonly TicTacModel _gameModel;
        private readonly CompositeDisposable _disposables = new();

        private readonly ReactiveCommand<Unit> _restartCommand = new();
        private readonly ReactiveCommand<Unit> _mainMenuCommand = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;

        public ReactiveCommand<Unit> RestartCommand => _restartCommand;
        public ReactiveCommand<Unit> MainMenuCommand => _mainMenuCommand;

        public TicTacGameResultStatePresenter(TicTacGameResultStateView resultView, TicTacModel gameModel)
        {
            _resultView = resultView ?? throw new ArgumentNullException(nameof(resultView));
            _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
            
            SubscribeToCommands();
        }

        public async UniTask Enter(object param)
        {
            _openNewModuleCommand = param as ReactiveCommand<ModulesMap> ?? throw new ArgumentException("Expected ReactiveCommand<ModulesMap>", nameof(param));
            
            var commands = new TicTacCommands(_restartCommand, _restartCommand, _mainMenuCommand, null);
            _resultView.SetupEventListeners(commands);
            
            // Show result based on game state
            char winner = _gameModel.CheckWinner();
            if (winner != '\0')
            {
                _resultView.ShowWinner(winner);
            }
            else
            {
                _resultView.ShowDraw();
            }
            
            await _resultView.Show();
        }

        public async UniTask Exit()
        {
            if (_resultView.isActiveAndEnabled) 
                await _resultView.Hide();
        }

        public void HideStateInstantly() => _resultView.HideInstantly();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void SubscribeToCommands()
        {
            _restartCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(_ => OnRestartButtonClicked())
                .AddTo(_disposables);
            
            _mainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);
        }

        private async void OnRestartButtonClicked()
        {
            await _gameModel.ChangeState(TicTacGameTriggers.Restart);
        }

        private void OnMainMenuButtonClicked()
        {
            _openNewModuleCommand?.Execute(ModulesMap.MainMenu);
        }
    }
}
