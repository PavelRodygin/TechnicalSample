using System.Collections.Generic;
using CodeBase.Services;
using CodeBase.Shared;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace CodeBase.Server.Rooms
{
    /// <summary>
    /// Управление жизненным циклом комнат убежища
    /// Каждая комната загружается как аддитивная сцена с собственным Scope
    /// </summary>
    public class ShelterRoomLifecycleManager : IRoomLifecycleManager
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IServerConfig _serverConfig;
        private readonly LifetimeScope _parentScope;
        private readonly Dictionary<string, RoomInstance> _rooms;
        
        // Имя сцены убежища для загрузки
        private const string ShelterSceneName = "ShelterRoom";

        [Inject]
        public ShelterRoomLifecycleManager(
            ISceneLoader sceneLoader,
            IServerConfig serverConfig,
            LifetimeScope parentScope)
        {
            _sceneLoader = sceneLoader;
            _serverConfig = serverConfig;
            _parentScope = parentScope;
            _rooms = new Dictionary<string, RoomInstance>();
        }

        public async UniTask<RoomInstance> CreateRoomAsync(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                ProjectLogger.LogWarning($"[ShelterRoomLifecycle] Room {roomId} already exists");
                return _rooms[roomId];
            }

            try
            {
                // Загружаем аддитивную сцену для комнаты
                ProjectLogger.Log($"[ShelterRoomLifecycle] Loading scene for room {roomId}");
                await _sceneLoader.LoadSceneAsyncAdditive(ShelterSceneName);
                
                var scene = SceneManager.GetSceneByName(ShelterSceneName);
                if (!scene.IsValid())
                {
                    ProjectLogger.LogError($"[ShelterRoomLifecycle] Failed to load scene for room {roomId}");
                    return null;
                }

                // Создаем Scope для комнаты
                var roomScope = CreateRoomScope(roomId, scene);
                
                // Создаем экземпляр комнаты
                var roomInstance = new RoomInstance(
                    roomId, 
                    scene, 
                    roomScope, 
                    _serverConfig.RoomCapacity);
                
                _rooms.Add(roomId, roomInstance);
                
                ProjectLogger.Log($"[ShelterRoomLifecycle] Room {roomId} created successfully (scene: {scene.name})");
                return roomInstance;
            }
            catch (System.Exception ex)
            {
                ProjectLogger.LogError($"[ShelterRoomLifecycle] Failed to create room {roomId}: {ex.Message}");
                return null;
            }
        }

        public async UniTask DestroyRoomAsync(string roomId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogWarning($"[ShelterRoomLifecycle] Room {roomId} not found");
                return;
            }

            try
            {
                // Уничтожаем Scope
                if (room.RoomScope != null)
                {
                    room.RoomScope.Dispose();
                }

                // Выгружаем сцену
                if (room.Scene.IsValid())
                {
                    await SceneManager.UnloadSceneAsync(room.Scene);
                }

                _rooms.Remove(roomId);
                
                ProjectLogger.Log($"[ShelterRoomLifecycle] Room {roomId} destroyed");
            }
            catch (System.Exception ex)
            {
                ProjectLogger.LogError($"[ShelterRoomLifecycle] Failed to destroy room {roomId}: {ex.Message}");
            }
        }

        public async UniTask<bool> ConnectPlayerToRoomAsync(string roomId, uint playerId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogError($"[ShelterRoomLifecycle] Room {roomId} not found");
                return false;
            }

            if (room.IsFull)
            {
                ProjectLogger.LogWarning($"[ShelterRoomLifecycle] Room {roomId} is full");
                return false;
            }

            if (!room.AddPlayer(playerId))
            {
                ProjectLogger.LogWarning($"[ShelterRoomLifecycle] Player {playerId} already in room {roomId}");
                return false;
            }

            // TODO: Переместить объект игрока в сцену комнаты
            // SceneManager.MoveGameObjectToScene(playerGameObject, room.Scene);
            
            ProjectLogger.Log($"[ShelterRoomLifecycle] Player {playerId} connected to room {roomId} ({room.PlayerIds.Count}/{room.MaxPlayers})");
            
            await UniTask.Yield();
            return true;
        }

        public async UniTask<bool> DisconnectPlayerFromRoomAsync(string roomId, uint playerId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogError($"[ShelterRoomLifecycle] Room {roomId} not found");
                return false;
            }

            if (!room.RemovePlayer(playerId))
            {
                ProjectLogger.LogWarning($"[ShelterRoomLifecycle] Player {playerId} not in room {roomId}");
                return false;
            }

            ProjectLogger.Log($"[ShelterRoomLifecycle] Player {playerId} disconnected from room {roomId} ({room.PlayerIds.Count}/{room.MaxPlayers})");

            // Если комната опустела, удаляем её
            if (room.IsEmpty)
            {
                ProjectLogger.Log($"[ShelterRoomLifecycle] Room {roomId} is empty, destroying...");
                await DestroyRoomAsync(roomId);
            }

            return true;
        }

        public RoomInstance GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }

        private LifetimeScope CreateRoomScope(string roomId, Scene scene)
        {
            // Создаем GameObject для Scope в загруженной сцене
            var scopeObject = new GameObject($"RoomScope_{roomId}");
            SceneManager.MoveGameObjectToScene(scopeObject, scene);
            
            // Создаем и настраиваем LifetimeScope
            var roomScope = scopeObject.AddComponent<LifetimeScope>();
            roomScope.autoRun = false;
            
            // TODO: Настроить билдер для регистрации специфичных для комнаты сервисов
            // roomScope.Configure(builder => 
            // {
            //     builder.RegisterInstance(roomId).AsSelf();
            //     // Регистрация других сервисов для комнаты
            // });
            
            // Запускаем Scope с родительским контейнером
            roomScope.Build();
            
            ProjectLogger.Log($"[ShelterRoomLifecycle] Room scope created for {roomId}");
            return roomScope;
        }
    }
}


