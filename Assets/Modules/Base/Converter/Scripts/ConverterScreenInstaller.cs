using CodeBase.Services.SceneInstallerService;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.Converter.Scripts
{
    public class ConverterScreenInstaller : BaseModuleSceneInstaller
    {
        [SerializeField] private ConverterView converterView;

        public override void RegisterSceneDependencies(IContainerBuilder builder)
        {
            base.RegisterSceneDependencies(builder);

            builder.Register<ConverterModuleModel>(Lifetime.Singleton);
            builder.Register<ConverterScreenPresenter>(Lifetime.Singleton);
            builder.Register<ConverterModuleController>(Lifetime.Singleton);
            builder.RegisterComponent(converterView).As<ConverterView>();
        }
    }
}