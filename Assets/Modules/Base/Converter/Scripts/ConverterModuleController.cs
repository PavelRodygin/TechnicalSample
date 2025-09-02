using System;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.Converter.Scripts
{
    public class ConverterModuleController : IModuleController
    {
        private readonly ConverterScreenPresenter _converterScreenPresenter;
        private readonly ConverterModuleModel _converterModuleModel;
        private readonly IScreenStateMachine _screenStateMachine;
        private readonly ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly CompositeDisposable _disposables = new();
        private readonly UniTaskCompletionSource _moduleCompletionSource;

        public ConverterModuleController(IScreenStateMachine screenStateMachine, ConverterScreenPresenter converterScreenPresenter, 
            ConverterModuleModel converterModuleModel)
        {
            _screenStateMachine = screenStateMachine ?? throw new ArgumentNullException(nameof(screenStateMachine));
            _converterScreenPresenter = converterScreenPresenter ?? throw new ArgumentNullException(nameof(converterScreenPresenter));
            _converterModuleModel = converterModuleModel ?? throw new ArgumentNullException(nameof(converterModuleModel));
            _moduleCompletionSource = new UniTaskCompletionSource();
        }

        public async UniTask Enter(object param)
        {
            SubscribeToModuleUpdates();
            
            _converterScreenPresenter.HideInstantly();
            await _converterScreenPresenter.Enter(_openNewModuleCommand);
        }

        public UniTask Execute() => _moduleCompletionSource.Task;

        public async UniTask Exit()
        {
            await _converterScreenPresenter.Exit();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _converterScreenPresenter.Dispose();
            _converterModuleModel.Dispose();
        }

        private void SubscribeToModuleUpdates()
        {
            // Prevent rapid module switching
            _openNewModuleCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.ModuleTransitionThrottleDelay))
                .Subscribe(RunNewModule)
                .AddTo(_disposables);
        }

        private void RunNewModule(ModulesMap module)
        {
            _moduleCompletionSource.TrySetResult();
            _screenStateMachine.RunModule(module);
        }
    }
}
