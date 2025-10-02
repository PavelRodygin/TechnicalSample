# TechnicalSample

A technical repository with a Unity project containing my main architectural template for creating applications and games.

## 🏗️ Architecture

The project is built on a modular architecture, where each module is independent, testable, and isolated. The module architecture follows the MVP (Model-View-Presenter) pattern, with the ability to add sub-states to the module using the .NET Stateless library, which is perfect for this task. The modularity of the architecture supports powerful SceneManagement and comprehensive work with DI (VContainer is used in the project).

### Core Principles:
- **Modularity**: Each module is encapsulated and independent
- **MVP Pattern**: Separation of logic into Model, View, and Presenter
- **Dependency Injection**: Using VContainer for dependency management
- **Asynchrony**: UniTask support for asynchronous operations
- **State Management**: Optional Stateless FSM for complex state coordination

### Architectural Patterns:
- **Standard MVP**: Single Model-View-Presenter per module (MainMenu, Converter)
- **State-Based MVP**: Multiple MVP triads with shared Model (TicTac)
- **Module Controller**: Coordinates state transitions and module lifecycle
- **Reactive Programming**: R3 integration for event-driven architecture

## 🛠️ Development Tools

### Module Creator
Built-in tool for automatically creating new modules:
- **Access**: `Tools > Create Module` in Unity Editor
- **Capabilities**:
  - Automatic folder structure creation
  - Generation of basic scripts (Installer, Presenter, View, Model)
  - Creation of Assembly Definition files
  - Generation of scenes and prefabs
  - Module type selection (Base, Additional, Test)

### Module Types:
- **Base**: Core application modules
- **Additional**: Additional modules
- **Test**: Test modules for debugging

## 📦 Existing Modules

### Base Modules
- **TicTac**: Mini-game "Tic-tac-toe" with **advanced state-based MVP architecture**
  - Multiple MVP triads coordinated by Module Controller
  - Stateless FSM demonstration with 3 states: Tutorial → Game → Result
  - Educational example of complex state management patterns
  - R3 reactive programming integration
- **Playground3D**: 3D multiplayer sample module demonstrating **Mirror networking integration** *(in development)*
  - Custom NetworkManager integrated with VContainer DI
  - PlayerFactory system for dynamic player spawning with dependency injection
  - Network player synchronization with SyncVar attributes
  - 3D character controller with animations (idle, walk, run, jump)
  - Educational example of multiplayer architecture patterns
- **Converter**: Data conversion utility with unified MVP pattern
- **MainMenu**: Main application menu with standard MVP architecture
- **Bootstrap**: Game launch screen

### Test Modules
- **PopupsTester**: Test module for demonstrating the popup system
  - Implements PopupHub for modal window management
  - Contains test buttons for various scenarios
  - Demonstrates work with R3 (Reactive Extensions)

## 🏛️ Project Structure

```
Assets/
├── CodeBase/                 # Main codebase
│   ├── Core/                # System core
│   │   ├── Patterns/        # Architectural patterns
│   │   │   └── MVP/         # MVP interfaces
│   │   ├── Systems/         # System components
│   │   │   └── PopupHub/    # Popup management system
│   │   └── UI/              # UI components
│   ├── Services/            # Application services
│   ├── Editor/              # Editor tools
│   │   └── ModuleCreator/   # Module creator
│   └── Tests/               # Tests
├── Modules/                  # Application modules
│   ├── Base/                # Base modules
│   ├── Additional/          # Additional modules
│   └── Test/                # Test modules
└── Resources/                # Resources
```

## 🚀 Quick Start

1. **Clone the repository**
2. **Open the project in Unity** (recommended Unity 2022.3 LTS or newer)
3. **Create a new module**:
   - In Unity Editor: `Tools > Create Module`
   - Choose module name
   - Select module type
   - Configure components to create
   - Click "Create Module"

## 📋 Requirements

- Unity 2022.3 LTS or newer
- .NET 4.x
- Supported platforms: Windows, macOS, Linux

## 🔧 Technologies

- **Unity**
- **UniTask**: Asynchronous operations
- **R3**: Reactive Extensions for Unity
<<<<<<< Updated upstream
- **VContainer**: Dependency Injection container
- **DOTween**: Animations
- **Stateless**: State management (optional)
=======
- **Stateless**: Advanced state machine library for complex state management
>>>>>>> Stashed changes

## ⚡ Reactive Programming with R3

The repository extensively uses **R3 (Reactive Extensions for Unity)** for reactive programming patterns:

### Key Features:
- **Observable Streams**: Event-driven architecture with reactive data flows
- **UI Binding**: Automatic UI updates based on data changes
- **Event Handling**: Reactive event processing and composition
- **Memory Management**: Automatic subscription cleanup with `AddTo()` pattern

### Usage Examples:
```csharp
// Reactive button clicks with automatic cleanup
button.OnClickAsObservable()
    .Subscribe(_ => action.Invoke())
    .AddTo(this);

// Reactive data binding
dataStream
    .Where(x => x.IsValid)
    .Subscribe(UpdateUI)
    .AddTo(this);
```

### Benefits:
- **Declarative Code**: Clear data flow and event handling
- **Automatic Cleanup**: Prevents memory leaks with `AddTo()` pattern
- **Composition**: Easy combination of multiple event streams
- **Performance**: Efficient event processing and UI updates

## 🔄 State Management with Stateless

The project demonstrates advanced state management patterns using **Stateless FSM library**:

### TicTac Module Example:
```
Tutorial ↻ InitializeTutorial
    ↓ StartGame
  Game
    ↓ PlayerWon/GameDraw
 Result
    ↓ Restart (→ Game) | Exit (→ Tutorial)
```

### Key Patterns:
- **Multiple MVP States**: Each state has its own Model-View-Presenter triad
- **Shared Model**: Common data model across all state MVPs
- **Module Controller**: Coordinates state transitions without being part of MVP
- **Stateless Features**: `PermitReentry()`, `Ignore()`, `Permit()` for robust state control

### Benefits:
- **Educational Value**: Demonstrates complex state management patterns
- **Separation of Concerns**: Each state is independently managed
- **Robust Transitions**: Validated state changes with clear error handling
- **Scalability**: Easy to add new states or modify existing ones

## 📝 Features

- **Not a game**: This is a technical repository for development and testing
- **Mini-projects**: Each module represents a separate mini-project
- **Popup system**: Built-in modal window management system
- **Testing**: Support for creating test modules for debugging

## 🤝 Contributing

1. Fork the repository
2. Create a branch for new feature
3. Make changes
4. Create Pull Request

## 📄 License

MIT License

---
