# TicTacCellView Stateless State Machine

## Overview
The `TicTacCellView` now uses the Stateless library to implement a simple state machine that manages the three possible states of each cell in the TicTac game.

## States

### 1. Empty
- **Description**: Default state when the cell is not filled
- **Visual**: White background, no text, button is interactable
- **Transitions**: Can transition to `X` or `O`

### 2. X
- **Description**: Cell contains an X
- **Visual**: White background, shows X text, button is not interactable
- **Transitions**: Can transition back to `Empty` (reset)

### 3. O
- **Description**: Cell contains an O
- **Visual**: White background, shows O text, button is not interactable
- **Transitions**: Can transition back to `Empty` (reset)

## Triggers

### SetX
- **Effect**: Transitions from `Empty` to `X`
- **Usage**: Called when `SetText('X')` is called

### SetO
- **Effect**: Transitions from `Empty` to `O`
- **Usage**: Called when `SetText('O')` is called

### Reset
- **Effect**: Transitions from `X` or `O` to `Empty`
- **Usage**: Called when `SetText('\0')` is called

## State Machine Configuration

The state machine is configured in the `InitializeStateMachine()` method:

```csharp
_stateMachine.Configure(CellState.Empty)
    .OnEntry(() => OnEmptyStateEntered())
    .Permit(CellTrigger.SetX, CellState.X)
    .Permit(CellTrigger.SetO, CellState.O);
```

Each state has:
- **OnEntry actions**: Visual updates when entering the state
- **Permitted transitions**: Which triggers can cause state changes

## Additional Features

### Winning Highlight
- **Method**: `SetWinningHighlight(bool isWinning)`
- **Effect**: Changes background color to green when winning
- **Usage**: Called when the cell is part of a winning combination

### Blocking
- **Method**: `SetBlocked(bool isBlocked)`
- **Effect**: Disables button interaction and changes background to gray
- **Usage**: Called during game over or when cells should be disabled

## Benefits

1. **Simple and Clear**: Only 3 states that directly correspond to game logic
2. **Visual Feedback**: Different states provide immediate visual feedback to players
3. **Consistent Behavior**: State transitions ensure consistent behavior across all cells
4. **Easy Testing**: The Editor script allows easy testing of all state transitions
5. **Maintainable Code**: State logic is centralized and easy to modify

## Usage Examples

### Basic Cell Interaction
```csharp
// Set cell to X
cellView.SetText('X');  // Transitions to X state

// Set cell to O
cellView.SetText('O');  // Transitions to O state

// Reset the cell
cellView.SetText('\0');  // Transitions back to Empty state
```

### Visual Effects
```csharp
// Highlight winning cell
cellView.SetWinningHighlight(true);

// Block cell during game over
cellView.SetBlocked(true);

// Unblock cell for new game
cellView.SetBlocked(false);
```

### Game Flow Integration
```csharp
// When a player wins, highlight winning cells
var winningPositions = model.GetWinningPositions();
view.MarkWinningCells(winningPositions);

// Block all cells during game over
view.BlockBoard();

// Reset for new game
view.ClearBoard();
view.UnblockBoard();
```

## Testing

Use the `TicTacCellViewSimpleTester` Editor script to test all state transitions:
1. Select a TicTacCellView in the scene
2. Use the Inspector buttons to trigger state changes
3. Observe the visual changes and state transitions
4. Test visual effects like winning highlight and blocking

## Dependencies

- **Stateless**: State machine library
- **R3**: Reactive programming library
- **Unity UI**: Button and Image components (optional for visual feedback)
- **TextMeshPro**: Text display
