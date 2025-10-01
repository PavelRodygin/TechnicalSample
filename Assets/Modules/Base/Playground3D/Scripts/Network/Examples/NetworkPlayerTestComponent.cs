using Mirror;
using UnityEngine;

namespace Modules.Base.Playground3D.Scripts.Network.Examples
{
    /// <summary>
    /// Example component for testing network player spawning with PlayerFactory
    /// Add this to a GameObject to test network functionality
    /// </summary>
    public class NetworkPlayerTestComponent : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField, Tooltip("Key to press to start server")]
        private KeyCode startServerKey = KeyCode.S;
        
        [SerializeField, Tooltip("Key to press to start client")]  
        private KeyCode startClientKey = KeyCode.C;
        
        [SerializeField, Tooltip("Key to press to start host")]
        private KeyCode startHostKey = KeyCode.H;
        
        [SerializeField, Tooltip("Key to press to stop")]
        private KeyCode stopKey = KeyCode.Escape;

        private Playground3DNetworkManager _networkManager;

        void Start()
        {
            // Find our custom NetworkManager
            _networkManager = FindObjectOfType<Playground3DNetworkManager>();
            
            if (_networkManager == null)
            {
                Debug.LogError("❌ Playground3DNetworkManager not found in scene!");
                enabled = false;
                return;
            }
            
            Debug.Log($"✓ Found Playground3DNetworkManager. Use keys: {startServerKey} (Server), {startClientKey} (Client), {startHostKey} (Host), {stopKey} (Stop)");
        }

        void Update()
        {
            if (_networkManager == null) return;

            // Server
            if (Input.GetKeyDown(startServerKey) && !NetworkServer.active && !NetworkClient.active)
            {
                Debug.Log("🖥️ Starting server...");
                _networkManager.StartServer();
            }
            
            // Client
            if (Input.GetKeyDown(startClientKey) && !NetworkServer.active && !NetworkClient.active)
            {
                Debug.Log("💻 Starting client...");
                _networkManager.StartClient();
            }
            
            // Host
            if (Input.GetKeyDown(startHostKey) && !NetworkServer.active && !NetworkClient.active)
            {
                Debug.Log("🏠 Starting host...");
                _networkManager.StartHost();
            }
            
            // Stop
            if (Input.GetKeyDown(stopKey) && (NetworkServer.active || NetworkClient.active))
            {
                Debug.Log("🛑 Stopping network...");
                if (NetworkServer.active && NetworkClient.active)
                {
                    _networkManager.StopHost();
                }
                else if (NetworkServer.active)
                {
                    _networkManager.StopServer();
                }
                else if (NetworkClient.active)
                {
                    _networkManager.StopClient();
                }
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            
            GUILayout.Label("Network Player Factory Test", GUI.skin.box);
            GUILayout.Space(10);
            
            if (!NetworkServer.active && !NetworkClient.active)
            {
                GUILayout.Label("Network Status: Offline");
                
                if (GUILayout.Button($"Start Server ({startServerKey})"))
                {
                    _networkManager.StartServer();
                }
                
                if (GUILayout.Button($"Start Client ({startClientKey})"))
                {
                    _networkManager.StartClient();
                }
                
                if (GUILayout.Button($"Start Host ({startHostKey})"))
                {
                    _networkManager.StartHost();
                }
            }
            else
            {
                if (NetworkServer.active && NetworkClient.active)
                {
                    GUILayout.Label("Network Status: Host");
                }
                else if (NetworkServer.active)
                {
                    GUILayout.Label("Network Status: Server");
                }
                else if (NetworkClient.active)
                {
                    GUILayout.Label("Network Status: Client");
                }
                
                GUILayout.Label($"Connected Clients: {NetworkServer.connections.Count}");
                
                if (GUILayout.Button($"Stop ({stopKey})"))
                {
                    if (NetworkServer.active && NetworkClient.active)
                    {
                        _networkManager.StopHost();
                    }
                    else if (NetworkServer.active)
                    {
                        _networkManager.StopServer();
                    }
                    else if (NetworkClient.active)
                    {
                        _networkManager.StopClient();
                    }
                }
            }
            
            GUILayout.EndArea();
        }
    }
}
