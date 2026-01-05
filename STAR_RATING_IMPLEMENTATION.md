# Star Rating System Implementation

The "Cups" system has been replaced with a modern 1-3 star rating system that rewards performance and destruction.

## Scoring Logic

Stars are awarded based on the percentage of the **Optimal Score** achieved in a level:

- **1 Star**: Level Completed (Reached the exit door).
- **2 Stars**: Reached **60%** of the Optimal Score.
- **3 Stars**: Reached **90%** of the Optimal Score.

## Persistence

Star ratings are stored in the `user://animals.save` file (via `ScoreManager`) and persist across sessions. Only the highest star rating and best score are kept for each level.

## UI Representation

- **Level Selection**: Each level icon displays 0-3 stars based on the player's best performance.
- **Level Completion**: An animated celebration shows the stars being awarded one by one with a bounce effect.
- **Head Animation**: The player's head shows a "Happy" expression when 3 stars are achieved.
