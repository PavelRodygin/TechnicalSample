using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Server.Rooms;
using CodeBase.Shared;
using VContainer;

namespace CodeBase.Server
{
    /// <summary>
    /// Сервис для управления состоянием сервера и комнат
    /// </summary>
    public class ServerStateService
    {
        private readonly Dictionary<string, ServerRoomInfo> _rooms;
        private readonly IRoomLifecycleManager _roomLifecycle;

        public IServerConfig Config { get; }
        public IRoomLifecycleManager RoomLifecycle => _roomLifecycle;

        public IReadOnlyDictionary<string, ServerRoomInfo> Rooms => _rooms;
        public int TotalPlayers { get; private set; }

        public int AvailableRooms => _rooms.Count(r => r.Value.IsAvailable);
        public bool CanCreateNewRoom => _rooms.Count < Config.MaxRooms;

        [Inject]
        public ServerStateService(IServerConfig config, IRoomLifecycleManager roomLifecycle = null)
        {
            Config = config;
            _roomLifecycle = roomLifecycle;
            _rooms = new Dictionary<string, ServerRoomInfo>();
            TotalPlayers = 0;
            
            ProjectLogger.Log("[ServerStateService] Initialized with config: " +
                            $"Type={Config.ServerType}, " +
                            $"RoomCapacity={Config.RoomCapacity}, " +
                            $"MaxRooms={Config.MaxRooms}, " +
                            $"RoomLifecycle={(_roomLifecycle != null ? "Enabled" : "Disabled")}");
        }
        
        public string CreateRoom()
        {
            if (!CanCreateNewRoom)
            {
                ProjectLogger.LogWarning($"[ServerStateService] Cannot create room: max rooms limit reached ({Config.MaxRooms})");
                return null;
            }

            var roomId = GenerateRoomId();
            var room = new ServerRoomInfo(roomId, Config.RoomCapacity);
            _rooms.Add(roomId, room);
            
            ProjectLogger.Log($"[ServerStateService] Room created: {roomId} (capacity: {Config.RoomCapacity})");
            return roomId;
        }
        
        public string GetOrCreateAvailableRoom()
        {
            var availableRoom = _rooms.Values.FirstOrDefault(r => r.IsAvailable);
            if (availableRoom != null)
            {
                return availableRoom.RoomId;
            }

            if (CanCreateNewRoom)
                return CreateRoom();

            ProjectLogger.LogWarning($"[ServerStateService] No available rooms and cannot create new one");
            return null;
        }
        
        public bool AddPlayerToRoom(string roomId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogError($"[ServerStateService] Room not found: {roomId}");
                return false;
            }

            if (!room.IsAvailable)
            {
                ProjectLogger.LogWarning($"[ServerStateService] Room {roomId} is full");
                return false;
            }

            room.CurrentPlayers++;
            TotalPlayers++;
            
            ProjectLogger.Log($"[ServerStateService] Player added to room {roomId} ({room.CurrentPlayers}/{room.MaxPlayers})");
            return true;
        }
        
        public bool RemovePlayerFromRoom(string roomId)
        {
            if (!_rooms.TryGetValue(roomId, out var room))
            {
                ProjectLogger.LogError($"[ServerStateService] Room not found: {roomId}");
                return false;
            }

            if (room.CurrentPlayers <= 0)
            {
                ProjectLogger.LogWarning($"[ServerStateService] Room {roomId} is already empty");
                return false;
            }

            room.CurrentPlayers--;
            TotalPlayers--;
            
            ProjectLogger.Log($"[ServerStateService] Player removed from room {roomId} ({room.CurrentPlayers}/{room.MaxPlayers})");

            // Удаляем пустую комнату
            if (room.CurrentPlayers == 0)
            {
                RemoveRoom(roomId);
            }

            return true;
        }
        
        public bool RemoveRoom(string roomId)
        {
            if (!_rooms.ContainsKey(roomId))
            {
                return false;
            }

            _rooms.Remove(roomId);
            ProjectLogger.Log($"[ServerStateService] Room removed: {roomId}");
            return true;
        }
        
        public ServerRoomInfo GetRoomInfo(string roomId)
        {
            _rooms.TryGetValue(roomId, out var room);
            return room;
        }
        
        private string GenerateRoomId() => 
            $"{Config.ServerId}-room-{Guid.NewGuid().ToString().Substring(0, 8)}";
        
        public string GetServerStats()
        {
            return $"Server Stats:\n" +
                   $"  Type: {Config.ServerType}\n" +
                   $"  Total Players: {TotalPlayers}\n" +
                   $"  Total Rooms: {_rooms.Count}/{Config.MaxRooms}\n" +
                   $"  Available Rooms: {AvailableRooms}\n" +
                   $"  Room Capacity: {Config.RoomCapacity}";
        }
    }
}
