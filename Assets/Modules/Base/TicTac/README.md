### Stateless State Machine
The `TicTacCellView` uses the Stateless library to manage cell states:
- **Empty**: Default state, interactable
- **X**: Contains X, not interactable
- **O**: Contains O, not interactable

## Key Features

1. **Simple State Logic**: Each cell has only 3 possible states (Empty, X, O)
2. **Visual Feedback**: Different states provide clear visual indicators
3. **Reactive Programming**: Uses R3 for event handling
4. **Easy Testing**: Editor tools for testing state transitions
5. **Modular Design**: Clean separation of concerns
