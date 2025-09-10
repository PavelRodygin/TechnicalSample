using CodeBase.Services.SceneInstallerService;
using Modules.Base.TicTac.Scripts.GameResultState;
using Modules.Base.TicTac.Scripts.GameState;
using Modules.Base.TicTac.Scripts.TutorialState;
using UnityEngine;
using VContainer;

namespace Modules.Base.TicTac.Scripts
{
    public class TicTacModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private TicTacGameStateView ticTacGameStateView;
        [SerializeField] private TicTacGameResultStateView ticTacGameResultStateView;
        [SerializeField] private TicTacTutorialStateView ticTacTutorialStateView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            // Register Views for their respective Presenters
            builder.RegisterInstance(ticTacGameStateView).As<TicTacGameStateView>();
            builder.RegisterInstance(ticTacGameResultStateView).As<TicTacGameResultStateView>();
            builder.RegisterInstance(ticTacTutorialStateView).As<TicTacTutorialStateView>();
            
            builder.Register<TicTacModel>(Lifetime.Singleton);
            
            // Register Presenters (they will get their respective Views injected)
            builder.Register<TicTacGameStatePresenter>(Lifetime.Singleton);
            builder.Register<TicTacGameResultStatePresenter>(Lifetime.Singleton);
            builder.Register<TicTacTutorialStatePresenter>(Lifetime.Singleton);
            
            // Register Controller (it only gets Presenters, not Views)
            builder.Register<TicTacModuleController>(Lifetime.Singleton);
        }
    }
}