using R3;

namespace Modules.Base.Converter.Scripts
{
    public readonly struct ConverterCommands
    {
        public readonly ReactiveCommand<string> DetermineSourceCurrencyCommand;
        public readonly ReactiveCommand<string> DetermineTargetCurrencyCommand;
        public readonly ReactiveCommand<string> SourceAmountChangedCommand;
        public readonly ReactiveCommand<string> TargetAmountChangedCommand;
        public readonly ReactiveCommand<float> HandleAmountScrollBarChangedCommand;
        public readonly ReactiveCommand<Unit> BackButtonCommand;

        public ConverterCommands(
            ReactiveCommand<string> determineSourceCurrencyCommand,
            ReactiveCommand<string> determineTargetCurrencyCommand,
            ReactiveCommand<string> sourceAmountChangedCommand,
            ReactiveCommand<string> targetAmountChangedCommand,
            ReactiveCommand<float> handleAmountScrollBarChangedCommand,
            ReactiveCommand<Unit> backButtonCommand)
        {
            DetermineSourceCurrencyCommand = determineSourceCurrencyCommand;
            DetermineTargetCurrencyCommand = determineTargetCurrencyCommand;
            SourceAmountChangedCommand = sourceAmountChangedCommand;
            TargetAmountChangedCommand = targetAmountChangedCommand;
            HandleAmountScrollBarChangedCommand = handleAmountScrollBarChangedCommand;
            BackButtonCommand = backButtonCommand;
        }
    }
}
