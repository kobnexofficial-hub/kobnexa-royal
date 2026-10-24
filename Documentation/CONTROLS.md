# KOBNEXA ROYAL — Controls (Keyboard/Mouse + Gamepad)

The game supports **both** keyboard/mouse and a PC gamepad at the same time.
Last-input switching applies everywhere: using a stick or gamepad button
hands control to the gamepad; using the keyboard or mouse hands it back.
No gamepad is required to play.

## Gamepad route (how it is implemented)

- Primary: Unity **Input System** package (`com.unity.inputsystem` 1.14.0),
  read through `Assets/Scripts/Core/ControllerInput.cs`.
- Fallback: legacy Input Manager joystick axes/buttons, defined in
  `ProjectSettings/InputManager.asset` (`JoyLeftX/Y`, `JoyRightX/Y`,
  `JoyTrigger`, `JoyDPadX/Y`, joystick buttons 0-9), used automatically
  when no Input System gamepad is present.
- The project's Active Input Handler is set to **Both** by the editor
  setup script (`Assets/Scripts/Editor/InputHandlerSetup.cs`) so the
  existing keyboard/mouse code keeps working alongside the gamepad.
- Deadzone: 0.2 radial, with rescaling (see `ControllerInputMath`).

## Gameplay — gamepad mapping (Xbox-style names; PlayStation in brackets)

| Input | Action |
|---|---|
| Left stick | Move |
| Right stick | Camera orbit (uses Controller Sensitivity setting) |
| A / Cross (South) | Jump |
| B / Circle (East) | Melee swing (only when melee is the active weapon) |
| X / Square (West) | Reload (blaster only) |
| Y / Triangle (North) | Swap active weapon: Royal Blaster <-> Melee |
| Right trigger (RT / R2, held) | Shoot (blaster only) |
| Left bumper (LB / L1, held) | Sprint |
| Left trigger (LT / L2, held) | Sprint (aim is not implemented, so LT sprints instead) |
| Left stick click (L3, held) | Crouch |
| D-pad Up / Right / Down / Left | Use inventory slots 1 / 2 / 3 / 4 (consumables only) |
| Start (Menu) | Pause / resume |
| Back / View | Pause / resume (in a match) |

Unassigned: Right bumper (RB) and right stick click (R3) do nothing.

## Gameplay — keyboard/mouse (unchanged)

WASD move, mouse-look orbit camera, Space jump, Shift sprint, Ctrl crouch,
left mouse shoot, right mouse / F melee, R reload (plus auto-reload when
the magazine empties), P pause.

## Lobby / menus — gamepad

- D-pad or left stick: move between buttons (up/down/left/right cycles
  the visible panel's buttons in order).
- A / Cross: confirm the selected button. On a toggle it flips it, on a
  dropdown it opens it, on a slider the D-pad left/right changes its
  value instead of moving selection.
- B / Circle: back (closes a confirmation dialog, otherwise goes back
  a menu level, same as the Back button / Esc).
- The selected button is visibly highlighted through the EventSystem
  selection (the same highlight keyboard navigation uses).

## Settings (lobby Settings panel, saved locally)

- **Controller Sensitivity** (0.1-3.0, default 1.0): right-stick camera speed.
- **Controller Vibration** (on/off, default on): gamepad rumble on shots.
  Rumble only works through the Input System gamepad path; the legacy
  fallback has no rumble.

## Honest limitation

Controller code is implemented and covered by EditMode tests for its pure
parts (deadzone math, last-input switching, weapon-mode toggling, settings
clamping), but **physical-controller play-testing is pending**: no gamepad
is attached to the build VM, so the mapping has not been verified on real
hardware. If a button behaves differently on your controller (drivers map
sticks/triggers differently), tell Muse which button misbehaved and it
will be remapped.
