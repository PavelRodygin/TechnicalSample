using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory
{
    /// <summary>
    /// VContainer factory for creating players with dependency injection
    /// </summary>
    public class PlayerFactory : IPlayerFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly GameObject _playerPrefab;

        public PlayerFactory(IObjectResolver resolver, GameObject playerPrefab)
        {
            _resolver = resolver;
            _playerPrefab = playerPrefab;
        }

        public GameObject Create(Vector3 position, Quaternion rotation)
        {
            Debug.Log($"🏭 PlayerFactory.Create called at {position}");
            
            // Use VContainer's Instantiate method which automatically injects dependencies
            var playerInstance = _resolver.Instantiate(_playerPrefab, position, rotation);
            
            // Initialize player after all dependencies are injected
            var player = playerInstance.GetComponent<Player>();
            if (player)
            {
                player.Initialize(false); // Not in vehicle by default
                Debug.Log($"✅ Player created at {position} with VContainer dependencies injected: {playerInstance.name}");
            }
            else
            {
                Debug.LogError("Player component not found on instantiated prefab!");
            }
            
            return playerInstance;
        }

    }
}
