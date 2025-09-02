using System;
using System.Collections.Generic;
using CodeBase.Core.Infrastructure;
using Cysharp.Threading.Tasks;
using Modules.Additional.DynamicBackground.Scripts;
using R3;
using UnityEngine;
using Unit = R3.Unit;

namespace Modules.Base.Converter.Scripts
{
    public class ConverterScreenPresenter : IDisposable
    {
        private readonly ConverterModuleModel _converterModuleModel;
        private readonly ConverterView _converterView;
        private readonly DynamicParticleController _dynamicParticleController;

        private readonly ReactiveCommand<Unit> _backButtonCommand = new();
        private readonly ReactiveCommand<string> _determineSourceCurrencyCommand = new();
        private readonly ReactiveCommand<string> _determineTargetCurrencyCommand = new();
        private readonly ReactiveCommand<string> _sourceAmountChangedCommand = new();
        private readonly ReactiveCommand<string> _targetAmountChangedCommand = new();
        private readonly ReactiveCommand<float> _handleAmountScrollBarChangedCommand = new();
        
        private readonly Dictionary<string, Currencies> _currencyToName = new()
        {
            { "EUR", Currencies.Eur },
            { "USD", Currencies.Usd },
            { "PLN", Currencies.Pln },
            { "PR", Currencies.Pr }
        };
        
        private readonly CompositeDisposable _disposables = new();
        private ReactiveCommand<ModulesMap> _openNewModuleCommand;
        
        public ConverterScreenPresenter(ConverterModuleModel converterModuleModel, 
            ConverterView converterView, DynamicParticleController dynamicParticleController)
        {
            _converterModuleModel = converterModuleModel ?? throw new ArgumentNullException(nameof(converterModuleModel));
            _converterView = converterView ?? throw new ArgumentNullException(nameof(converterView));
            _dynamicParticleController = dynamicParticleController ?? throw new ArgumentNullException(nameof(dynamicParticleController));

            SubscribeToUIUpdates();
        }

        private void SubscribeToUIUpdates()
        {
            _backButtonCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.CommandThrottleDelay))
                .Subscribe(_ => OnExitButtonClicked())
                .AddTo(_disposables);
                
            _determineSourceCurrencyCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.CommandThrottleDelay))
                .Subscribe(DetermineSourceCurrency)
                .AddTo(_disposables);
                
            _determineTargetCurrencyCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.CommandThrottleDelay))
                .Subscribe(DetermineTargetCurrency)
                .AddTo(_disposables);
                
            _sourceAmountChangedCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.CommandThrottleDelay))
                .Subscribe(OnSourceAmountChanged)
                .AddTo(_disposables);
                
            _targetAmountChangedCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.CommandThrottleDelay))
                .Subscribe(OnTargetAmountChanged)
                .AddTo(_disposables);
                
            _handleAmountScrollBarChangedCommand
                .ThrottleFirst(TimeSpan.FromMilliseconds(_converterModuleModel.CommandThrottleDelay))
                .Subscribe(HandleAmountScrollBarChanged)
                .AddTo(_disposables);
        }

        public async UniTask Enter(ReactiveCommand<ModulesMap> runModuleCommand)
        {
            _openNewModuleCommand = runModuleCommand ?? throw new ArgumentNullException(nameof(runModuleCommand));
            
            var commands = new ConverterCommands(
                _determineSourceCurrencyCommand,
                _determineTargetCurrencyCommand,
                _sourceAmountChangedCommand,
                _targetAmountChangedCommand,
                _handleAmountScrollBarChangedCommand,
                _backButtonCommand
            );
            
            _converterView.SetupEventListeners(commands);
            await _converterView.Show();
        }

        public async UniTask Exit() => await _converterView.Hide();

        public void HideInstantly() => _converterView.HideInstantly();

        public void Dispose()
        {
            _disposables?.Dispose();
            _converterView?.Dispose();
            _converterModuleModel?.Dispose();
        }

        private void DetermineSourceCurrency(string name)
        {
            _converterModuleModel.SelectSourceCurrency(_currencyToName[name]);
            CountTargetMoney(_converterView.CurrentSourceAmount);
        }

        private void DetermineTargetCurrency(string name) 
        {
            _converterModuleModel.SelectTargetCurrency(_currencyToName[name]);
            CountTargetMoney(_converterView.CurrentSourceAmount);
        }

        private void OnSourceAmountChanged(string value)
        {
            if (float.TryParse(value, out var amount)) 
                CountTargetMoney(amount);
        }

        private void OnTargetAmountChanged(string value)
        {
            if (float.TryParse(value, out var amount)) 
                CountSourceMoney(amount);
        }

        private void CountSourceMoney(float count) =>
            _converterView.UpdateSourceText(_converterModuleModel.ConvertTargetToSource(count));

        private void HandleAmountScrollBarChanged(float scrollValue)
        {
            _dynamicParticleController.parameter = scrollValue;
            var intValue = Mathf.RoundToInt(scrollValue * 200);
            _converterView.UpdateSourceText(intValue);
            CountTargetMoney(intValue); 
        }
        
        private void CountTargetMoney(float count) =>
            _converterView.UpdateTargetText(_converterModuleModel.ConvertSourceToTarget(count));

        private void OnExitButtonClicked() => 
            RunNewScreen(ModulesMap.MainMenu);

        private void RunNewScreen(ModulesMap screen)
        {
            _openNewModuleCommand.Execute(screen);
        }
    }
}
