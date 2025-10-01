# TicTac Module - Proper R3 and State-Based MVP Architecture

## Overview

Модуль TicTac был правильно улучшен с применением лучших практик R3 и throttling, **сохранив оригинальную архитектуру с отдельными MVP состояниями** для демонстрации работы со Stateless library. Архитектура использует **Controller как координатор состояний**, который переключает между различными MVP триадами, разделяющими общую модель.

## Key Improvements Applied

### 1. **R3 Integration**
- Все Views используют `OnClickAsObservable()` вместо прямых `onClick.AddListener`
- Добавлены проверки `IsActive` для предотвращения случайных кликов
- Автоматическая очистка подписок с помощью `AddTo(this)`
- Использование `ReactiveCommand<T>` для команд

### 2. **Throttling Protection**
- Добавлена защита от спама кликов на уровне контроллера
- Настраиваемые задержки в модели:
  - `CommandThrottleDelay = 300ms` для обычных команд
  - `ModuleTransitionThrottleDelay = 500ms` для переходов между модулями
- `ThrottleFirst()` применен ко всем командам в контроллере

### 3. **Commands Structure**
- Введена структура `TicTacCommands` для унификации с другими модулями
- Централизованная передача команд между компонентами
- Типизированные команды для разных действий

### 4. **Enhanced Cell View**
- Улучшенная фильтрация кликов на уровне подписки R3
- Сохранена логика state machine для демонстрации Stateless
- Убраны избыточные проверки в обработчиках

5. **Proper State Coordination**
- Controller служит координатором между MVP состояниями, а не частью MVP
- Все взаимодействия с View происходят через соответствующие Presenters
- Соблюдены принципы разделения ответственности

## Architecture Design

### State-Based MVP Pattern
Модуль использует **множественные MVP триады**, координируемые Controller'ом:

1. **Tutorial MVP**:
   - Model: `TicTacModel` (общая)
   - View: `TicTacTutorialStateView`
   - Presenter: `TicTacTutorialStatePresenter`

2. **Game MVP**:
   - Model: `TicTacModel` (общая)
   - View: `TicTacGameStateView`
   - Presenter: `TicTacGameStatePresenter`

3. **Result MVP**:
   - Model: `TicTacModel` (общая)
   - View: `TicTacGameResultStateView`
   - Presenter: `TicTacGameResultStatePresenter`

### Module Controller Role
`TicTacModuleController` **НЕ является частью MVP паттерна**. Его роль:
- Координировать переключения между MVP состояниями **через Stateless FSM**
- Управлять жизненным циклом модуля
- Обрабатывать команды состояний и вызывать переходы в State Machine
- Содержать бизнес-логику игры (ходы, проверка победы)
- **Никогда не обращаться к презентерам напрямую** - только через модель

Это сделано для демонстрации возможностей Stateless library и правильного разделения ответственности между состояниями.

### State Machine Flow
```
InitializeTutorial ↻ Tutorial → Game → Result
                                 ↑       ↓
                                 ←-------←
```

**Stateless Patterns Used:**
- `PermitReentry(InitializeTutorial)` - для правильной инициализации Tutorial состояния
- `Ignore(trigger)` - игнорирование неприменимых triggers в каждом состоянии
- `Permit(trigger, state)` - для валидных переходов между состояниями

**Robust State Configuration:**
- **Tutorial**: Игнорирует game triggers (PlayerWon, GameDraw, Restart, Exit)
- **Game**: Игнорирует tutorial triggers (InitializeTutorial, Exit)  
- **Result**: Игнорирует game triggers (PlayerWon, GameDraw, InitializeTutorial)

