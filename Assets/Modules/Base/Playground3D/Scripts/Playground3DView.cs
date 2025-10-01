using CodeBase.Core.UI.Views;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Unit = R3.Unit;

namespace Modules.Base.Playground3D.Scripts
{
    public readonly struct Playground3DCommands
    {
        public readonly ReactiveCommand<Unit> OpenMainMenuCommand;
        public readonly ReactiveCommand<Unit> SettingsPopupCommand;
        public readonly ReactiveCommand<bool> ToggleSoundCommand;

        public Playground3DCommands(
            ReactiveCommand<Unit> openMainMenuCommand,
            ReactiveCommand<Unit> settingsPopupCommand,
            ReactiveCommand<bool> toggleSoundCommand)
        {
            OpenMainMenuCommand = openMainMenuCommand;
            SettingsPopupCommand = settingsPopupCommand;
            ToggleSoundCommand = toggleSoundCommand;
        }
    }
    
    public class Playground3DView : BaseView
    {
        [Header("Navigation")]
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button settingsButton;
        
        [Header("Sound")]
        [SerializeField] private Toggle soundToggle;

        private Playground3DCommands _commands;

        public void SetupEventListeners(Playground3DCommands commands)
        {
            _commands = commands;
            
            if (mainMenuButton != null)
            {
                mainMenuButton.OnClickAsObservable()
                    .Subscribe(_ => _commands.OpenMainMenuCommand.Execute(Unit.Default))
                    .AddTo(this);
            }
            
            if (settingsButton != null)
            {
                settingsButton.OnClickAsObservable()
                    .Subscribe(_ => _commands.SettingsPopupCommand.Execute(Unit.Default))
                    .AddTo(this);
            }
            
            if (soundToggle != null)
            {
                soundToggle.OnValueChangedAsObservable()
                    .Subscribe(isOn => _commands.ToggleSoundCommand.Execute(isOn))
                    .AddTo(this);
            }
        }

        public void InitializeSoundToggle(bool isMusicOn)
        {
            if (soundToggle != null)
            {
                soundToggle.isOn = isMusicOn;
            }
        }
    }
}
