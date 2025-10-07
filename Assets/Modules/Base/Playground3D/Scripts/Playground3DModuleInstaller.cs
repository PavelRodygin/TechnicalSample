using CodeBase.Services.SceneInstallerService;
using Modules.Base.GameModule.Scripts.Gameplay.Systems;
using Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory;
using Modules.Base.Playground3D.Scripts.Network;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.Playground3D.Scripts
{
    public class Playground3DModuleInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private Playground3DNetworkManager networkManager;
        [SerializeField] private Playground3DView playground3DView;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject playerPrefab;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);
            
            builder.Register<Playground3DModuleController>(Lifetime.Singleton);
            
            builder.Register<Playground3DModuleModel>(Lifetime.Singleton);
            builder.Register<Playground3DPresenter>(Lifetime.Singleton);
            builder.RegisterComponent(playground3DView).As<Playground3DView>();
            
            builder.RegisterComponent(gameManager);
            
            // Register player factory following the established pattern
            builder
                .Register<PlayerFactory>(Lifetime.Singleton)
                .WithParameter("playerPrefab", playerPrefab)
                .AsSelf()
                .AsImplementedInterfaces();
            
            // Register network player spawner
            builder
                .Register<PlayerSpawner>(Lifetime.Singleton)
                .WithParameter("fallbackPrefab", playerPrefab)
                .AsSelf()
                .AsImplementedInterfaces();
            
            // Register network manager
            if (networkManager != null)
            {
                builder.RegisterComponent(networkManager).As<Playground3DNetworkManager>();
            }
        }
    }
}
