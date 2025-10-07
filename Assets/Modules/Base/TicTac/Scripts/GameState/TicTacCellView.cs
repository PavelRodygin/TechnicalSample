using R3;
using Stateless;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modules.Base.TicTac.Scripts.GameState
{
    public class TicTacCellView : MonoBehaviour
    {
        [SerializeField] private Button cellButton;
        [SerializeField] private TMP_Text cellText;
        [SerializeField] private Image cellBackground; 
        
        private readonly CompositeDisposable _disposables = new();
        private ReactiveCommand<int[]> _onCellClicked;
        private int _x, _y;
        
        private StateMachine<CellState, CellTrigger> _stateMachine;
        
        public enum CellState
        {
            Empty,
            X,
            O
        }
        
        public enum CellTrigger
        {
            SetX,
            SetO,
            Reset
        }

        private void Awake()
        {
            if (cellButton == null)
                Debug.LogWarning($"TicTacCellView {name}: cellButton is not assigned!");
            if (cellText == null)
                Debug.LogWarning($"TicTacCellView {name}: cellText is not assigned!");
            
            InitializeStateMachine();
            
            if (_stateMachine != null && _stateMachine.IsInState(CellState.Empty)) 
                OnEmptyStateEntered();
        }
        
        private void InitializeStateMachine()
        {
            _stateMachine = new StateMachine<CellState, CellTrigger>(CellState.Empty);
            
            // Empty state configuration
            _stateMachine.Configure(CellState.Empty)
                .OnEntry(OnEmptyStateEntered)
                .Permit(CellTrigger.SetX, CellState.X)
                .Permit(CellTrigger.SetO, CellState.O)
                .Ignore(CellTrigger.Reset); // Already empty, ignore reset
            
            // X state configuration
            _stateMachine.Configure(CellState.X)
                .OnEntry(OnXStateEntered)
                .Permit(CellTrigger.Reset, CellState.Empty)
                .Ignore(CellTrigger.SetX) // Already X, ignore
                .Ignore(CellTrigger.SetO); // Can't change to O without reset
            
            // O state configuration
            _stateMachine.Configure(CellState.O)
                .OnEntry(OnOStateEntered)
                .Permit(CellTrigger.Reset, CellState.Empty)
                .Ignore(CellTrigger.SetO) // Already O, ignore
                .Ignore(CellTrigger.SetX); // Can't change to X without reset
        }
        
        public void Initialize(int x, int y, ReactiveCommand<int[]> cellCommand)
        {
            _x = x;
            _y = y;
            _onCellClicked = cellCommand;

            if (cellButton != null)
            {
                cellButton.OnClickAsObservable()
                    .Where(_ => _stateMachine.IsInState(CellState.Empty))
                    .Subscribe(_ => OnCellClicked())
                    .AddTo(_disposables);
            }
            
            // Ensure we start with empty state
            SafeSetEmptyState();
        }
        
        private void SafeSetEmptyState()
        {
            if (_stateMachine != null && _stateMachine.IsInState(CellState.Empty))
            {
                // Manually set the visual state without triggering state machine
                if (cellText != null)
                    cellText.text = "";
                if (cellButton != null)
                    cellButton.interactable = true;
                if (cellBackground != null)
                    cellBackground.color = Color.white;
            }
        }

        private void OnCellClicked()
        {
            _onCellClicked?.Execute(new[] {_x, _y});
        }

        public void SetText(char text)
        {
            if (_stateMachine == null)
            {
                Debug.LogWarning($"TicTacCellView {name}: State machine is not initialized yet!");
                return;
            }
            
            switch (text)
            {
                case '\0':
                    _stateMachine.Fire(CellTrigger.Reset);
                    break;
                    
                case 'X':
                    _stateMachine.Fire(CellTrigger.SetX);
                    break;
                    
                case 'O':
                    _stateMachine.Fire(CellTrigger.SetO);
                    break;
                    
                default:
                    Debug.LogWarning($"TicTacCellView {name}: Unknown text character '{text}'");
                    break;
            }
        }

        public void SetWinningHighlight(bool isWinning)
        {
            if (cellBackground != null)
            {
                cellBackground.color = isWinning ? Color.green : Color.white;
            }
        }

        public void SetBlocked(bool isBlocked)
        {
            if (cellButton != null)
                cellButton.interactable = !isBlocked;
            if (cellBackground != null && isBlocked)
            {
                cellBackground.color = Color.gray;
            }
        }

        // State entry handlers
        private void OnEmptyStateEntered()
        {
            if (cellText != null)
                cellText.text = "";
            if (cellButton != null)
                cellButton.interactable = true;
            if (cellBackground != null)
                cellBackground.color = Color.white;
        }
        
        private void OnXStateEntered()
        {
            if (cellText != null)
                cellText.text = "X";
            if (cellButton != null)
                cellButton.interactable = false;
            if (cellBackground != null)
                cellBackground.color = Color.white;
        }
        
        private void OnOStateEntered()
        {
            if (cellText != null)
                cellText.text = "O";
            if (cellButton != null)
                cellButton.interactable = false;
            if (cellBackground != null)
                cellBackground.color = Color.white;
        }

        public CellState GetCurrentState() => _stateMachine.State;

        private void ClearEventListeners()
        {
            _disposables.Clear();
            _onCellClicked = null;
        }

        private void OnDestroy() => ClearEventListeners();
    }
}