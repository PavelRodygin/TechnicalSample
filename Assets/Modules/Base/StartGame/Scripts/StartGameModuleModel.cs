using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeBase.Core.Patterns.Architecture.MVP;
using CodeBase.Services.LongInitializationServices;
using DG.Tweening;
using DG.Tweening.Core.Enums;

namespace Modules.Base.StartGame.Scripts
{
    public class StartGameModel : IModel
    {
        public readonly Dictionary<string, Func<Task>> Commands;

        private readonly FirstLongInitializationService _firstLongInitializationService;
        private readonly SecondLongInitializationService _secondLongInitializationService;
        private readonly ThirdLongInitializationService _thirdLongInitializationService;
        
        private readonly string[] _tooltips;
        private int _currentTooltipIndex;

        public StartGameModel(FirstLongInitializationService firstLongInitializationService,
            SecondLongInitializationService secondLongInitializationService,
            ThirdLongInitializationService thirdLongInitializationService)
        {
            _firstLongInitializationService = firstLongInitializationService;
            _secondLongInitializationService = secondLongInitializationService;
            _thirdLongInitializationService = thirdLongInitializationService;

            Commands = new Dictionary<string, Func<Task>>();
            
            _tooltips = new []
            {
                "Monitor parked cars carefully! Violators should be fined or towed.",
                "Tow illegally parked vehicles to keep the streets clear.",
                "Check license plates and issue fines to traffic violators.",
                "Be quick! Some drivers might return to their cars in time.",
                "Patrol the city and maintain order on the roads.",
                "Use your police tablet to scan for violations and issue tickets.",
                "Pay attention to road signs! Illegal parking means towing.",
                "Improve your skills as an officer and become the best in the city.",
                "Some drivers will try to argue—stand your ground and enforce the law.",
                "Earn rewards for efficient towing and accurate fines."
            };
        }
        
        public void DoTweenInit()
        {
            DOTween.Init().SetCapacity(240, 30);
            DOTween.safeModeLogBehaviour = SafeModeLogBehaviour.None;
            DOTween.defaultAutoKill = true;
            DOTween.defaultRecyclable = true;
            DOTween.useSmoothDeltaTime = true;
        }

        public void RegisterCommands()
        {
            Commands.Add("First Service", _firstLongInitializationService.Init);
            Commands.Add("Second Service", _secondLongInitializationService.Init);
            Commands.Add("Third Service", _thirdLongInitializationService.Init);
        }
        
        public string GetNextTooltip()
        {
            var tooltip = _tooltips[_currentTooltipIndex];
            _currentTooltipIndex = (_currentTooltipIndex + 1) % _tooltips.Length;
            return tooltip;
        }

        public void Dispose() => Commands.Clear();
    }
}
