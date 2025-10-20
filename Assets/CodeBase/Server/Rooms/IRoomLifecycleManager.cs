using Cysharp.Threading.Tasks;

namespace CodeBase.Server.Rooms
{
    /// <summary>
    /// Интерфейс для управления жизненным циклом комнат
    /// </summary>
    public interface IRoomLifecycleManager
    {
        /// <summary>
        /// Создает и инициализирует комнату
        /// </summary>
        UniTask<RoomInstance> CreateRoomAsync(string roomId);
        
        /// <summary>
        /// Удаляет комнату и освобождает ресурсы
        /// </summary>
        UniTask DestroyRoomAsync(string roomId);
        
        /// <summary>
        /// Подключает игрока к комнате
        /// </summary>
        UniTask<bool> ConnectPlayerToRoomAsync(string roomId, uint playerId);
        
        /// <summary>
        /// Отключает игрока от комнаты
        /// </summary>
        UniTask<bool> DisconnectPlayerFromRoomAsync(string roomId, uint playerId);
        
        /// <summary>
        /// Получает экземпляр комнаты
        /// </summary>
        RoomInstance GetRoom(string roomId);
    }
}


