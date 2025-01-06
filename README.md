# Project Purpose and General Description

This project aims to develop a tile-matching and popping game under the player's control. The primary objective of the game is to align at least three objects of the same color horizontally or vertically to pop them and earn points. To make the game more engaging, joker objects and dynamic game controls have been added. The project has been developed in compliance with object-oriented programming (OOP) principles.

# Fulfillment of Project Requirements

Below is a detailed explanation of how the specified requirements are met:

## Menu Screen

The menu screen allows the player to enter their name and start the game. The menu screen features include:

- A button to start the game (handled by `button1_Click` method).
- A `TextBox` for the player to enter their name.
- Name validation is implemented; a warning message is displayed if no name is entered.
- A link to view "Best Scores" is provided, displaying the highest scores on a separate form (`linkLabel1_LinkClicked` method).

Additionally, game information and instructions are provided on the screen:

- "Use the mouse to select and/or arrow keys to move objects."
- "Press the P key to pause the game."

## Game Screen

The game screen is designed to be dynamic and user-friendly:

### Game Board

- An 8x8 game board is created.
- At the start of the game, colorful objects and jokers are placed randomly (`InitializeGameBoard` method).
- The initial placement ensures no matches are present (`WouldCauseMatch` method).

### Game Objects

- Four different colored objects and four different jokers (Rocket, Helicopter, Bomb, Rainbow) are included. Joker functionalities are as follows:
  - **Rocket**: Pops all objects horizontally or vertically (`RocketTile` class).
  - **Helicopter**: Pops a random object (`CopterTile` class).
  - **Bomb**: Pops its 8 adjacent neighbors (`BombTile` class).
  - **Rainbow**: Chooses a random color and pops all objects of that color (`RainbowTile` class).

- Objects can only be moved horizontally and vertically (`AreAdjacent` method).
- Joker abilities are successfully implemented.

### Popping Mechanism

- At least three objects of the same color aligned horizontally or vertically will pop (`CheckAndHandleMatches` method).
- New objects are dynamically added to the empty spaces (`DropTiles` and `FillEmptySpaces` methods).
- Dynamic control ensures the board is checked continuously as long as pops occur.

### Scoring and Timing

- Players earn points when objects are popped (`UpdateScore` method). Each match awards 10 points.
- A timer starts at the beginning of the game (`GameTimer_Tick` method). The game ends when the timer runs out (`EndGame` method).
- The total time and score are displayed in real-time on the game screen (`lblScore`, `lblTime`).

### Controls

- Players can select tiles using the mouse and move them with arrow keys (`Tile_Click` and `HandleKeyboardInput` methods).
- Pressing the "P" key pauses and resumes the game (`PauseGame` method).

## High Score Management and Display

- At the end of the game, the player's score is checked. If it is among the top 5 highest scores, it is saved (`CheckHighScore` method).
- Scores are saved to a text file and updated each time (`SaveHighScores` and `LoadHighScores` methods).
- The "Best Scores" link in the menu screen allows players to view the top scores on a separate screen (`ShowHighScores` method).

# Object-Oriented Programming (OOP) Usage

The project adheres to object-oriented programming principles. Below is an explanation of how OOP concepts are applied:

### Inheritance

- A `Tile` class is defined for game objects, and jokers are derived from this class (`NormalTile`, `RocketTile`, `CopterTile`, `BombTile`, `RainbowTile`).
- The `Tile` class contains common properties and methods, while derived classes implement specific behaviors for jokers.

### Polymorphism

- Virtual methods in the `Tile` class (`OnMatched`, `Destroy`) are overridden by derived classes to implement joker-specific functionalities.

### Encapsulation

- Class properties and methods are protected by appropriate access modifiers (`private`, `protected`, `public`).

### Abstraction

- The `Tile` class is designed as an abstract structure, extended by joker classes. This avoids code repetition.

### Static Properties

- A static property `PlayerName` is defined to hold the player's name, which is utilized in the `Form2` class.

# Conclusion and Evaluation

This project successfully meets the specified requirements and provides an enjoyable gaming experience. Key achievements include:

- Implementation of object-oriented programming principles.
- Creation of a dynamic game board.
- Integration of joker objects to enhance gameplay.
