namespace CodeBase.Server
{
    public class ServerRoomInfo
    {
        public string RoomId { get; set; }
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsAvailable => CurrentPlayers < MaxPlayers;
        
        public ServerRoomInfo(string roomId, int maxPlayers)
        {
            RoomId = roomId;
            MaxPlayers = maxPlayers;
            CurrentPlayers = 0;
        }
    }
}
