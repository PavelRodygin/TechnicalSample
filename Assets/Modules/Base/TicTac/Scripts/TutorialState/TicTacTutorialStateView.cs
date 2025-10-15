using CodeBase.Core.UI.Views;
using CodeBase.Services.Input;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Modules.Base.TicTac.Scripts.TutorialState
{
    /// <summary>
    /// View for the tutorial state that shows game rules and action buttons
    /// </summary>
    public class TicTacTutorialStateView : BaseView
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TMP_Text tutorialMessage;

        private InputSystemService _inputSystemService;

        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }

        protected override void Awake()
        {
            base.Awake();
            HideInstantly();
        }

        public void SetupEventListeners(TicTacCommands commands)
        {
            continueButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.StartGameCommand.Execute(Unit.Default))
                .AddTo(this);

            exitButton.OnClickAsObservable()
                .Where(_ => IsActive)
                .Subscribe(_ => commands.ExitCommand.Execute(Unit.Default))
                .AddTo(this);
        }

        public void ShowTutorial()
        {
            if (tutorialMessage) 
                tutorialMessage.text = "Welcome to Tic-Tac-Toe!\n\nRules:\n- Click on any empty cell to place your mark\n- X goes first, then O\n- Get 3 in a row (horizontal, vertical, or diagonal) to win\n- If the board fills up with no winner, it's a draw\n\nGood luck!";
        }

        public override async UniTask Show()
        {
            _inputSystemService.SetFirstSelectedObject(continueButton);
            await base.Show();
        }
    }
}
