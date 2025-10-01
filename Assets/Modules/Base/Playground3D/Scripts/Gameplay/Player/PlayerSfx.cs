using CodeBase.Core.Systems;
using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerSfx : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private AudioSource audioSource;
        [Space]
        [SerializeField] private AudioClip landingAudioClip;
        [SerializeField] private AudioClip[] footstepAudioClips;
        [Space]
        [Range(0, 1)] [SerializeField] private float footstepAudioVolume = 0.5f;
        [Range(0, 1)] [SerializeField] private float landingAudioVolume = 0.7f;

        private AudioSystem _audioSystem;
        private PlayerMoveController _moveController;
        private Player _player;

        // Public property for accessing Player reference
        public Player Player => _player;

        [Inject]
        private void Construct(AudioSystem audioSystem)
        {
            _audioSystem = audioSystem;
            
            // Subscribe to audio system volume changes
            if (_audioSystem != null)
                _audioSystem.OnSoundsVolumeChanged += OnSoundsVolumeChanged;
        }

        private void Awake()
        {
            // Get required components
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            _moveController = GetComponent<PlayerMoveController>();
            _player = GetComponent<Player>();
            
            if (_moveController == null)
                Debug.LogError("PlayerMoveController not found on the same GameObject!");
            
            if (_player == null)
                Debug.LogError("Player not found on the same GameObject!");
        }

        private void Start()
        {
            // Apply initial audio settings
            if (_audioSystem != null)
                OnSoundsVolumeChanged(_audioSystem.SoundsVolume);
        }

        /// <summary>
        /// Called by Animation Events when player steps
        /// </summary>
        public void OnFootstep(AnimationEvent animationEvent)
        {
            if (!(animationEvent.animatorClipInfo.weight > 0.5f) || footstepAudioClips.Length <= 0) 
                return;
            
            int index = Random.Range(0, footstepAudioClips.Length);
            PlaySfx(footstepAudioClips[index], footstepAudioVolume);
        }

        /// <summary>
        /// Called by Animation Events when player lands
        /// </summary>
        public void OnLand(AnimationEvent animationEvent)
        {
            if (!(animationEvent.animatorClipInfo.weight > 0.5f)) 
                return;
                
            PlaySfx(landingAudioClip, landingAudioVolume);
        }

        /// <summary>
        /// Play a random footstep sound - can be called directly from code
        /// </summary>
        public void PlayFootstepSound()
        {
            if (footstepAudioClips.Length <= 0) return;
            
            int index = Random.Range(0, footstepAudioClips.Length);
            PlaySfx(footstepAudioClips[index], footstepAudioVolume);
        }

        /// <summary>
        /// Play landing sound - can be called directly from code
        /// </summary>
        public void PlayLandingSound()
        {
            PlaySfx(landingAudioClip, landingAudioVolume);
        }

        /// <summary>
        /// Generic method to play any audio clip with volume control
        /// </summary>
        public void PlaySfx(AudioClip clip, float volume = 1.0f)
        {
            if (audioSource == null || clip == null) return;
            
            // Apply global sounds volume from AudioSystem
            float finalVolume = _audioSystem != null ? volume * _audioSystem.SoundsVolume : volume;
            audioSource.PlayOneShot(clip, finalVolume);
        }

        private void OnSoundsVolumeChanged(float newVolume)
        {
            // Update base volume for AudioSource if needed
            // Note: PlayOneShot already applies the volume multiplier
        }

        private void OnDestroy()
        {
            if (_audioSystem != null)
                _audioSystem.OnSoundsVolumeChanged -= OnSoundsVolumeChanged;
        }
    }
}
