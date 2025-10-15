using CodeBase.Core.UI.Views;
using CodeBase.Core.UI.Widgets.Buttons;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.TicTac.Scripts.GameState
{
    public class TicTacGameStateView : BaseView
    {
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button thirdPopupButton;
        [SerializeField] private PulsatingButton restartButton;
        [SerializeField] private TicTacCellView[] cellViews;
        [SerializeField] private TMP_Text winnerText;

        private const int BoardSize = 3;

        protected override void Awake()
        {
            if (cellViews.Length != BoardSize * BoardSize)
                Debug.LogError("The number of cell views should be equal to " + (BoardSize * BoardSize));

            base.Awake();
            HideInstantly();
        }

        public void SetupEventListeners(TicTacCommands commands)
        {
            if (mainMenuButton != null)
                mainMenuButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => commands.ExitCommand.Execute(Unit.Default))
                    .AddTo(this);

            if (thirdPopupButton != null)
                thirdPopupButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => { /* Third popup functionality can be added later */ })
                    .AddTo(this);

            if (restartButton?.pulsatingButton != null)
                restartButton.pulsatingButton.OnClickAsObservable()
                    .Where(_ => IsActive)
                    .Subscribe(_ => commands.RestartCommand.Execute(Unit.Default))
                    .AddTo(this);

            if (cellViews != null)
            {
                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        int index = i * BoardSize + j;
                        if (index < cellViews.Length && cellViews[index] != null)
                        {
                            cellViews[index].Initialize(i, j, commands.CellClickCommand);
                        }
                        else
                        {
                            Debug.LogError($"TicTacView: Cannot initialize cell view [{i},{j}] at index {index}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("TicTacView: cellViews array is null!");
            }
        }

        public void UpdateBoard(char[,] board)
        {
            if (cellViews == null || board == null)
            {
                Debug.LogWarning("TicTacView: cellViews or board is null, cannot update board");
                return;
            }
            
            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    int index = i * BoardSize + j;
                    if (index < cellViews.Length && cellViews[index] != null)
                    {
                        cellViews[index].SetText(board[i, j]);
                    }
                }
            }
        }

        public void ClearBoard()
        {
            if (cellViews == null)
            {
                Debug.LogWarning("TicTacView: cellViews array is null, cannot clear board");
                return;
            }
            
            foreach (var cellView in cellViews)
            {
                if (cellView != null)
                {
                    try
                    {
                        cellView.SetText('\0');
                        cellView.SetBlocked(false);
                        cellView.SetWinningHighlight(false);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error clearing cell view {cellView.name}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning("TicTacView: Found null cell view in array");
                }
            }
            
            if (winnerText != null)
                winnerText.text = "";
        }

        public void ShowWinner(char winner) => winnerText.text = $"Winner: {winner}";

        public void ShowDraw() => winnerText.text = "Draw!";

        public void MarkWinningCells(int[][] winningPositions)
        {
            foreach (var position in winningPositions)
            {
                for (int i = 0; i < position.Length; i += 2)
                {
                    int x = position[i];
                    int y = position[i + 1];
                    int index = x * BoardSize + y;
                    cellViews[index].SetWinningHighlight(true);
                }
            }
        }

        public void BlockBoard()
        {
            foreach (var cellView in cellViews) 
                cellView.SetBlocked(true);
        }

        public void UnblockBoard()
        {
            foreach (var cellView in cellViews) 
                cellView.SetBlocked(false);
        }

        public void AnimateRestartButton() => restartButton.PlayAnimation();

        public void StopAnimateRestartButton() => restartButton.StopAnimation();
    }
}
