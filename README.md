# Match3-Unity-Intern-Test2025

### Task 1: Re-skin All Items
- Created new prefabs for all game items and applied the new visual assets.
- Updated item prefab file paths in `Constants.cs` to map to the newly created asset prefabs.

### Task 2: Refactor Game Mechanics
- Created a new Controller class `BottomRack.cs` to manage the bottom rack of items, including adding and removing items.
- Created a new `RackSlot.cs` class to represent individual slots in the bottom rack, which can hold items and track their origin cells.
- Updated the `Cell.cs` class to include a reference to its corresponding `RackSlot` when an item is moved to the bottom rack.
- Modify `GameManager.cs` - add GAME_WON state
- Create `UIPanelWin.cs` - victory screen
- Modify `UIPanelGameOver.cs` - update lose screen
- UIManager now instantiates the BottomRack prefab and manages its visibility based on game state.
- Adjust the `BoardController.cs` to handle item collection and removal through the `BottomRack`
- Implement win condition: board empty + rack empty
- Implement lose condition: rack full without matches
- Modify `UIPanelMain.cs` - add Play/Autoplay/Auto-Lose buttons
- Modify `UIMainManager.cs` - handle new game states
- Update `GameSettings.cs` - remove timer/moves, validate board size (the total cells must be divisible by 3)
- Remove `LevelCondition`, `LevelMoves`, `LevelTime` references
- Create `AutoplayController.cs` - optimal autoplay win strategy
- Create `AutoLoseController.cs` - intentional autoplay lose strategy
- Integrate automation triggers in GameManager
- Implement 0.5s delay between automated actions
### Task 3: Implement Time Attack Mode
Notes / rationale:
- Capturing OriginCell before RemoveItem is required because RemoveItem clears the slot's OriginCell reference.
- This change keeps the normal mode mechanics intact while enabling reversible moves only for Time Attack.
 Updated BoardController input handling to detect clicks on RackSlot objects.
  - On mouse click the controller first raycasts for a Cell; if none found and the current mode is TIME_ATTACK it checks for a RackSlot.
  - When a RackSlot is tapped, the code captures `slot.OriginCell` BEFORE calling `BottomRack.RemoveItem(slot)` (RemoveItem clears the origin reference).
  - If the captured origin cell exists and is empty the item is returned to that cell, the view root is set back to the board transform and `AnimationMoveToPosition()` is used to animate the item into the cell. The controller sets `IsBusy` and re-checks win/lose after the animation.
  - If the origin is null or occupied the item is re-added to the rack to avoid data loss.

- Modified CollectCellItem so the BottomRack receives the origin cell only when in TIME_ATTACK mode:
  - NORMAL: `m_bottomRack.AddItem(item)` (unchanged behavior — non-reversible)
  - TIME_ATTACK: `m_bottomRack.AddItem(item, cell)` (stores origin for possible withdraw)

- Added Time Attack timer state to BoardController: `m_timeRemaining`, `m_timerRunning`, and `m_timerText`.
- StartGame now initializes `m_timeRemaining = gameSettings.TimeAttackSeconds` when starting in TIME_ATTACK mode and attempts to find a UI Text GameObject named `TextTimer` in the scene to drive the on-screen display.
- Update() now decrements the timer when `m_mode == TIME_ATTACK` and `m_timerRunning` (unless the game is paused or over), clamps the value, updates the displayed MM:SS text, and triggers `GameOver(won)` when time reaches zero (won = board empty).
- OnGameStateChange pauses the timer when the game enters PAUSE and resumes on GAME_STARTED; the timer is stopped on GAME_WON / GAME_OVER.
- A helper `UpdateTimerText()` formats seconds to MM:SS and writes to the `Text` component.
