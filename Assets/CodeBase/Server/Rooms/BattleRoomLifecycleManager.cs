using System.Collections.Generic;
using CodeBase.Shared;
using Cysharp.Threading.Tasks;
using VContainer;

namespace CodeBase.Server.Rooms
{
    /// <summary>
    /// Управление жизненным циклом боевых комнат
    /// Для боевых комнат все игроки находятся в одной сцене, без физической изоляции
    /// </summary>
    public class BattleRoomLifecycleManager : IRoomLifecycleManager
    {
        private readonly IServerConfig _serverConfig;
        private readonly Dictionary<string, RoomInstance> _rooms;

        [Inject]
        public BattleRoomLifecycleManager(IServerConfig serverConfig)
        {
            _serverConfig = serverConfig;
            _rooms = new Dictionary<string, RoomInstance>();
        }

        public async UniTask<RoomInstance> CreateRoomAsync(string roomId)
        {
            if (_rooms.ContainsKey(roomId))
            {
                ProjectLogger.LogWarning($"[BattleRoomLifecycle] Room {roomId} already exists");
                return _rooms[roomId];
            }

            // Для боевых комнат не создаем отдельную сцену
            // Все игроки находятся в одной BattleScene
            var roomInstance = new RoomInstance(
                roomId,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
                null,
                _serverConfig.RoomCapacity);

            _rooms.Add(roomId, roomInstance);
            
            ProjectLogger.Log($"[BattleRoomLifecycle] Room {roomId} created");
            
            await UniTask.Yield();
            return roomInstance;
        }

        public async UniTask DestroyRoomAsync(string roomId)
        {
            if (!_rooms.Remove(roomId))
            {
                ProjectLogger.LogWarning($"[BattleRoomLifecycle] Room {roomId} not found");
                return;
            }

            ProjectLogger.Log($"[BattleRoomLifecycle] Room {roomId} destroyed");
            await UniTask.Yield();
        }

        public async UniTask<bool> ConnectPlayerToRoomAsync(string roomId, uint playerId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogError($"[BattleRoomLifecycle] Room {roomId} not found");
                return false;
            }

            if (room.IsFull)
            {
                ProjectLogger.LogWarning($"[BattleRoomLifecycle] Room {roomId} is full");
                return false;
            }

            if (!room.AddPlayer(playerId))
            {
                ProjectLogger.LogWarning($"[BattleRoomLifecycle] Player {playerId} already in room {roomId}");
                return false;
            }

            ProjectLogger.Log($"[BattleRoomLifecycle] Player {playerId} connected to room {roomId} ({room.PlayerIds.Count}/{room.MaxPlayers})");
            
            await UniTask.Yield();
            return true;
        }

        public async UniTask<bool> DisconnectPlayerFromRoomAsync(string roomId, uint playerId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogError($"[BattleRoomLifecycle] Room {roomId} not found");
                return false;
            }

            if (!room.RemovePlayer(playerId))
            {
                ProjectLogger.LogWarning($"[BattleRoomLifecycle] Player {playerId} not in room {roomId}");
                return false;
            }

            ProjectLogger.Log($"[BattleRoomLifecycle] Player {playerId} disconnected from room {roomId} ({room.PlayerIds.Count}/{room.MaxPlayers})");

            // Для боевых комнат не удаляем комнату автоматически при опустошении
            // Она может быть переиспользована
            
            await UniTask.Yield();
            return true;
        }

        public RoomInstance GetRoom(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }
    }
}


