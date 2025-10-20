using System.Collections.Generic;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace CodeBase.Server.Rooms
{
    /// <summary>
    /// Экземпляр комнаты с загруженной сценой и scope
    /// </summary>
    public class RoomInstance
    {
        public string RoomId { get; }
        public Scene Scene { get; }
        public LifetimeScope RoomScope { get; }
        public HashSet<uint> PlayerIds { get; }
        public int MaxPlayers { get; }
        
        public bool IsFull => PlayerIds.Count >= MaxPlayers;
        public bool IsEmpty => PlayerIds.Count == 0;
        
        public RoomInstance(string roomId, Scene scene, LifetimeScope roomScope, int maxPlayers)
        {
            RoomId = roomId;
            Scene = scene;
            RoomScope = roomScope;
            MaxPlayers = maxPlayers;
            PlayerIds = new HashSet<uint>();
        }
        
        public bool AddPlayer(uint playerId)
        {
            if (IsFull)
                return false;
                
            return PlayerIds.Add(playerId);
        }
        
        public bool RemovePlayer(uint playerId)
        {
            return PlayerIds.Remove(playerId);
        }
    }
}


