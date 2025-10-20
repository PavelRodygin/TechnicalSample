using CodeBase.Shared;
using UnityEngine;

namespace CodeBase.Server
{
    // TODO этот сервис не должен быть MonoBehaviour, также он должен быть в Features.Server
    
    /// <summary>
    /// Устанавливает Environment Variables для сервера в Unity Editor
    /// Выполняется перед всеми другими скриптами
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public class ServerEnvironmentSetup : MonoBehaviour
    {
        private static ServerEnvironmentSetup _instance;
        private static bool _isInitialized;
        
        // TODO данные должны быть в отдельном ScriptableObject, под интерфейсом для значений
        
        [Header("Server Configuration")]
        [SerializeField] private ServerType _serverType = ServerType.BattleRoom;
        [SerializeField] private int _roomCapacity = 4;
        [SerializeField] private int _maxRooms = 1;
        [SerializeField] private ushort _serverPort = 7777;
        [SerializeField] private string _serverId = "dev-server";

        [Header("Settings")]
        [SerializeField] private bool _overrideEnvironmentVariables = true;

        private void Awake()
        {
            _instance = this;
            
            if (ServerUtility.IsServerInstance() && _overrideEnvironmentVariables)
                SetupEnvironmentVariables();
        }

        private void SetupEnvironmentVariables()
        {
            SetEnvironmentVariables(
                _serverType,
                _roomCapacity,
                _maxRooms,
                _serverPort,
                _serverId
            );
        }
        
        private static void SetEnvironmentVariables(
            ServerType serverType,
            int roomCapacity,
            int maxRooms,
            ushort serverPort,
            string serverId)
        {
            if (_isInitialized)
                return;
                
            _isInitialized = true;

            System.Environment.SetEnvironmentVariable("SERVER_TYPE", serverType.ToString());
            System.Environment.SetEnvironmentVariable("ROOM_CAPACITY", roomCapacity.ToString());
            System.Environment.SetEnvironmentVariable("MAX_ROOMS", maxRooms.ToString());
            System.Environment.SetEnvironmentVariable("SERVER_PORT", serverPort.ToString());
            System.Environment.SetEnvironmentVariable("SERVER_ID", serverId);

            ProjectLogger.Log($"Environment variables set:\n" + 
                              $"  SERVER_TYPE={serverType}\n" + 
                              $"  ROOM_CAPACITY={roomCapacity}\n" + 
                              $"  MAX_ROOMS={maxRooms}\n" + 
                              $"  SERVER_PORT={serverPort}\n" + 
                              $"  SERVER_ID={serverId}");
        }
        
        /// <summary>
        /// Принудительная инициализация переменных окружения
        /// </summary>
        public static void EnsureEnvironmentVariablesSet()
        {
            if (_isInitialized)
                return;

            // Если instance уже создан (Awake выполнился), используем его настройки
            if (_instance != null && ServerUtility.IsServerInstance() && _instance._overrideEnvironmentVariables)
            {
                _instance.SetupEnvironmentVariables();
                return;
            }
            
            if (ServerUtility.IsServerInstance())
            {
                SetEnvironmentVariables(
                    ServerType.BattleRoom,
                    4,
                    1, 
                    7777,  
                    "dev-server"  
                );
            }
        }
    }
}

