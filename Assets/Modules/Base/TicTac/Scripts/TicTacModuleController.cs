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
        
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();

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
            SubscribeToModuleUpdates();
            
            // Configure FSM in model with separate state presenters
            _moduleModel.ConfigureStateMachine(
                onEnterTutorial: async () => await _tutorialStatePresenter.Enter(_openNewModuleCommand),
                onExitTutorial: () => _tutorialStatePresenter.Exit(),
                onEnterGame: async () => await _gameStatePresenter.Enter(_openNewModuleCommand),
                onExitGame: () => _gameStatePresenter.Exit(),
                onEnterResult: async () => await _gameResultStatePresenter.Enter(_openNewModuleCommand),
                onExitResult: () => _gameResultStatePresenter.Exit()
            );

            _moduleModel.StateMachine.OnTransitionCompleted(OnChangeState);

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
            Debug.Log($"OnChangeState: {transition.Trigger} -> {transition.Destination}");
        }

        private void SubscribeToModuleUpdates()
        {
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
                .Subscribe(RunNewModule)
                .AddTo(_disposables);
        }

        private void RunNewModule(ModulesMap screen)
        {
            _moduleCompletionSource.TrySetResult();
            _screenStateMachine.RunModule(screen);
        }
    }
}
