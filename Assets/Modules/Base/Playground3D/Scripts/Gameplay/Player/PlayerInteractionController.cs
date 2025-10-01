using CodeBase.Services.Input;
using UnityEngine;
using VContainer;
// using Modules.Additional.ControlsTooltipSystem.Scripts; // Removed - using mock
// using Modules.Base.GameModule.Scripts.Gameplay.TowTrucks; // Removed - not needed for basic movement
// using Modules.Base.GameModule.Scripts.Gameplay.ViolationVehicles; // Removed - not needed for basic movement
// using Modules.Base.GameModule.Scripts.GameState; // Removed - not needed for basic movement
// using Stateless; // Removed - not needed for basic movement

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player
{
    public class PlayerInteractionController : MonoBehaviour
    {
        private const string EnterVehicleTooltipText = "Drive";
        private const string IssueTicketTooltipText = "Issue a fine";

        [SerializeField] private float minLookCosine = 0.9f; // D(cos) = [-1;1]
        [SerializeField] private bool canInteract = true;

        private InputSystemService _inputSystemService;
        private Transform _playerCamera;
        private Player _player;
        
        public bool IsPlayerInVehicle { get; private set; } = false;

        // public event Action OnEnterVehicle;
        // public event Action OnExitVehicle;

        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }

        private void Awake()
        {
            _player = GetComponent<Player>();
        }

        public void Initialize(Transform playerCameraGameObject, bool isPlayerInVehicle)
        {
            _playerCamera = playerCameraGameObject;
            IsPlayerInVehicle = isPlayerInVehicle;
            canInteract = true;
        }

        private void Update()
        {
            // Only process if Player component is available
            if (!_player && _player.isOwned) return;
            
            // Basic movement only - no vehicle interactions
        }
        // Removed all complex interaction methods - only basic movement needed
        
        // Mock tooltip methods - no UI interaction needed for basic movement
        private void ShowTooltip(string tooltipText, string inputActionName) 
        {
            Debug.Log($"[Mock] Show tooltip: {tooltipText} ({inputActionName})");
        }

        private void HideTooltip(string tooltipText) 
        {
            Debug.Log($"[Mock] Hide tooltip: {tooltipText}");
        }
    }
}