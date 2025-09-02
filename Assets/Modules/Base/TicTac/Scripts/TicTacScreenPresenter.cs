using CodeBase.Core.Infrastructure;
using CodeBase.Core.Infrastructure.Modules;
using CodeBase.Core.Systems.PopupHub;
using Cysharp.Threading.Tasks;
using R3;

namespace Modules.Base.TicTac.Scripts
{
    public class TicTacScreenPresenter : IModuleController
    {
        private readonly TicTacView _ticTacView;
        private readonly IScreenStateMachine _screenStateMachine;
        private readonly TicTacModel _newModel;
        private readonly TicTacView _newModuleView;
        private readonly UniTaskCompletionSource<bool> _completionSource;
        private readonly TicTacModel _ticTacModel;
        private readonly IPopupHub _popupHub;
        
        private readonly ReactiveCommand<int[]> _cellCommand = new ReactiveCommand<int[]>();
        private readonly ReactiveCommand<Unit> _mainMenuCommand = new ReactiveCommand<Unit>();
        private readonly ReactiveCommand<Unit> _restartCommand = new ReactiveCommand<Unit>();
        private readonly ReactiveCommand<Unit> _thirdPopupCommand = new ReactiveCommand<Unit>();

        public TicTacScreenPresenter(IScreenStateMachine screenStateMachine,
            TicTacModel newModel, TicTacView newModuleView, 
            TicTacView ticTacView, TicTacModel ticTacModel, IPopupHub popupHub)
        {
            _completionSource = new UniTaskCompletionSource<bool>();

            _screenStateMachine = screenStateMachine;
            _newModel = newModel;
            _newModuleView = newModuleView;
            _ticTacView = ticTacView;
            _ticTacModel = ticTacModel;
            _popupHub = popupHub;
        }

        private void SubscribeToUIUpdates()
        {
            _cellCommand.Subscribe(position => OnCellClicked(position[0], position[1]));
            _mainMenuCommand.Subscribe(_ => OnMainMenuButtonClicked());
            _restartCommand.Subscribe(_ => OnRestartButtonClicked());
            _thirdPopupCommand.Subscribe(_ => OnThirdPopupButtonClicked());
        }

        public async UniTask Enter(object param)
        {
            _ticTacModel.InitializeGame();
            _newModuleView.gameObject.SetActive(false);
            
            // First setup event listeners to initialize cell views
            SubscribeToUIUpdates();
            _ticTacView.SetupEventListeners(_mainMenuCommand, _cellCommand, _restartCommand, 
                _thirdPopupCommand);
            
            // Show the view first
            await _newModuleView.Show();
            
            // Wait a frame to ensure all components are properly initialized
            await UniTask.Yield();
            
            // Now clear the board after everything is ready
            _ticTacView.ClearBoard();
        }

        public async UniTask Execute() => await _completionSource.Task;

        public async UniTask Exit() => await _ticTacView.Hide();

        public void Dispose()
        {
            _newModuleView.Dispose();
            _newModel.Dispose();
        }

        private void RunNewScreen(ModulesMap screen)
        {
            _completionSource.TrySetResult(true);
            _screenStateMachine.RunModule(screen);
        }

        private void OnMainMenuButtonClicked() => RunNewScreen(ModulesMap.MainMenu);

        private void OnCellClicked(int x, int y)
        {
            _ticTacModel.MakeMove(x, y);
            _ticTacView.UpdateBoard(_ticTacModel.Board);
            char winner = _ticTacModel.CheckWinner();
            if (winner != '\0')
            {
                _ticTacView.ShowWinner(winner);
                _ticTacView.MarkWinningCells(_ticTacModel.GetWinningPositions());
                _ticTacView.BlockBoard();
                _ticTacView.AnimateRestartButton();
            }
            else if (_ticTacModel.IsBoardFull())
            {
                _ticTacView.ShowDraw();
                _ticTacView.BlockBoard();
                _ticTacView.AnimateRestartButton();
            }
        }

        private void OnRestartButtonClicked()
        {
            _ticTacModel.InitializeGame();
            _ticTacView.ClearBoard();
            _ticTacView.UnblockBoard(); 
            _ticTacView.StopAnimateRestartButton();
        }

        private void OnThirdPopupButtonClicked() => _popupHub.OpenThirdPopup();
    }
}
