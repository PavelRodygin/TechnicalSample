using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerGfx : MonoBehaviour
    {

        private Animator _animator;
        private CharacterController _characterController;
        private PlayerMoveController _moveController;
        private PlayerSfx _playerSfx;
        private Player _player;
        private bool _animationsEnabled = true;

        // Public property for accessing Player reference
        public Player Player => _player;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        // PlayerMoveController is a component dependency - get it via GetComponent instead of DI
        // [Inject] - Removed: using GetComponent instead for internal player component dependencies

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _characterController = GetComponent<CharacterController>();
            
            // Get required components from the same GameObject
            _moveController = GetComponent<PlayerMoveController>();
            if (!_moveController)
            {
                Debug.LogError("PlayerMoveController not found on the same GameObject!");
            }
            
            _playerSfx = GetComponent<PlayerSfx>();
            if (!_playerSfx)
            {
                Debug.LogError("PlayerSfx not found on the same GameObject!");
            }
            
            _player = GetComponent<Player>();
            if (!_player)
            {
                Debug.LogError("Player not found on the same GameObject!");
            }

            AssignAnimationIDs();
        }

        private void Update()
        {
            if (_animationsEnabled) UpdateAnimations();
        }
        
        public void OnTowTruckEntered()
        {
            if (!_animationsEnabled) return; // Avoid redundant calls

            _animationsEnabled = false;
            _animator.enabled = false; // Disable animator to freeze animations
        }

        public void OnTowTruckExited()
        {
            if (_animationsEnabled) return; // Avoid redundant calls

            _animationsEnabled = true;
            _animator.enabled = true; // Re-enable animator to resume animations
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void UpdateAnimations()
        {
            if (_animator == null || _player == null) return;
            
            // Use network data for non-owners, local data for owner
            if (_player.IsOwned)
            {
                // Owner uses direct move controller data
                _animator.SetFloat(_animIDSpeed, _moveController.CurrentSpeed);
                _animator.SetFloat(_animIDMotionSpeed, _moveController.InputMagnitude);
                _animator.SetBool(_animIDGrounded, _moveController.IsGrounded);

                if (_moveController.IsJumping)
                {
                    _animator.SetBool(_animIDJump, true);
                }
                else
                {
                    _animator.SetBool(_animIDJump, false);
                }

                if (_moveController.IsFalling)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
                else
                {
                    _animator.SetBool(_animIDFreeFall, false);
                }
            }
            else
            {
                // Non-owners use network synchronized data
                _animator.SetFloat(_animIDSpeed, _player.NetworkSpeed);
                _animator.SetFloat(_animIDMotionSpeed, _player.NetworkSpeed > 0.1f ? 1.0f : 0.0f);
                _animator.SetBool(_animIDGrounded, _player.NetworkIsGrounded);
                
                // For non-owners, we don't have detailed jump/fall states, so simplified logic
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, !_player.NetworkIsGrounded);
            }
        }


        private void OnFootstep(AnimationEvent animationEvent)
        {
            // Only play audio for owner or local player
            if (_playerSfx != null && (_player == null || _player.IsOwned))
                _playerSfx.OnFootstep(animationEvent);
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            // Only play audio for owner or local player
            if (_playerSfx != null && (_player == null || _player.IsOwned))
                _playerSfx.OnLand(animationEvent);
            
            // Handle animation state for owner
            if (animationEvent.animatorClipInfo.weight > 0.5f && _player != null && _player.IsOwned)
            {
                _animator.SetBool(_animIDJump, false);
            }
        }
    }
}