using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using Modules.Base.TicTac.Scripts.GameResultState;
using Modules.Base.TicTac.Scripts.GameState;
using Modules.Base.TicTac.Scripts.TutorialState;
using R3;
using Stateless;
using UnityEngine;

namespace Modules.Base.TicTac.Scripts
{
    /// <summary>
    /// Main controller for TicTac module that manages the module lifecycle
    /// and coordinates between separate state presenters for Stateless demonstration
    /// </summary>
    public class TicTacModuleController : IModuleController
    {
        private readonly IScreenStateMachine _screenStateMachine;
        private readonly TicTacModel _moduleModel;
        private readonly TicTacGameStatePresenter _gameStatePresenter;
        private readonly TicTacGameResultStatePresenter _gameResultStatePresenter;
        private readonly TicTacTutorialStatePresenter _tutorialStatePresenter;
        private readonly UniTaskCompletionSource _moduleCompletionSource;
        private readonly CompositeDisposable _disposables = new();

        public TicTacModuleController(IScreenStateMachine screenStateMachine, TicTacModel moduleModel, 
            TicTacGameStatePresenter gameStatePresenter,
            TicTacGameResultStatePresenter gameResultStatePresenter, 
            TicTacTutorialStatePresenter tutorialStatePresenter)
        {
            _screenStateMachine = screenStateMachine ?? throw new ArgumentNullException(nameof(screenStateMachine));
            _moduleModel = moduleModel ?? throw new ArgumentNullException(nameof(moduleModel));
            _gameStatePresenter = gameStatePresenter ?? throw new ArgumentNullException(nameof(gameStatePresenter));
            _gameResultStatePresenter = gameResultStatePresenter ?? throw new ArgumentNullException(nameof(gameResultStatePresenter));
            _tutorialStatePresenter = tutorialStatePresenter ?? throw new ArgumentNullException(nameof(tutorialStatePresenter));
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            // Configure FSM in model with separate state presenters
            _moduleModel.ConfigureStateMachine(
                onEnterTutorial: () => _tutorialStatePresenter.Enter(null),
                onExitTutorial: () => _tutorialStatePresenter.Exit(),
                onEnterGame: () => _gameStatePresenter.Enter(null),
                onExitGame: () => _gameStatePresenter.Exit(),
                onEnterResult: () => _gameResultStatePresenter.Enter(null),
                onExitResult: () => _gameResultStatePresenter.Exit()
            );

            _moduleModel.StateMachine.OnTransitionCompleted(OnChangeState);
            SubscribeToCommands();

            _gameStatePresenter.HideStateInstantly();
            _gameResultStatePresenter.HideStateInstantly();
            _tutorialStatePresenter.HideStateInstantly();
            
            // Start with tutorial state through state machine
            await _moduleModel.ChangeState(TicTacGameTriggers.InitializeTutorial);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            _disposables.Dispose();
            await UniTask.WhenAll(_gameStatePresenter.Exit(), _gameResultStatePresenter.Exit(), _tutorialStatePresenter.Exit());
        }

        public void Dispose()
        {
            _moduleModel.Dispose();
            _gameStatePresenter.Dispose();
            _gameResultStatePresenter.Dispose();
            _tutorialStatePresenter.Dispose();
            _disposables.Dispose();
        }

        private void OnChangeState(StateMachine<TicTacGameStates, TicTacGameTriggers>.Transition transition)
        {
            Debug.Log($"TicTac: Transitioned to {transition.Destination}");
        }

        private void SubscribeToCommands()
        {
            // Subscribe to tutorial state commands with throttling
            _tutorialStatePresenter.ContinueCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(_ => OnContinueButtonClicked())
                .AddTo(_disposables);
            
            _tutorialStatePresenter.ExitCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(_ => OnExitButtonClicked())
                .AddTo(_disposables);

            // Subscribe to game state commands with throttling
            _gameStatePresenter.CellCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.CommandThrottleDelay))
                .Subscribe(OnCellClicked)
                .AddTo(_disposables);
            
            _gameStatePresenter.RestartCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(_ => OnRestartButtonClicked())
                .AddTo(_disposables);
            
            _gameStatePresenter.MainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);

            // Subscribe to result state commands with throttling
            _gameResultStatePresenter.RestartCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(_ => OnRestartButtonClicked())
                .AddTo(_disposables);
            
            _gameResultStatePresenter.MainMenuCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(_ => OnMainMenuButtonClicked())
                .AddTo(_disposables);
        }

        private async void OnCellClicked(int[] position)
        {
            if (_moduleModel.StateMachine.State != TicTacGameStates.Game) return;
            
            _moduleModel.MakeMove(position[0], position[1]);
            
            // Notify GameStatePresenter to update its view
            _gameStatePresenter.UpdateBoardDisplay(_moduleModel.Board);
                
            char winner = _moduleModel.CheckWinner();
            
            if (winner != '\0')
            {
                var winningPositions = _moduleModel.GetWinningPositions();
                _gameStatePresenter.MarkWinningCells(winningPositions);
                await _moduleModel.ChangeState(TicTacGameTriggers.PlayerWon);
            }
            else if (_moduleModel.IsBoardFull()) 
            {
                await _moduleModel.ChangeState(TicTacGameTriggers.GameDraw);
            }
        }

        private async void OnRestartButtonClicked() => 
            await _moduleModel.ChangeState(TicTacGameTriggers.Restart);

        private async void OnContinueButtonClicked() => 
            await _moduleModel.ChangeState(TicTacGameTriggers.StartGame);

        private void OnExitButtonClicked()
        {
            _moduleCompletionSource.TrySetResult();
            _screenStateMachine.RunModule(ModulesMap.MainMenu);
        }

        private void OnMainMenuButtonClicked()
        {
            _moduleCompletionSource.TrySetResult();
            _screenStateMachine.RunModule(ModulesMap.MainMenu);
        }
    }
}
