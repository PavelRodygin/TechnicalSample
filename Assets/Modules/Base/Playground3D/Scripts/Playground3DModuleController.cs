using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using Modules.Base.GameModule.Scripts.Gameplay.Systems;
using Modules.Base.Playground3D.Scripts.Gameplay.Player;
using Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory;
using R3;
using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts
{
    public class Playground3DModuleController : IModuleController
    {
        private readonly IScreenStateMachine _screenStateMachine;
        private readonly Playground3DModuleModel _playground3DModuleModel;
        private readonly Playground3DPresenter _playground3DPresenter;
        
        private readonly GameManager _gameManager;
        
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly UniTaskCompletionSource _moduleCompletionSource;

        private readonly CompositeDisposable _disposables = new();
        
        public Playground3DModuleController(IScreenStateMachine screenStateMachine, 
            Playground3DModuleModel playground3DModuleModel, Playground3DPresenter playground3DPresenter, GameManager gameManager)
        {
            _playground3DModuleModel = playground3DModuleModel ?? throw new ArgumentNullException(nameof(playground3DModuleModel));
            _playground3DPresenter = playground3DPresenter ?? throw new ArgumentNullException(nameof(playground3DPresenter));
            _gameManager = gameManager ?? throw new ArgumentNullException(nameof(gameManager));
            _screenStateMachine = screenStateMachine ?? throw new ArgumentNullException(nameof(screenStateMachine));
            
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();

            _playground3DPresenter.HideInstantly();
            
            // Start game - GameManager will handle player spawning and game logic
            _gameManager.StartGame();
            
            await _playground3DPresenter.Enter(_openNewModuleCommand);
        }

        public async UniTask Execute() => await _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            // End game - GameManager will clean up players
            _gameManager.EndGame();
            
            await _playground3DPresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            
            _playground3DPresenter.Dispose();
            
            _playground3DModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_playground3DModuleModel.ModuleTransitionThrottleDelay))
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
