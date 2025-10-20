using System;
using CodeBase.Shared;
using UnityEngine;

namespace CodeBase.Server
{
    public class ServerConfig : IServerConfig
    {
        private const string EnvServerType = "SERVER_TYPE";
        private const string EnvRoomCapacity = "ROOM_CAPACITY";
        private const string EnvMaxRooms = "MAX_ROOMS";
        private const string EnvServerPort = "SERVER_PORT";
        private const string EnvServerID = "SERVER_ID";
        
        private const ServerType DefaultServerType = ServerType.BattleRoom;
        private const int DefaultRoomCapacity = 4;
        private const int DefaultMaxRooms = 1;
        private const ushort DefaultServerPort = 7777;
        private const string DefaultServerID = "default-server";

        public ServerType ServerType { get; private set; }
        public int RoomCapacity { get; private set; }
        public int MaxRooms { get; private set; }
        public ushort ServerPort { get; private set; }
        public string ServerId { get; private set; }

        public ServerConfig() => 
            LoadFromEnvironment();

        private void LoadFromEnvironment()
        {
            LoadServerType();
            LoadRoomCapacity();
            LoadMaxRooms();
            LoadServerPort();
            LoadServerId();
            LogConfiguration();
        }

        private void LoadServerType()
        {
            var serverTypeStr = Environment.GetEnvironmentVariable(EnvServerType);
            if (!string.IsNullOrEmpty(serverTypeStr) &&
                Enum.TryParse<ServerType>(serverTypeStr, true, out var serverType))
                ServerType = serverType;
            else
            {
                ServerType = DefaultServerType;
                Debug.LogWarning($"[ServerConfig] {EnvServerType} not set or invalid, using default: {DefaultServerType}");
            }
        }

        private void LoadRoomCapacity()
        {
            var roomCapacityStr = Environment.GetEnvironmentVariable(EnvRoomCapacity);
            if (!string.IsNullOrEmpty(roomCapacityStr) && int.TryParse(roomCapacityStr, out var roomCapacity))
                RoomCapacity = roomCapacity;
            else
            {
                RoomCapacity = DefaultRoomCapacity;
                Debug.LogWarning($"[ServerConfig] {EnvRoomCapacity} not set or invalid, using default: {DefaultRoomCapacity}");
            }
        }

        private void LoadMaxRooms()
        {
            var maxRoomsStr = Environment.GetEnvironmentVariable(EnvMaxRooms);
            if (!string.IsNullOrEmpty(maxRoomsStr) && int.TryParse(maxRoomsStr, out var maxRooms))
                MaxRooms = maxRooms;
            else
            {
                MaxRooms = DefaultMaxRooms;
                Debug.LogWarning($"[ServerConfig] {EnvMaxRooms} not set or invalid, using default: {DefaultMaxRooms}");
            }
        }

        private void LoadServerPort()
        {
            var serverPortStr = Environment.GetEnvironmentVariable(EnvServerPort);
            if (!string.IsNullOrEmpty(serverPortStr) && ushort.TryParse(serverPortStr, out var serverPort))
                ServerPort = serverPort;
            else
            {
                ServerPort = DefaultServerPort;
                Debug.LogWarning($"[ServerConfig] {EnvServerPort} not set or invalid, using default: {DefaultServerPort}");
            }
        }

        private void LoadServerId()
        {
            var serverId = Environment.GetEnvironmentVariable(EnvServerID);
            if (!string.IsNullOrEmpty(serverId))
                ServerId = serverId;
            else
            {
                ServerId = DefaultServerID;
                Debug.LogWarning($"[ServerConfig] {EnvServerID} not set, using default: {DefaultServerID}");
            }
        }

        private void LogConfiguration()
        {
            ProjectLogger.Log("Server configuration loaded:\n" + 
                              $"  Server Type: {ServerType}\n" + 
                              $"  Room Capacity: {RoomCapacity}\n" + 
                              $"  Max Rooms: {MaxRooms}\n" + 
                              $"  Server Port: {ServerPort}\n" + 
                              $"  Server ID: {ServerId}");
        }
    }
}
