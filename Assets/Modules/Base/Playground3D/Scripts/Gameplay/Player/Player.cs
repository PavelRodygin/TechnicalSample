using CodeBase.Services.Input;
using Mirror;
using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(PlayerInteractionController))]
    [RequireComponent(typeof(PlayerMoveController))]
    [RequireComponent(typeof(PlayerGfx))]
    [RequireComponent(typeof(PlayerSfx))]
    public class Player : NetworkBehaviour
    {
        // [SerializeField] private CinemachineVirtualCamera playerVirtualCamera; // Removed - using static camera
        [SerializeField] private Transform playerCameraTransform;
        [SerializeField] private CharacterController characterController;

        private InputSystemService _inputSystemService;
        private Transform _gameWorldTransform;
        private Vector3 _originalPosition;
        
        // Public properties for components to access network data
        [field: SyncVar] 
        public bool IsInVehicle { get; private set; }

        [field: SyncVar]
        public float NetworkSpeed { get; private set; }

        [field: SyncVar] 
        public bool NetworkIsGrounded { get; private set; }
        
        [field: SyncVar]
        public bool NetworkIsJumping { get; private set; }
        
        [field: SyncVar]
        public bool NetworkIsFalling { get; private set; }
        
        [field: SyncVar]
        public float NetworkInputMagnitude { get; private set; }
        
        [field: SyncVar] 
        public Vector3 NetworkPosition { get; private set; }
        
        [field: SyncVar]
        public Quaternion NetworkRotation { get; private set; }

        public bool IsLocalPlayer => isLocalPlayer;
        public bool IsOwned => isOwned;

        // Public properties for component access
        [Header("Controllers")]
        [field: SerializeField] public PlayerMoveController MoveController { get; private set; }
        [field: SerializeField] public PlayerInteractionController InteractionController { get; private set; }
        [field: SerializeField] public PlayerGfx GfxManager { get; private set; }
        [field: SerializeField] public PlayerSfx SfxManager { get; private set; }

        [Inject]
        private void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
            
            // playerInteractionController.OnEnterVehicle += EnterVehicle;
            // playerInteractionController.OnExitVehicle += ExitVehicle;
        }

        // Method to set GameWorldTransform from external source (e.g., GameManager)
        public void SetGameWorldTransform(Transform gameWorldTransform)
        {
            _gameWorldTransform = gameWorldTransform;
        }

        public void Initialize(bool isPlayerInVehicle)
        {
            // playerInteractionController.Initialize(playerCameraTransform.transform, isPlayerInVehicle);
            MoveController.Initialize(playerCameraTransform, isPlayerInVehicle);
        }

        private void Awake()
        {
            if (!MoveController)
                MoveController = GetComponent<PlayerMoveController>();

            if (!InteractionController)
                InteractionController = GetComponent<PlayerInteractionController>();

            if (!GfxManager)
                GfxManager = GetComponent<PlayerGfx>();

            if (!SfxManager)
                SfxManager = GetComponent<PlayerSfx>();
        }

        private void Update()
        {
            // Only owner processes input and updates
            if (IsLocalPlayer)
            {
                if (IsInVehicle) return;
                
                MoveController.UpdateController();
                    
                // Send movement data to server for synchronization
                CmdUpdateMovementData(
                    transform.position,
                    transform.rotation,
                    MoveController.CurrentSpeed,
                    MoveController.IsGrounded,
                    MoveController.IsJumping,
                    MoveController.IsFalling,
                    MoveController.InputMagnitude,
                    InteractionController.IsPlayerInVehicle
                );
            }
            else
            {
                // Non-owners interpolate to network position
                InterpolateToNetworkData();
            }
        }

        private void LateUpdate()
        {
            if (isOwned && !IsInVehicle) 
            {
                MoveController.LateUpdateController();
            }
        }

        [Command]
        private void CmdUpdateMovementData(
            Vector3 position, 
            Quaternion rotation, 
            float speed, 
            bool isGrounded,
            bool isJumping,
            bool isFalling,
            float inputMagnitude,
            bool isInVehicle)
        {
            // Server updates SyncVar fields, which auto-sync to all clients
            NetworkPosition = position;
            NetworkRotation = rotation;
            NetworkSpeed = speed;
            NetworkIsGrounded = isGrounded;
            NetworkIsJumping = isJumping;
            NetworkIsFalling = isFalling;
            NetworkInputMagnitude = inputMagnitude;
            IsInVehicle = isInVehicle;
        }

        private void InterpolateToNetworkData()
        {
            // Smoothly interpolate non-owners to network position
            transform.position = Vector3.Lerp(transform.position, NetworkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Lerp(transform.rotation, NetworkRotation, Time.deltaTime * 10f);
        }

        private void EnterVehicle()
        {
            // Mock vehicle entry - not needed for basic movement
            Debug.Log("[Mock] Player entered vehicle");
        }

        private void ExitVehicle()
        {
            // Mock vehicle exit - not needed for basic movement  
            Debug.Log("[Mock] Player exited vehicle");
        }

        private void OnDestroy()
        {
            // playerInteractionController.OnEnterVehicle -= EnterVehicle;
            // playerInteractionController.OnExitVehicle -= ExitVehicle;
        }
    }
}