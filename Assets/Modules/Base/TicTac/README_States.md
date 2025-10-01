# TicTac Module State Machine Implementation

This module now uses the Stateless library to implement a state machine pattern, similar to the GarageModule.

## States

The module has four main states:

1. **Initial** - The starting state when the module is first loaded
2. **Game** - Active gameplay state where players can make moves
3. **Win** - Victory state when a player wins the game
4. **Draw** - Draw state when the game ends in a tie

## State Transitions

- **Initial → Game**: Triggered by `StartGame` when the module enters
- **Game → Win**: Triggered by `PlayerWon` when a player wins
- **Game → Draw**: Triggered by `GameDraw` when the game ends in a tie
- **Win → Game**: Triggered by `Restart` when restarting the game
- **Win → Initial**: Triggered by `Exit` when returning to main menu
- **Draw → Game**: Triggered by `Restart` when restarting the game
- **Draw → Initial**: Triggered by `Exit` when returning to main menu

## Architecture

The module follows the MVP pattern with state-specific presenters:

- **TicTacScreenPresenter**: Main controller that manages the state machine
- **GameStatePresenter**: Handles the active gameplay state
- **WinStatePresenter**: Manages the victory screen
- **DrawStatePresenter**: Manages the draw screen

## Usage

1. The main presenter configures the state machine with callbacks
2. State transitions are triggered by game events (win, draw, restart)
3. Each state presenter handles its own UI and logic
4. The main controller coordinates between states

## Benefits

- Clean separation of concerns between different game states
- Easy to add new states or modify existing ones
- Consistent with the project's architectural patterns
- Better testability and maintainability
