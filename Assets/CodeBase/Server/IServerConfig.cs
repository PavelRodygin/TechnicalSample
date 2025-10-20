namespace CodeBase.Server
{
    public interface IServerConfig
    {
        /// <summary>
        /// Тип сервера (убежище/боевая комната)
        /// </summary>
        ServerType ServerType { get; }
        
        /// <summary>
        /// Максимальная вместимость одной комнаты
        /// </summary>
        int RoomCapacity { get; }
        
        /// <summary>
        /// Максимальное количество комнат на сервере
        /// </summary>
        int MaxRooms { get; }
        
        /// <summary>
        /// Порт сервера
        /// </summary>
        ushort ServerPort { get; }
        
        /// <summary>
        /// ID сервера
        /// </summary>
        string ServerId { get; }
    }
}
