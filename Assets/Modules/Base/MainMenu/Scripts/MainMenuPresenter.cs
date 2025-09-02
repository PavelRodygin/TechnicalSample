using System;
using System.Threading;
using System.Threading.Tasks;
using CodeBase.Core.Infrastructure;
using CodeBase.Core.Systems;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using MediatR;
using R3;
using VContainer;
using Unit = R3.Unit;

namespace Modules.Base.MainMenu.Scripts
{
    public class MainMenuRequest : IRequest<string> { }

    public class MainMenuHandler : IRequestHandler<MainMenuRequest, string>
    {
        public Task<string> Handle(MainMenuRequest request, CancellationToken cancellationToken) => 
            Task.FromResult("MainMenu Handler Invoked!");
    }
    
    public class MainMenuPresenter : IDisposable
    {
        [Inject] private IMediator _mediator;
        private readonly MainMenuModuleModel _mainMenuModuleModel;
        private readonly MainMenuView _mainMenuView;
        private readonly IPopupHub _popupHub;
        private readonly AudioSystem _audioSystem;
        
        private readonly ReactiveCommand<Unit> _openConverterCommand = new();
        private readonly ReactiveCommand<Unit> _openTicTacCommand = new();
        private readonly ReactiveCommand<Unit> _openTycoonCommand = new();
        private readonly ReactiveCommand<Unit> _settingsPopupCommand = new();
        private readonly ReactiveCommand<Unit> _secondPopupCommand = new();
        private readonly ReactiveCommand<bool> _toggleSoundCommand = new();
        
        private ReactiveCommand<ModulesMap> _openNewModuleCommand = new();
        private readonly CompositeDisposable _disposables = new();
        
        public MainMenuPresenter(IPopupHub popupHub, MainMenuModuleModel mainMenuModuleModel, 
            MainMenuView mainMenuView, AudioSystem audioSystem)
        {
            _mainMenuModuleModel = mainMenuModuleModel ?? throw new ArgumentNullException(nameof(mainMenuModuleModel));
            _mainMenuView = mainMenuView ?? throw new ArgumentNullException(nameof(mainMenuView));
            _audioSystem = audioSystem ?? throw new ArgumentNullException(nameof(audioSystem));
            _popupHub = popupHub ?? throw new ArgumentNullException(nameof(popupHub));

            SubscribeToUIUpdates();
        }

        private void SubscribeToUIUpdates()
        {
            _openConverterCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnConverterButtonClicked())
                .AddTo(_disposables);
            _openTicTacCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnTicTacButtonClicked())
                .AddTo(_disposables);
            _openTycoonCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnTycoonButtonClicked())
                .AddTo(_disposables);
            _settingsPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSettingsPopupButtonClicked())
                .AddTo(_disposables);
            _secondPopupCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_mainMenuModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnSecondPopupButtonClicked())
                .AddTo(_disposables);
            _toggleSoundCommand.Subscribe(OnSoundToggled).AddTo(_disposables);
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            _mainMenuView.HideInstantly();

            var commands = new MainMenuCommands(
                _openConverterCommand,
                _openTicTacCommand,
                _settingsPopupCommand,
                _secondPopupCommand,
                _toggleSoundCommand
            );

            _mainMenuView.SetupEventListeners(commands);

            _mainMenuView.InitializeSoundToggle(isMusicOn: _audioSystem.MusicVolume != 0);
            await _mainMenuView.Show();
            
            _audioSystem.PlayMainMenuMelody();
        }
        
        public async UniTask Exit()
        {
            await _mainMenuView.Hide();
            _audioSystem.StopMusic();
        }
        
        public void HideInstantly() => _mainMenuView.HideInstantly();

        public void Dispose()
        {
            _disposables?.Dispose();
            _mainMenuView?.Dispose();
            _mainMenuModuleModel?.Dispose();
        }

        private void OnConverterButtonClicked() => _openNewModuleCommand.Execute(ModulesMap.Converter);
        private void OnTicTacButtonClicked() => _openNewModuleCommand.Execute(ModulesMap.TicTac);
        private void OnTycoonButtonClicked() => _openNewModuleCommand.Execute(ModulesMap.DeliveryTycoon);
        private void OnSettingsPopupButtonClicked() => _popupHub.OpenSettingsPopup();
        private void OnSecondPopupButtonClicked() => _popupHub.OpenSecondPopup();
        private void OnSoundToggled(bool isOn) => _audioSystem.SetMusicVolume(isOn ? 1 : 0);
    }
}