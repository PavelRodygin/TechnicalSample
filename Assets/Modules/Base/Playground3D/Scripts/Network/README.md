# Playground3D Network Integration

This package provides integration between Mirror Networking and the Playground3D module's PlayerFactory system with VContainer dependency injection.

## Overview

The solution allows dynamic player creation in multiplayer scenarios while maintaining the benefits of dependency injection and the modular architecture.

## Components

### 1. `Playground3DNetworkManager`
Custom NetworkManager that overrides `OnServerAddPlayer` to use PlayerFactory instead of simple prefab instantiation.

**Features:**
- Automatic VContainer dependency injection
- Fallback mechanisms for robust operation
- Multiple injection strategies (automatic and manual)
- Comprehensive logging for debugging

### 2. `INetworkPlayerSpawner`
Interface defining the contract for network player spawning strategies.

### 3. `PlayerFactoryNetworkSpawner`
Implementation of `INetworkPlayerSpawner` that uses PlayerFactory for player creation.

## Setup Instructions

### 1. Scene Setup
1. Add `Playground3DNetworkManager` component to a GameObject in your scene
2. Configure the NetworkManager settings as usual (transport, prefabs, etc.)
3. Set the `fallbackPlayerPrefab` field as a backup option

### 2. Module Installer Configuration
The `Playground3DModuleInstaller` automatically registers:
- `IPlayerFactory` - for creating players with DI
- `INetworkPlayerSpawner` - strategy for network player spawning
- `Playground3DNetworkManager` - if assigned in inspector

### 3. Player Prefab Requirements
Ensure your player prefab has:
- `NetworkIdentity` component (required by Mirror)
- `Player` component (expected by PlayerFactory)
- Any other components that need dependency injection

## How It Works

1. **Client Connection**: When a client connects, Mirror calls `OnServerAddPlayer`
2. **Dependency Injection**: NetworkManager attempts to inject dependencies
3. **Player Spawning**: Uses the following priority:
   - `INetworkPlayerSpawner` (primary strategy)
   - `IPlayerFactory` (fallback)
   - Standard Mirror instantiation (final fallback)
4. **Network Registration**: Adds the created player to the network connection

## Alternative Approaches

If you prefer not to override NetworkManager, consider these alternatives:

### 1. Custom Player Spawning Service
```csharp
public class NetworkPlayerService
{
    [Inject] private IPlayerFactory _playerFactory;
    
    public void SpawnPlayerForConnection(NetworkConnectionToClient conn)
    {
        // Custom spawning logic
    }
}
```

### 2. Event-Based Integration
```csharp
public class PlayerSpawnHandler
{
    [Inject] private IPlayerFactory _playerFactory;
    
    void Start()
    {
        NetworkServer.RegisterHandler<AddPlayerMessage>(OnAddPlayer);
    }
    
    private void OnAddPlayer(NetworkConnectionToClient conn, AddPlayerMessage msg)
    {
        // Handle player spawning with factory
    }
}
```

### 3. NetworkBehaviour Component
```csharp
public class NetworkPlayerFactorySpawner : NetworkBehaviour
{
    [Inject] private IPlayerFactory _playerFactory;
    
    [Command]
    private void CmdSpawnPlayer()
    {
        // Spawn player using factory
    }
}
```

## Benefits of This Approach

1. **Dependency Injection**: Players are created with all dependencies properly injected
2. **Modular Design**: Follows the project's modular architecture
3. **Flexibility**: Multiple fallback strategies ensure robust operation
4. **Testability**: Can easily mock PlayerFactory for testing
5. **Maintainability**: Clear separation of concerns

## Best Practices

1. **Always configure fallback prefab** for development safety
2. **Test dependency injection** in both editor and build
3. **Monitor logs** for injection success/failure
4. **Consider performance** - DI has slight overhead but provides significant benefits
5. **Use interfaces** for maximum flexibility and testability

## Troubleshooting

- **"Dependencies not injected"**: Ensure LifetimeScope exists in scene
- **"Player has no NetworkIdentity"**: Add NetworkIdentity to player prefab
- **"PlayerFactory is null"**: Check module installer registration
- **"Fallback prefab error"**: Verify fallback prefab is assigned and valid
