using Mirror;
using Modules.Base.Playground3D.Scripts.Gameplay.Player.Factory;
using UnityEngine;
using VContainer;

namespace Modules.Base.Playground3D.Scripts.Network
{
    /// <summary>
    /// Custom NetworkManager that integrates with VContainer and PlayerFactory
    /// for dynamic player creation with dependency injection
    /// </summary>
    public class Playground3DNetworkManager : NetworkManager
    {
        [SerializeField] private GameObject playerPrefabTemplate;
        
        private IPlayerSpawner _playerSpawner;
        private IPlayerFactory _playerFactory;
        
        [Inject]
        private void Construct(IPlayerSpawner playerSpawner, IPlayerFactory playerFactory)
        {
            _playerSpawner = playerSpawner;
            _playerFactory = playerFactory;
        }
        
        #region Server Methods
        
        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("🌐 OnStartServer: Customizing AddPlayer handler");

            NetworkServer.ReplaceHandler<AddPlayerMessage>(CustomOnServerAddPlayerInternal);
        }
        
        private void CustomOnServerAddPlayerInternal(NetworkConnectionToClient conn, AddPlayerMessage msg)
        {
            Debug.Log($"📩 Custom AddPlayerMessage handler for conn {conn.connectionId}");
            
            if (conn.identity)
            {
                Debug.LogError("There is already a player for this connection.");
                return;
            }

            OnServerAddPlayer(conn);
        }
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Debug.Log($"🌐 OnServerAddPlayer called for connection {conn.connectionId}");
    
            Transform startPos = GetStartPosition();
            Vector3 spawnPosition = startPos ? startPos.position : Vector3.zero;
            Quaternion spawnRotation = startPos ? startPos.rotation : Quaternion.identity;
            
            GameObject player = _playerSpawner.SpawnPlayer(conn.connectionId, spawnPosition, spawnRotation);
    
            if (player) 
            {
                NetworkServer.AddPlayerForConnection(conn, player);
                Debug.Log($"✅ Player added: {player.name}");
            }
            else
            {
                Debug.LogError("❌ Failed to create player");
            }
        }
        
        #endregion
        
        #region Client Methods
        
        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("🎬 OnStartClient: Registering custom spawn handlers");
            RegisterCustomSpawnHandlers();
        }
        
        #endregion
        
        #region Custom Spawn Handlers
        
        private void RegisterCustomSpawnHandlers()
        {
            if (!playerPrefabTemplate)
            {
                Debug.LogError("❌ playerPrefabTemplate not set!");
                return;
            }

            var playerIdentity = playerPrefabTemplate.GetComponent<NetworkIdentity>();
            if (!playerIdentity) 
            {
                Debug.LogError("❌ playerPrefabTemplate lacks NetworkIdentity!");
                return;
            }

            NetworkClient.RegisterSpawnHandler(playerIdentity.assetId, SpawnPlayerHandler, UnSpawnPlayerHandler);
            Debug.Log($"✅ Registered custom spawn handler for AssetId: {playerIdentity.assetId}");
        }

        private GameObject SpawnPlayerHandler(SpawnMessage msg)
        {
            Debug.Log($"🎭 Client SpawnHandler: Pos {msg.position}, Rot {msg.rotation}");
            
            try
            {
                // Client-side creation с DI через factory
                var player = _playerFactory.Create(msg.position, msg.rotation);
                if (player != null)
                {
                    player.transform.localScale = msg.scale;
                    Debug.Log($"✅ Client player spawned: {player.name}");
                    return player;
                }
                throw new System.Exception("PlayerFactory returned null");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Client spawn failed: {ex.Message}. Using fallback.");
                // Fallback: Без DI, но чтобы не крашить
                var fallback = Instantiate(playerPrefab, msg.position, msg.rotation);
                fallback.transform.localScale = msg.scale;
                return fallback;
            }
        }

        private void UnSpawnPlayerHandler(GameObject spawned)
        {
            Debug.Log($"🗑️ UnSpawn: {spawned.name}");
            Destroy(spawned);   //TODO Add pooling
        }
        
        #endregion
    }
}