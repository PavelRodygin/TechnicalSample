# Room Lifecycle Management System

Система управления жизненным циклом комнат на игровом сервере.

## Архитектура

### Интерфейсы

#### `IRoomLifecycleManager`
Основной интерфейс для управления жизненным циклом комнат:
- `CreateRoomAsync(roomId)` - создание и инициализация комнаты
- `DestroyRoomAsync(roomId)` - удаление комнаты и освобождение ресурсов
- `ConnectPlayerToRoomAsync(roomId, playerId)` - подключение игрока к комнате
- `DisconnectPlayerFromRoomAsync(roomId, playerId)` - отключение игрока от комнаты
- `GetRoom(roomId)` - получение экземпляра комнаты

### Классы

#### `RoomInstance`
Представляет экземпляр комнаты:
- `RoomId` - уникальный идентификатор комнаты
- `Scene` - загруженная аддитивная сцена (для Shelter)
- `RoomScope` - VContainer LifetimeScope для изоляции сервисов
- `PlayerIds` - список подключенных игроков
- `MaxPlayers` - максимальное количество игроков

#### `ShelterRoomLifecycleManager`
Управление комнатами убежища:
- ✅ Загружает **аддитивную сцену** для каждой комнаты
- ✅ Создает **отдельный Scope** для изоляции сервисов
- ✅ Обеспечивает **физическую изоляцию** игроков через отдельные сцены
- ✅ **Автоматически удаляет** пустые комнаты

#### `BattleRoomLifecycleManager`
Управление боевыми комнатами:
- ✅ Все игроки находятся в **одной сцене** (BattleScene)
- ✅ Нет физической изоляции - все игроки видят друг друга
- ✅ Комнаты **не удаляются** автоматически при опустошении
- ✅ Переиспользование комнат

## Регистрация

Регистрация происходит в `ServerStateInstaller` в зависимости от типа сервера (`ServerType`):

```csharp
case ServerType.Shelter:
    builder.Register<ShelterRoomLifecycleManager>()
        .AsImplementedInterfaces()
        .AsSelf();
    break;
    
case ServerType.BattleRoom:
    builder.Register<BattleRoomLifecycleManager>()
        .AsImplementedInterfaces()
        .AsSelf();
    break;
```

## Использование

### Создание комнаты

```csharp
[Inject] private IRoomLifecycleManager _roomLifecycle;

async UniTask CreateRoom()
{
    var roomInstance = await _roomLifecycle.CreateRoomAsync("room-123");
    if (roomInstance != null)
    {
        Debug.Log($"Room created: {roomInstance.RoomId}");
    }
}
```

### Подключение игрока

```csharp
async UniTask ConnectPlayer(string roomId, uint playerId)
{
    bool success = await _roomLifecycle.ConnectPlayerToRoomAsync(roomId, playerId);
    if (success)
    {
        Debug.Log($"Player {playerId} connected to room {roomId}");
    }
}
```

### Получение информации о комнате

```csharp
var room = _roomLifecycle.GetRoom("room-123");
if (room != null)
{
    Debug.Log($"Room {room.RoomId} has {room.PlayerIds.Count}/{room.MaxPlayers} players");
}
```

## Различия между типами серверов

| Aspect | Shelter (Убежище) | BattleRoom (Бой) |
|--------|------------------|------------------|
| **Сцены** | Отдельная аддитивная сцена для каждой комнаты | Все в одной BattleScene |
| **Изоляция** | Физическая изоляция через разные сцены | Нет изоляции |
| **Scope** | Отдельный LifetimeScope для каждой комнаты | Общий Scope |
| **Удаление** | Автоматическое при опустошении | Ручное управление |
| **Использование** | Хаб, социальная зона | PvE/PvP зона |

## TODO

- [ ] Реализовать перемещение GameObject игрока в сцену комнаты
- [ ] Настроить билдер Scope для регистрации специфичных сервисов
- [ ] Добавить PhysicsScene изоляцию для Shelter комнат
- [ ] Реализовать переиспользование комнат для BattleRoom
- [ ] Добавить метрики и мониторинг комнат


