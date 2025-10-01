using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Patterns.Architecture.MVP;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.TicTac.Scripts.TutorialState
{
    /// <summary>
    /// Presenter for the tutorial state that shows game rules and instructions
    /// </summary>
    public class TicTacTutorialStatePresenter : IPresenter
    {
        private readonly TicTacTutorialStateView _tutorialView;
        private readonly TicTacModel _gameModel;
        private readonly CompositeDisposable _disposables = new();

        private readonly ReactiveCommand<Unit> _continueCommand = new();
        private readonly ReactiveCommand<Unit> _exitCommand = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;

        public ReactiveCommand<Unit> ContinueCommand => _continueCommand;
        public ReactiveCommand<Unit> ExitCommand => _exitCommand;

        public TicTacTutorialStatePresenter(TicTacTutorialStateView tutorialView, TicTacModel gameModel)
        {
            _tutorialView = tutorialView ?? throw new ArgumentNullException(nameof(tutorialView));
            _gameModel = gameModel ?? throw new ArgumentNullException(nameof(gameModel));
            
            SubscribeToCommands();
        }

        public async UniTask Enter(object param)
        {
            _openNewModuleCommand = param as ReactiveCommand<ModulesMap> ?? throw new ArgumentException("Expected ReactiveCommand<ModulesMap>", nameof(param));
            
            var commands = new TicTacCommands(_continueCommand, _exitCommand, _exitCommand, null);
            _tutorialView.SetupEventListeners(commands);
            
            // Show tutorial/rules
            _tutorialView.ShowTutorial();
            await _tutorialView.Show();
        }

        public async UniTask Exit()
        {
            if (_tutorialView.isActiveAndEnabled) 
                await _tutorialView.Hide();
        }

        public void HideStateInstantly() => _tutorialView.HideInstantly();

        public void Dispose()
        {
            _tutorialView.Dispose();
            _disposables.Dispose();
        }

        private void SubscribeToCommands()
        {
            _continueCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(_ => OnContinueButtonClicked())
                .AddTo(_disposables);
            
            _exitCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_gameModel.CommandThrottleDelay))
                .Subscribe(_ => OnExitButtonClicked())
                .AddTo(_disposables);
        }

        private async void OnContinueButtonClicked()
        {
            await _gameModel.ChangeState(TicTacGameTriggers.StartGame);
        }

        private void OnExitButtonClicked()
        {
            _openNewModuleCommand?.Execute(ModulesMap.MainMenu);
        }
    }
}
