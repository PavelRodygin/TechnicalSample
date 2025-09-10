using CodeBase.Core.UI.Views;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.TicTac.Scripts.GameResultState
{
    /// <summary>
    /// View for the result state that shows game outcome (win/draw) and action buttons
    /// </summary>
    public class TicTacGameResultStateView : BaseView
    {
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private TMP_Text resultText;

        protected override void Awake()
        {
            base.Awake();
            HideInstantly();
        }

        public void SetupEventListeners(TicTacCommands commands)
        {
            if (restartButton != null)
                restartButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => commands.RestartCommand.Execute(Unit.Default))
                    .AddTo(this);

            if (mainMenuButton != null)
                mainMenuButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => commands.ExitCommand.Execute(Unit.Default))
                    .AddTo(this);
        }

        public void ShowWinner(char winner)
        {
            if (resultText) resultText.text = $"Player {winner} Wins!";
        }

        public void ShowDraw()
        {
            if (resultText) resultText.text = "It's a Draw!";
        }

        public override void Dispose()
        {
            base.Dispose();
            // R3 OnClickAsObservable subscriptions are automatically disposed with AddTo(this)
        }
    }
}
