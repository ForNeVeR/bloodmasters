Changelog
=========

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] (2.0.0)
### Changed
- The RAR archives containing the game content are replaced with ZIP

## 1.1
- **Rewritten the sound code to improve performance.**
- Fixed max clients check in server which causes crash on full server.
- Fixed flag detach from player after team change with flag.
- Flag now rotates nicely with player angle when carried by a player.
- Removed erroneous scavenger items in Mayan Temple.
- **Fixed invisible (under-the-ground) players.**
- Fixed maximum players limit.
- Fixed electric shocks across the map when target or source object teleports.
- Adjusted speed of legs animation.
- Added new MP3 track "Confronting Dreams".
- **Added different movement method options.**
- Increased flyby damage Ion Cannon.
- Joining game through Game Details now also downloads maps.
- Mouse cursor locked inside window when playing in windowed mode.
- Auto-respawn feature added.
- Added switch to next best weapon when weapon is empty.
- Control options no longer allow the same control on multiple actions.
- Added server version information in game details.
- Added callvote for next map, map change and restart.
- Screenshots are now saved with readable dates and time in filename.
- Added red flashes to health status as alternative to screen flashes.
- Added flag icons in scoreboard that indicate who took your flag.
- Fixed issues with team-colored player names.
- **Fixed full-bright red screen flashes on some ATI cards.**
- **Total game reset (items, scores and players) after countdown.**
- Fixed join red/blue menu items in games where manual team choice is allowed.
- Added overview of weapons and ammo when switching weapons.
- Powerup names now also displayed in status bar when taken.

## 1.0
- Fixed crash in launcher when downloading file.
- Fixed `ServerBrowser.GetFilteredList()` crash in launcher.
- Linked with correct DirectX libraries.
- Console input now cleared when closing console.
- Fixed Avenger animation speed.
- Decreased initial ammunition for several weapons.
- Fixed renamed player name after map change.
- Fixed weapon switching crash when switching to the same weapon.

## 1.0-RC2
- Added Revenge of Seth MP3 music track.
- Fixed floor textures in Mayan CTF Temple map.
- Changed powerups in some CTF maps.
- Fixed auto-switch-weapons option.
- Added The Damp Place map by Boris Iwanski.
- Added mouse scrollwheel support to the controls list.
- Right box in statusbar shows score and topscore in DM.
- Right box in statusbar shows team scores in TDM and CTF.

## 1.0-RC1
- Changed Mayan Temple and added CTF version.
- Fixed flames timing.
- Fixed "incinerated" death message.
- Phoenix now does less direct damage.
- Increased nuke damage.
- Halved animations framerate (less memory usage, better performance)

## 0.9
- Added map Mayan Temple.
- Fixed invisible projectiles bug.
- Fixed weapons pickup crash.
- Fixed broken chatbox background after ALT+TAB.
- Only weapons and powerups will be bobbing, other items will hold still.
- Ability to change player name in game (not saved to config).

## 0.8
- Ammo collected before weapon found is now preserved.
- Shields are now stronger.
- Geometry after fullscreen ALT+TAB switch fixed.
- Game menu key (ESC) now also aborts connecting if still connecting.
- Changed `fraglimit` on game details in launcher to `scorelimit`.
- Launcher can now auto-download the map from the server website.

[1.2]: https://github.com/ForNeVeR/bloodmasters/releases/tag/v1.2
[Unreleased]: https://github.com/ForNeVeR/bloodmasters/compare/v1.2...HEAD
