# GameManager & Player Factory Setup Instructions

## Summary

Реализована архитектура с разделением ответственности:
- **ModuleController** - управляет жизненным циклом модуля
- **GameManager** - управляет игровой логикой и спавном игроков
- **PlayerFactory** - создает игроков с VContainer dependency injection

## Completed Implementation

### 1. Enhanced GameManager System
- **Spawn Points Management** - система точек спавна игроков
- **Player Management** - отслеживание активных игроков
- **Game Lifecycle** - методы StartGame()/EndGame()
- **Validation** - проверка конфигурации при запуске

### 2. Improved Module Controller
- `Playground3DModuleController` делегирует игровую логику `GameManager`
- При `Enter()` вызывается `GameManager.StartGame()`
- При `Exit()` вызывается `GameManager.EndGame()`
- Убрана логика создания игроков из контроллера

### 3. Updated Installer
- Зарегистрирован `GameManager` как компонент сцены
- `PlayerFactory` остается в VContainer
- Добавлено поле для GameManager в inspector

## Required Setup in Unity

### 1. Setup GameManager in Scene
1. Откройте сцену `ThirdPersonMP.unity`
2. Создайте GameObject с именем "GameManager"
3. Добавьте компонент `GameManager`
4. В inspector настройте:
   - **Game World Transform** - корневой transform игрового мира
   - **Player Spawn Points** - массив transform-ов для точек спавна
   - **Spawn Player On Start** - включить для автоматического спавна

### 2. Configure Installer
1. Найдите GameObject с `Playground3DModuleInstaller` компонентом
2. В inspector назначьте:
   - **Player Prefab** - `Assets/Modules/Base/Playground3D/Prefabs/Player.prefab`
   - **Game Manager** - созданный GameManager из сцены

### 3. Setup Spawn Points
1. Создайте пустые GameObject-ы для точек спавна
2. Расположите их в нужных местах на карте
3. Добавьте их в массив **Player Spawn Points** в GameManager

## Testing

1. Запустите сцену `ThirdPersonMP.unity`
2. Перейдите в модуль Playground3D через главное меню
3. GameManager автоматически создаст игрока в первой точке спавна
4. Проверьте Console на сообщения:
   - "Game started successfully!"
   - "Player spawned successfully at spawn point X"
5. Игрок должен реагировать на управление (WASD, пробел для прыжка)

## GameManager Features

### Public Methods
- `SpawnPlayer()` - создать нового игрока в следующей точке спавна
- `RemovePlayer(GameObject)` - удалить конкретного игрока
- `DestroyAllPlayers()` - удалить всех игроков
- `StartGame()` / `EndGame()` - управление жизненным циклом игры

### Public Properties
- `ActivePlayers` - список активных игроков (read-only)
- `HasActivePlayers` - есть ли активные игроки
- `GameWorldTransform` - корневой transform игрового мира

### Editor Features
- Gizmos для визуализации точек спавна в Scene View
- Автоматическая валидация настроек при запуске

## Architecture Benefits

- **Separation of Concerns**: ModuleController управляет модулем, GameManager - игрой
- **Scalable**: Легко добавлять новых игроков и функциональность
- **Testable**: GameManager можно тестировать независимо
- **Configurable**: Все настройки через inspector
- **Future-Ready**: Готов для мультиплеера и расширений

## Troubleshooting

**Проблема**: "No spawn points configured"  
**Решение**: Добавьте Transform-ы в массив Player Spawn Points в GameManager

**Проблема**: "GameWorldTransform is not set"  
**Решение**: Назначьте Game World Transform в GameManager

**Проблема**: GameManager не зарегистрирован  
**Решение**: Проверьте, что GameManager назначен в Playground3DModuleInstaller

**Проблема**: Игрок не создается  
**Решение**: Проверьте настройки Player Prefab и точек спавна
