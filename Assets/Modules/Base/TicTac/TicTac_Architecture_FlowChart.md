# TicTac Module Architecture FlowChart

```mermaid
flowchart TD
    %% Module Controller
    Controller[TicTacModuleController<br/>📋 State Coordinator]
    
    %% Shared Model
    Model[TicTacModel<br/>🎯 Shared Model<br/>+ Stateless FSM<br/>+ Game Logic<br/>+ Board State]
    
    %% Tutorial State MVP
    TutorialPresenter[TicTacTutorialStatePresenter<br/>📖 Tutorial Presenter]
    TutorialView[TicTacTutorialStateView<br/>🖼️ Tutorial View]
    
    %% Game State MVP
    GamePresenter[TicTacGameStatePresenter<br/>🎮 Game Presenter]
    GameView[TicTacGameStateView<br/>🖼️ Game View]
    
    %% Result State MVP
    ResultPresenter[TicTacGameResultStatePresenter<br/>🏆 Result Presenter]
    ResultView[TicTacGameResultStateView<br/>🖼️ Result View]
    
    %% Cell Views (sub-components)
    CellView1[TicTacCellView<br/>🔲 Cell 1]
    CellView2[TicTacCellView<br/>🔲 Cell 2]
    CellView3[TicTacCellView<br/>🔲 Cell ...]
    
    %% Controller coordinates State Machine
    Controller -.->|"🔄 Configure FSM<br/>State Transitions"| Model
    
    %% Model provides data to all Presenters
    Model -.->|"📊 Game Data<br/>Board State"| TutorialPresenter
    Model -.->|"📊 Game Data<br/>Board State"| GamePresenter
    Model -.->|"📊 Game Data<br/>Board State"| ResultPresenter
    
    %% MVP Triads
    TutorialPresenter -->|"📝 Update UI"| TutorialView
    TutorialView -->|"🖱️ User Actions"| TutorialPresenter
    
    GamePresenter -->|"📝 Update UI"| GameView
    GameView -->|"🖱️ User Actions"| GamePresenter
    
    ResultPresenter -->|"📝 Update UI"| ResultView
    ResultView -->|"🖱️ User Actions"| ResultPresenter
    
    %% Game View manages Cell Views
    GameView -->|"📋 Manages"| CellView1
    GameView -->|"📋 Manages"| CellView2
    GameView -->|"📋 Manages"| CellView3
    
    %% Cell Views send events to Game View
    CellView1 -.->|"🖱️ Cell Click"| GameView
    CellView2 -.->|"🖱️ Cell Click"| GameView
    CellView3 -.->|"🖱️ Cell Click"| GameView
    
    %% State transitions through Model
    TutorialPresenter -.->|"🔄 StartGame"| Model
    GamePresenter -.->|"🔄 PlayerWon/GameDraw"| Model
    ResultPresenter -.->|"🔄 Restart/Exit"| Model
    
    %% Exit to Main Menu
    TutorialPresenter -.->|"🚪 Exit to MainMenu"| Controller
    GamePresenter -.->|"🚪 Exit to MainMenu"| Controller
    ResultPresenter -.->|"🚪 Exit to MainMenu"| Controller
    
    %% Styling
    classDef controller fill:#e1f5fe,stroke:#01579b,stroke-width:3px
    classDef model fill:#f3e5f5,stroke:#4a148c,stroke-width:3px
    classDef presenter fill:#e8f5e8,stroke:#1b5e20,stroke-width:2px
    classDef view fill:#fff3e0,stroke:#e65100,stroke-width:2px
    classDef cellview fill:#fce4ec,stroke:#880e4f,stroke-width:1px
    
    class Controller controller
    class Model model
    class TutorialPresenter,GamePresenter,ResultPresenter presenter
    class TutorialView,GameView,ResultView view
    class CellView1,CellView2,CellView3 cellview
```

## Architecture Components

### 🎯 **TicTacModel (Shared)**
- Единая модель для всех состояний
- Содержит Stateless FSM
- Управляет игровой логикой и состоянием доски

### 📋 **TicTacModuleController (State Coordinator)**
- Координирует переходы между состояниями
- Настраивает State Machine
- Обрабатывает выход в главное меню

### 📖 **Tutorial MVP State**
- **Presenter**: Управляет логикой tutorial состояния
- **View**: Отображает правила игры и кнопки

### 🎮 **Game MVP State**
- **Presenter**: Управляет игровой логикой и ходами
- **View**: Отображает игровое поле и управляет cell views

### 🏆 **Result MVP State**
- **Presenter**: Управляет отображением результата
- **View**: Показывает победителя или ничью

### 🔲 **Cell Views (Sub-components)**
- Отдельные view компоненты для каждой клетки
- Управляются Game View
- Используют собственные Stateless state machines

## Key Principles

1. **State-Based MVP**: Множественные MVP триады с общей моделью
2. **Centralized Coordination**: Controller координирует через FSM
3. **Shared State**: Общая модель обеспечивает консистентность
4. **Hierarchical Views**: Game View управляет Cell Views
