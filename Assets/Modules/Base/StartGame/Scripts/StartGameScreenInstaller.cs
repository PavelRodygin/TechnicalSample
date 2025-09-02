using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;

namespace Modules.Base.StartGame.Scripts
{
    public class StartGameScreenInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private StartGameView startGameView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            builder.RegisterInstance(startGameView).As<StartGameView>();
            builder.Register<StartGameScreenPresenter>(Lifetime.Singleton);
            builder.Register<StartGameModel>(Lifetime.Singleton);
        }
    }
}