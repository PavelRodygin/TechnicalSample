# TicTac Module - Refactored Architecture

## Overview

Модуль TicTac был полностью рефакторен для соответствия лучшим практикам, используемым в модулях MainMenu и Converter. Теперь модуль следует единому архитектурному паттерну проекта.

## Key Improvements

### 1. **Unified MVP Architecture**
- Заменены множественные презентеры на единый `TicTacScreenPresenter`
- Следует тому же паттерну, что и `MainMenuPresenter` и `ConverterScreenPresenter`
- Централизованное управление всеми состояниями игры

### 2. **Commands Structure**
- Введена структура `TicTacCommands` аналогичная `MainMenuCommands` и `ConverterCommands`
- Все команды теперь организованы в единой структуре
- Упрощенная передача команд между компонентами

### 3. **Proper R3 Usage**
- Все Views теперь используют `OnClickAsObservable()` вместо прямых `onClick.AddListener`
- Добавлены проверки `IsActive` для предотвращения случайных кликов
- Автоматическая очистка подписок с помощью `AddTo(this)`

### 4. **Throttling Protection**
- Добавлена защита от спама кликов с помощью `ThrottleFirst`
- Настраиваемые задержки в модели (`CommandThrottleDelay`, `ModuleTransitionThrottleDelay`)
- Следует паттерну других модулей

### 5. **Simplified Controller**
- `TicTacModuleController` теперь следует тому же паттерну, что и другие контроллеры
- Упрощенная логика: только управление жизненным циклом модуля
- Делегирование всей логики презентации в `TicTacScreenPresenter`

### 6. **Enhanced Cell View**
- `TicTacCellView` улучшен для корректного использования R3 паттернов
- Добавлена фильтрация кликов на уровне подписки
- Убраны избыточные проверки в обработчиках

## Architecture Components

### TicTacModel
```csharp
public class TicTacModel : IModel
{
    // Throttle delays for anti-spam protection
    public int CommandThrottleDelay { get; } = 300;
    public int ModuleTransitionThrottleDelay { get; } = 500;
    
    // State machine and game logic
    // ...
}
```

### TicTacCommands
```csharp
public readonly struct TicTacCommands
{
    public readonly ReactiveCommand<Unit> StartGameCommand;
    public readonly ReactiveCommand<Unit> RestartCommand;
    public readonly ReactiveCommand<Unit> ExitCommand;
    public readonly ReactiveCommand<int[]> CellClickCommand;
}
```

### TicTacScreenPresenter
- Единый презентер для всех состояний игры
- Управляет переходами между состояниями
- Подписывается на команды с throttling
- Координирует работу всех View компонентов

### TicTacModuleController
- Упрощенный контроллер по образцу MainMenu/Converter
- Управляет жизненным циклом модуля
- Делегирует презентационную логику в TicTacScreenPresenter

## Views Improvements

### Unified Event Handling
Все View теперь используют единый метод подписки:
```csharp
public void SetupEventListeners(TicTacCommands commands)
{
    if (button != null)
        button.OnClickAsObservable()
            .Where(_ => IsActive)
            .Subscribe(_ => commands.SomeCommand.Execute(Unit.Default))
            .AddTo(this);
}
```

### Automatic Cleanup
- Подписки автоматически очищаются при уничтожении View
- Использование `AddTo(this)` для автоматической утилизации
- Упрощенные методы `Dispose()`

## State Management

Модуль по-прежнему использует Stateless library для управления состояниями:
- **Tutorial**: Отображение правил игры
- **Game**: Активная игра
- **Result**: Отображение результата (победа/ничья)

Но теперь все переходы между состояниями управляются единым презентером.

## Benefits

1. **Consistency**: Единообразие с другими модулями проекта
2. **Maintainability**: Упрощенная архитектура, легче поддерживать
3. **Performance**: Оптимизированные подписки и throttling
4. **Reliability**: Защита от спама кликов и некорректных состояний
5. **Readability**: Чистый и понятный код

## Migration Notes

При переходе на новую архитектуру:
- Удалены старые презентеры состояний
- Обновлен Installer для регистрации только необходимых компонентов
- Все Views адаптированы под новую структуру команд

Модуль теперь полностью соответствует архитектурным стандартам проекта ShrimpOlympus.
