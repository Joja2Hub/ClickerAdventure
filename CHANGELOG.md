# Changelog

## Unreleased

### Product direction
- Documented and evolved the project as a child routine gamification app: RPG clicker gameplay plus real-life tasks approved by a parent.
- Defined the practical direction around a stronger child game loop, parent controls, progression, rewards, economy, and clearer UI.

### Real-life tasks and parent flow
- Added real-life task lifecycle statuses: assigned, submitted, approved, rejected, claimed, cancelled.
- Added child task submission with optional note for the parent.
- Added parent approval/rejection flow with optional parent feedback.
- Added parent task cancellation.
- Added reward claim flow after parent approval.
- Added Firestore-oriented runtime task handling through `QuestReceiver`.
- Added task update notifications for child-facing status changes.

### Parent controls
- Added runtime Parent Center for reviewing real-life tasks.
- Added quick-assign task templates.
- Added custom real-life task creation with title, description, gold, and XP.
- Added daily routine goal settings in parent controls.
- Added parent PIN gate with first-time setup, session unlock, and lock action.

### Daily routine progression
- Added daily real-life task goal tracking.
- Added daily completion bonus.
- Added streak tracking.
- Added configurable daily goal.
- Added routine progress display inside active quests.

### Achievements
- Added persistent achievement progress.
- Added achievements for first, five, and ten completed real-life tasks.
- Added achievements for three-day and seven-day streaks.
- Added achievement bonus rewards.
- Added achievements UI panel with progress bars and rewards.

### Game economy and upgrades
- Expanded hero upgrades beyond damage and HP.
- Added critical chance upgrade.
- Added rage gain upgrade.
- Added battle gold bonus upgrade.
- Connected new upgrade stats to combat rewards and player damage flow.
- Expanded the upgrade shop UI to show all upgrade branches.

### Battle gameplay
- Added runtime battle HUD.
- Added power strike, heal, and rage burst abilities.
- Added combo and critical hit combat loop.
- Added rage meter and ability cooldown UI.
- Added enemy health bars, hit feedback, damage popups, death effects, and attack warnings.
- Added battle performance tracking: best combo, critical hits, rage bursts, power usage, and damage taken.
- Added battle rank rewards with bonus gold and XP.

### UI integration
- Integrated runtime UI with the existing scene `UiCanvas`.
- Routed runtime panels through existing `PanelsUI`.
- Routed main runtime buttons through existing `ButtonGroup`.
- Routed popups through a shared runtime popup root.
- Added `RuntimeUiHost` to avoid creating isolated canvases when the scene already has UI.
- Removed the extra child task status button from startup because the existing `Quest` button is already the quest entry point.

### UI styling
- Added `RuntimeUiStyle` as a shared style source for font sizes, colors, card heights, and button sizes.
- Normalized `ActiveQuestPanel` spacing, section headers, empty states, daily routine card, and list layout.
- Normalized active quest item card height, text positions, text sizes, reward text, and action button size.
- Normalized runtime main buttons such as Shop, Goals, and Parent to match the existing button scale.
- Added scene UI styling pass in `UIManager` for existing buttons and panels.

### Bug fixes
- Fixed upgrade shop close behavior by disabling the blocker object on hide and ensuring panel buttons have stable layout sizes.
- Fixed town entry `NullReferenceException` by guarding missing `TownData`, missing background image, and missing button references.
- Updated town UI lookup to search child objects for `TownUIController` when the controller is not on the root object.
- Added visual lock overlays for map locations that require a higher player level.
- Added locked location feedback that shows the required level and current player level.
- Added hover/press highlighting for town zones.
- Reworked enemy attack windup so attacking reads as a lunge instead of the enemy taking damage.

### Build and maintenance
- Kept generated C# project includes updated for newly added scripts.
- Repeatedly verified changes with `dotnet build ClickerAdventure.sln --no-restore`.
- Remaining build warnings are Firebase architecture warnings about x86/MSIL references.
