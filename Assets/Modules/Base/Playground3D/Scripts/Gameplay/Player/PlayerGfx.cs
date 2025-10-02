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
            
            _moveController = GetComponent<PlayerMoveController>();
            
            if (!_moveController) 
                Debug.LogError("PlayerMoveController not found on the same GameObject!");
            
            _playerSfx = GetComponent<PlayerSfx>();
            if (!_playerSfx) 
                Debug.LogError("PlayerSfx not found on the same GameObject!");
            
            _player = GetComponent<Player>();
            
            if (!_player) 
                Debug.LogError("Player not found on the same GameObject!");

            AssignAnimationIDs();
        }

        private void Update()
        {
            if (_animationsEnabled) UpdateAnimations();
        }
        
        public void OnTowTruckEntered()
        {
            if (!_animationsEnabled) return;

            _animationsEnabled = false;
            _animator.enabled = false;
        }

        public void OnTowTruckExited()
        {
            if (_animationsEnabled) return;

            _animationsEnabled = true;
            _animator.enabled = true;
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
            if (!_animator || !_player) return;
            
            if (_player.IsLocalPlayer)
            {
                // Owner uses direct move controller data
                _animator.SetFloat(_animIDSpeed, _moveController.CurrentSpeed);
                _animator.SetFloat(_animIDMotionSpeed, _moveController.InputMagnitude);
                _animator.SetBool(_animIDGrounded, _moveController.IsGrounded);

                _animator.SetBool(_animIDJump, _moveController.IsJumping);

                _animator.SetBool(_animIDFreeFall, _moveController.IsFalling);
            }
            else
            {
                // Non-owners use network synchronized data
                _animator.SetFloat(_animIDSpeed, _player.NetworkSpeed);
                _animator.SetFloat(_animIDMotionSpeed, _player.NetworkInputMagnitude);
                _animator.SetBool(_animIDGrounded, _player.NetworkIsGrounded);
                _animator.SetBool(_animIDJump, _player.NetworkIsJumping);
                _animator.SetBool(_animIDFreeFall, _player.NetworkIsFalling);
            }
        }


        private void OnFootstep(AnimationEvent animationEvent)
        {
            // Only play audio for owner or local player
            if (_playerSfx && (!_player || _player.IsLocalPlayer))
                _playerSfx.OnFootstep(animationEvent);
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            // Only play audio for owner or local player
            if (_playerSfx && (!_player || _player.IsLocalPlayer))
                _playerSfx.OnLand(animationEvent);
            
            // Handle animation state for owner
            if (animationEvent.animatorClipInfo.weight > 0.5f && _player && _player.IsLocalPlayer) 
                _animator.SetBool(_animIDJump, false);
        }
    }
}