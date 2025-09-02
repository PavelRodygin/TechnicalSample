using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;

namespace Modules.Base.TicTac.Scripts
{
    public class TicTacScreenInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private TicTacView ticTacView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            builder.RegisterInstance(ticTacView).As<TicTacView>();
            builder.Register<TicTacScreenPresenter>(Lifetime.Singleton);
            builder.Register<TicTacModel>(Lifetime.Singleton);
        }
    }
}