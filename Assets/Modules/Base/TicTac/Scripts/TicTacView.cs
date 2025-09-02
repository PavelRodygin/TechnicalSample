using CodeBase.Core.UI.Views;
using CodeBase.Core.UI.Widgets.Buttons;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.TicTac.Scripts
{
    public class TicTacView : BaseView
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

        public void SetupEventListeners(
            ReactiveCommand<Unit> onMainMenuButtonClicked, ReactiveCommand<int[]> onCellClicked, 
            ReactiveCommand<Unit> onRestartButtonClicked, 
            ReactiveCommand<Unit> onThirdPopupButtonClicked)
        {
            Debug.Log("TicTacView: Setting up event listeners");
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => onMainMenuButtonClicked.Execute(Unit.Default));

            if (thirdPopupButton != null)
                thirdPopupButton.onClick.AddListener(() => onThirdPopupButtonClicked.Execute(Unit.Default));

            if (restartButton?.pulsatingButton != null)
                restartButton.pulsatingButton.onClick.AddListener(() => onRestartButtonClicked.Execute(Unit.Default));

            if (cellViews != null)
            {
                Debug.Log($"TicTacView: Initializing {cellViews.Length} cell views");
                
                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        int index = i * BoardSize + j;
                        if (index < cellViews.Length && cellViews[index] != null)
                        {
                            Debug.Log($"TicTacView: Initializing cell view [{i},{j}] at index {index}");
                            cellViews[index].Initialize(i, j, onCellClicked);
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
            
            Debug.Log("TicTacView: Event listeners setup complete");
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
            
            Debug.Log($"TicTacView: Clearing board with {cellViews.Length} cell views");
            
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
                
            Debug.Log("TicTacView: Board cleared successfully");
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
