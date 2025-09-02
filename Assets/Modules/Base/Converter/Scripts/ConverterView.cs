using System.Globalization;
using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Modules.Base.Converter.Scripts
{
    public class ConverterView : BaseView
    {
        [SerializeField] private TMP_InputField sourceAmountInputField;
        [SerializeField] private TMP_InputField targetAmountInputField;
        [SerializeField] private TMP_Dropdown sourceCurrencyDropdown;
        [SerializeField] private TMP_Dropdown targetCurrencyDropdown;
        [SerializeField] private Scrollbar amountScrollBar;
        [SerializeField] private Button exitButton;

        private InputSystemService _inputSystemService;
        
        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }
        
        protected override void Awake()
        {
            base.Awake();
            
            #if UNITY_EDITOR
            ValidateUIElements();
            #endif
        }
        
        private void ValidateUIElements()
        {
            if (sourceAmountInputField == null) Debug.LogError($"{nameof(sourceAmountInputField)} is not assigned in {nameof(ConverterView)}");
            if (targetAmountInputField == null) Debug.LogError($"{nameof(targetAmountInputField)} is not assigned in {nameof(ConverterView)}");
            if (sourceCurrencyDropdown == null) Debug.LogError($"{nameof(sourceCurrencyDropdown)} is not assigned in {nameof(ConverterView)}");
            if (targetCurrencyDropdown == null) Debug.LogError($"{nameof(targetCurrencyDropdown)} is not assigned in {nameof(ConverterView)}");
            if (amountScrollBar == null) Debug.LogError($"{nameof(amountScrollBar)} is not assigned in {nameof(ConverterView)}");
            if (exitButton == null) Debug.LogError($"{nameof(exitButton)} is not assigned in {nameof(ConverterView)}");
        }

        public void SetupEventListeners(ConverterCommands commands)
        {
            sourceAmountInputField.onValueChanged.AddListener(value => 
            {
                if (IsActive)
                    commands.SourceAmountChangedCommand.Execute(value);
            });

            targetAmountInputField.onValueChanged.AddListener(value => 
            {
                if (IsActive)
                    commands.TargetAmountChangedCommand.Execute(value);
            });

            sourceCurrencyDropdown.onValueChanged.AddListener(index => 
            {
                if (IsActive)
                    commands.DetermineSourceCurrencyCommand.Execute(sourceCurrencyDropdown.options[index].text);
            });

            targetCurrencyDropdown.onValueChanged.AddListener(index => 
            {
                if (IsActive)
                    commands.DetermineTargetCurrencyCommand.Execute(targetCurrencyDropdown.options[index].text);
            });

            amountScrollBar.onValueChanged.AddListener(value => 
            {
                if (IsActive)
                    commands.HandleAmountScrollBarChangedCommand.Execute(value);
            });
            
            exitButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.BackButtonCommand.Execute(default))
                .AddTo(this);
            
            var escapePerformedObservable = 
                _inputSystemService.GetPerformedObservable(_inputSystemService.InputActions.UI.Cancel);
            
            escapePerformedObservable
                .Where(_ => IsActive && IsInteractable)
                .Subscribe(_ => commands.BackButtonCommand.Execute(default))
                .AddTo(this);
        }

        public override async UniTask Show()
        {
            await base.Show();
            
            _inputSystemService.SwitchToUI();
            OnScreenEnabled();
        }
        
        public void OnScreenEnabled()
        {
            _inputSystemService.SetFirstSelectedObject(exitButton);
        }

        public float CurrentSourceAmount =>
            float.TryParse(sourceAmountInputField.text, out var r) ? r : 0f;

        public void UpdateSourceText(float amount) =>
            sourceAmountInputField.SetTextWithoutNotify(amount.ToString(CultureInfo.InvariantCulture));

        public void UpdateTargetText(float amount) =>
            targetAmountInputField.SetTextWithoutNotify(amount.ToString(CultureInfo.InvariantCulture));
    }
}
