using System.Collections.Generic;
using System.Linq;
using CodeBase.Services.Input;
using Modules.Base.Playground3D.Scripts.Gameplay.Player;
using Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory;
using UnityEngine;
using VContainer;

namespace Modules.Base.GameModule.Scripts.Gameplay.Systems
{
    public class GameManager : MonoBehaviour
    {
        [Header("Game World Settings")]
        [SerializeField] private Transform gameWorldTransform;
        
        [Header("Player Spawn Settings")]
        [SerializeField] private Transform[] playerSpawnPoints;
        [SerializeField] private bool spawnPlayerOnStart = false; // Disabled for network play
        
        private InputSystemService _inputSystemService;
        private IPlayerFactory _playerFactory;
        private readonly List<GameObject> _activePlayers = new();
        private int _nextSpawnIndex = 0;

        public Transform GameWorldTransform => gameWorldTransform;
        public IReadOnlyList<GameObject> ActivePlayers => _activePlayers;
        public bool HasActivePlayers => _activePlayers.Count > 0;
        
        [Inject]
        private void Construct(InputSystemService inputSystemService, IPlayerFactory playerFactory)
        {
            _inputSystemService = inputSystemService;
            _playerFactory = playerFactory;
        }
        
        public void StartGame()
        {
            if (!ValidateConfiguration())
                return;

            InitializeGameInput();
            
            if (spawnPlayerOnStart)
            {
                SpawnPlayer();
            }
            
            Debug.Log("Game started successfully!");
        }

        public void EndGame()
        {
            DestroyAllPlayers();
            Debug.Log("Game ended!");
        }

        /// <summary>
        /// Spawns a new player at the next available spawn point
        /// </summary>
        /// <returns>Created player GameObject or null if failed</returns>
        public GameObject SpawnPlayer()
        {
            var spawnPoint = GetNextSpawnPoint();
            if (spawnPoint == null)
            {
                Debug.LogError("No spawn points available for player creation!");
                return null;
            }

            var player = _playerFactory.Create(spawnPoint.position, spawnPoint.rotation);
            if (player != null)
            {
                _activePlayers.Add(player);
                
                // Set up player in game world
                var playerComponent = player.GetComponent<Player>();
                if (playerComponent != null)
                {
                    playerComponent.SetGameWorldTransform(gameWorldTransform);
                }
                
                player.name = $"Player_{_activePlayers.Count}";
                Debug.Log($"Player spawned successfully at spawn point {_nextSpawnIndex - 1}");
            }

            return player;
        }

        /// <summary>
        /// Removes specific player from the game
        /// </summary>
        /// <param name="player">Player to remove</param>
        public void RemovePlayer(GameObject player)
        {
            if (_activePlayers.Remove(player))
            {
                if (player != null)
                {
                    Destroy(player);
                }
                Debug.Log("Player removed from game");
            }
        }

        /// <summary>
        /// Destroys all active players
        /// </summary>
        public void DestroyAllPlayers()
        {
            foreach (var player in _activePlayers.Where(p => p != null))
            {
                Destroy(player);
            }
            _activePlayers.Clear();
            _nextSpawnIndex = 0;
            Debug.Log("All players destroyed");
        }

        private bool ValidateConfiguration()
        {
            if (gameWorldTransform == null)
            {
                Debug.LogError("GameWorldTransform is not set in GameManager!");
                return false;
            }

            if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
            {
                Debug.LogError("No player spawn points configured in GameManager!");
                return false;
            }

            if (_playerFactory == null)
            {
                Debug.LogError("PlayerFactory is not injected in GameManager!");
                return false;
            }

            return true;
        }

        private Transform GetNextSpawnPoint()
        {
            if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
                return null;

            var spawnPoint = playerSpawnPoints[_nextSpawnIndex % playerSpawnPoints.Length];
            _nextSpawnIndex++;
            return spawnPoint;
        }

        private void InitializeGameInput()
        {
            _inputSystemService?.SwitchToPlayerHumanoid();
        }

        private void OnDestroy()
        {
            DestroyAllPlayers();
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw spawn points in editor
            if (playerSpawnPoints != null)
            {
                Gizmos.color = Color.green;
                for (int i = 0; i < playerSpawnPoints.Length; i++)
                {
                    if (playerSpawnPoints[i] != null)
                    {
                        Gizmos.DrawWireSphere(playerSpawnPoints[i].position, 0.5f);
                        UnityEditor.Handles.Label(playerSpawnPoints[i].position + Vector3.up, $"Spawn {i}");
                    }
                }
            }
        }
        #endif
    }
}