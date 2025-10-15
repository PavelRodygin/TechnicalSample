using Mirror;
using Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory;
using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts.Network
{
    /// <summary>
    /// Network player spawner that uses PlayerFactory for creating players with DI
    /// </summary>
    public class PlayerSpawner : IPlayerSpawner
    {
        private readonly IPlayerFactory _playerFactory;

        [Inject]
        public PlayerSpawner(IPlayerFactory playerFactory)
        {
            _playerFactory = playerFactory;
        }

        public GameObject SpawnPlayer(int connId, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            GameObject player = null;
            
            // Try to use PlayerFactory
            if (_playerFactory != null)
            {
                try
                {
                    player = _playerFactory.Create(spawnPosition, spawnRotation);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"❌ Failed to create network player using PlayerFactory: {ex.Message}");
                }
            }

            if (!player) return player;
            
            // Ensure NetworkIdentity exists
            if (!player.TryGetComponent<NetworkIdentity>(out _))
            {
                Object.Destroy(player);
                return null;
            }
                
            player.name = $"Player [connId={connId}]";  // Now with real connId

            return player;
        }
    }
}