using CodeBase.Core.UI.Views;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

        protected override void Awake()
        {
            base.Awake();
            HideInstantly();
        }

        public void SetupEventListeners(TicTacCommands commands)
        {
            if (continueButton != null)
                continueButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => commands.StartGameCommand.Execute(Unit.Default))
                    .AddTo(this);

            if (exitButton != null)
                exitButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => commands.ExitCommand.Execute(Unit.Default))
                    .AddTo(this);
        }

        public void ShowTutorial()
        {
            if (tutorialMessage != null)
            {
                tutorialMessage.text = "Welcome to Tic-Tac-Toe!\n\nRules:\n- Click on any empty cell to place your mark\n- X goes first, then O\n- Get 3 in a row (horizontal, vertical, or diagonal) to win\n- If the board fills up with no winner, it's a draw\n\nGood luck!";
            }
        }

        public void Dispose()
        {
            // R3 OnClickAsObservable subscriptions are automatically disposed with AddTo(this)
        }
    }
}
