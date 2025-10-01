using System;
using System.Collections.Generic;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using CodeBase.Core.Patterns.Architecture.MVP;
using Modules.Base.Bootstrap.Scripts;
using Modules.Base.Converter.Scripts;
using Modules.Base.MainMenu.Scripts;
using Modules.Base.Playground3D.Scripts;
using Modules.Base.TicTac.Scripts;
using VContainer;

namespace CodeBase.Implementation.Infrastructure
{
    ///<summary> 
    /// Responsible for resolving (or instantiating) the appropriate module controller for the
    /// specified ModulesMap, using a dependency injection container provided by sceneLifetimeScope
    ///</summary>
    public class ModuleTypeMapper
    {
        private readonly Dictionary<ModulesMap, Type> _map;

        public ModuleTypeMapper()
        {
            _map = new Dictionary<ModulesMap, Type> 
            {
                { ModulesMap.Bootstrap, typeof(BootstrapModuleController) },
                { ModulesMap.Converter, typeof(ConverterModuleController) },
                { ModulesMap.MainMenu, typeof(MainMenuModuleController) },
                { ModulesMap.TicTac, typeof(TicTacModuleController) },
                { ModulesMap.Playground3D, typeof(Playground3DModuleController) },
            };
        }

        public IPresenter Resolve(ModulesMap modulesMap, IObjectResolver objectResolver) => 
            (IPresenter)objectResolver.Resolve(_map[modulesMap]);
        
        public IModuleController ResolveModuleController(ModulesMap modulesMap, IObjectResolver objectResolver) =>
        (IModuleController)objectResolver.Resolve(_map[modulesMap]);
    }
}