### Complete Architecture Diagram
```
TicTacModuleController (State Coordinator)
        ↓ coordinates
    ┌─────────────────────────────────────┐
    │           TicTacModel               │
    │      (Shared by all states)         │
    │   - Game data + Stateless FSM      │
    └─────────────────────────────────────┘
        ↓ shared by        ↓ shared by        ↓ shared by
    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
    │ Tutorial MVP│    │  Game MVP   │    │ Result MVP  │
    │             │    │             │    │             │
    │ [M] Model ←─┼────┼── Model ←───┼────┼── Model     │
    │ [V] TutorialView│ │ [V] GameView│    │ [V] ResultView│
    │ [P] TutorialPres│ │ [P] GamePres│    │ [P] ResultPres│
    └─────────────┘    └─────────────┘    └─────────────┘
```

## Implementation Details

### TicTacCommands Structure
```csharp
public readonly struct TicTacCommands
{
    public readonly ReactiveCommand<Unit> StartGameCommand;  // Tutorial → Game
    public readonly ReactiveCommand<Unit> RestartCommand;    // Result → Game
    public readonly ReactiveCommand<Unit> ExitCommand;       // Any → MainMenu
    public readonly ReactiveCommand<int[]> CellClickCommand; // Game cell clicks
}
```

### Throttling Implementation
В контроллере применено throttling на уровне подписок:
```csharp
_tutorialStatePresenter.ContinueCommand
    .ThrottleFirst(TimeSpan.FromMilliseconds(_moduleModel.ModuleTransitionThrottleDelay))
    .Subscribe(_ => OnContinueButtonClicked())
    .AddTo(_disposables);
```

### View Improvements
Все Views теперь используют унифицированный подход:
```csharp
public void SetupEventListeners(TicTacCommands commands)
{
    button.OnClickAsObservable()
        .Where(_ => IsActive)
        .Subscribe(_ => commands.SomeCommand.Execute(Unit.Default))
        .AddTo(this);
}
```

## Benefits of This Approach

### ✅ Preserved Architecture Benefits
1. **Stateless Demonstration**: Сохранена оригинальная цель - показать работу с состояниями
2. **Separation of Concerns**: Каждый presenter отвечает за свое состояние
3. **Educational Value**: Модуль остается примером правильного использования Stateless

### ✅ Applied Improvements
1. **R3 Best Practices**: Правильное использование реактивных команд
2. **Performance**: Throttling защита от спама
3. **Consistency**: Унификация с паттернами проекта
4. **Maintainability**: Чистые подписки и автоматическая очистка

## Key Differences from MainMenu/Converter

В отличие от MainMenu и Converter, которые используют единый презентер, TicTac модуль **намеренно использует множественные state presenters** для:

1. **Демонстрации Stateless**: Показать как состояния могут управляться отдельными компонентами
2. **Образовательных целей**: Примером того, как можно организовать сложные state machines
3. **Гибкости**: Каждое состояние может иметь свою уникальную логику

## Migration Summary

### What Was Changed ✅
- R3 integration в Views
- Throttling protection в Controller
- Commands structure для унификации
- Enhanced TicTacCellView
- **Proper State Coordination**: Controller работает только через State Machine, не напрямую с Presenters
- **Added InitializeTutorial trigger**: Правильная инициализация через FSM

### What Was Preserved ✅
- Separate state presenters
- Stateless state machine
- Educational architecture
- State-specific logic separation

### Architecture Pattern Compliance ✅
- **Module Controller** → координатор состояний (НЕ часть MVP)
- **State Presenters** → управляют своими Views в рамках каждого MVP состояния
- **State Views** → только отображение, никакой логики
- **Shared Model** → общая модель для всех MVP состояний (данные + state machine)

### Key Architecture Benefits ✅
1. **Separation of Concerns**: Каждое состояние имеет свою MVP триаду
2. **Shared State**: Общая модель обеспечивает консистентность данных
3. **Coordinated Transitions**: Controller управляет переходами через Stateless FSM
4. **Educational Value**: Демонстрирует продвинутые паттерны состояний

Модуль теперь сочетает лучшие практики R3/throttling с правильной state-based MVP архитектурой и оригинальной концепцией для демонстрации Stateless library.
