using CodeBase.Services.Input;
using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMoveController : MonoBehaviour
    {
        [Header("Player Settings")]
        [Tooltip("Move speed of the character in m/s")]
        public float moveSpeed = 2.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float sprintSpeed = 5.335f;
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float rotationSmoothTime = 0.12f;
        [Tooltip("Acceleration and deceleration")]
        public float speedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float jumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float gravity = -15.0f;
        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float jumpTimeout = 0.50f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float fallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool grounded = true;
        [Tooltip("Useful for rough ground")]
        public float groundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float groundedRadius = 0.28f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask groundLayers;

        [Header("Camera Settings (Static)")]
        [Tooltip("Static camera - no dynamic camera movement")]
        public bool useStaticCamera = true;

        // Removed Cinemachine variables - using static camera
        private float _speed;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private CharacterController _controller;
        private Transform _mainCamera;
        private InputSystemService _inputSystemService;
        private Player _player;

        // Public property for accessing Player reference
        public Player Player => _player;
        
        // Public properties for passing data to PlayerGfx
        public float CurrentSpeed => _speed;
        public float InputMagnitude => _moveInput.magnitude;
        public bool IsGrounded => grounded;
        public bool IsJumping => _verticalVelocity > 1.0f;
        public bool IsFalling => !grounded && _fallTimeoutDelta <= 0.0f;

        private Vector2 _moveInput;

        [Inject]
        public void Construct(InputSystemService inputSystemService)
        {
            _inputSystemService = inputSystemService;
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _player = GetComponent<Player>();

            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        private void Start()
        {
            // Removed Cinemachine initialization - using static camera
        }

        public void Initialize(Transform playerCamera, bool isPlayerInVehicle)
        {
            _mainCamera = playerCamera;
        }

        public void UpdateController()
        {
            // Only process if Player component is available
            GroundedCheck();
            JumpAndGravity();
            Move();
        }

        public void LateUpdateController()
        {
            // Removed camera rotation - using static camera
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
        }

        // Removed CameraRotation method - using static camera

        private void Move()
        {
            if (_inputSystemService != null)
            {
                _moveInput = _inputSystemService.InputActions.PlayerHumanoid.Move.ReadValue<Vector2>();
                float targetSpeed = _inputSystemService.InputActions.PlayerHumanoid.Sprint.IsPressed()
                    ? sprintSpeed
                    : moveSpeed;

                if (_moveInput == Vector2.zero) targetSpeed = 0.0f;

                var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
                var speedOffset = 0.1f;

                if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                    currentHorizontalSpeed > targetSpeed + speedOffset)
                {
                    _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * _moveInput.magnitude,
                        Time.deltaTime * speedChangeRate);
                    _speed = Mathf.Round(_speed * 1000f) / 1000f;
                }
                else
                    _speed = targetSpeed;
            }
            else
            {
                Debug.Log($"{nameof(InputSystemService)} is null)");
            }

            Vector3 inputDirection = new Vector3(_moveInput.x, 0.0f, _moveInput.y).normalized;
            if (_moveInput != Vector2.zero)
            {
                // Simple movement without camera dependency - movement relative to world space
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        private void JumpAndGravity()
        {
            if (_inputSystemService == null) return;

            if (grounded)
            {
                _fallTimeoutDelta = fallTimeout;

                if (_verticalVelocity < 0.0f) 
                    _verticalVelocity = -2f;

                if (_inputSystemService.InputActions.PlayerHumanoid.Jump.triggered && _jumpTimeoutDelta <= 0.0f)
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                if (_jumpTimeoutDelta >= 0.0f) 
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

                if (_fallTimeoutDelta >= 0.0f) 
                    _fallTimeoutDelta -= Time.deltaTime;
            }

            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                groundedRadius);
        }
    }
}