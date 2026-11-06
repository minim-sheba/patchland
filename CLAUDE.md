# Seaboard–Unity Piece: Handover for Claude Code
*Written 2026-10-27, continuing from a claude.ai chat. Also known as "the Seaboard piece" / "the VR piece" / Patchland.*

## About me and how I'd like to work
- Composer-performer and researcher. I know some Pure Data and Max/MSP but use them intermittently, so I often need reminding of basics. Background in web development; learned some C++ ~30 years ago.
- These conversations form part of my **academic documentation** of the work. Please explain *why* you're suggesting something, ask me why I want something if I haven't said, and question my assumptions rather than just agreeing.
- British English. Succinct but friendly. Prefer analytical feedback over silently rewriting things.
- For Pd help, explain techniques in prose; don't build demo/test patches unless I ask.
- Any factual overviews: give sources and flag their limitations.
- **Deletions:** I handle deletions myself. Archive or rename things rather than deleting them, unless I say otherwise.
- **Commits:** you can commit and push for me. Draft a subject line and a technical body (what changed and why), then leave a **`Practice notes:`** section and ask me to fill it in before committing. These form part of my documentation of creative practice. Keep the `Co-Authored-By: Claude` line.

## Repository layout (reorganised 2026-10-30)
Lives at `C:\Users\caitlin\GitHub\patchland`, deliberately outside OneDrive. The GitHub repo is `minim-sheba/patchland`, renamed from `seaboard-vr` on 2026-10-31 once the piece had a title (GitHub redirects the old URL). It's also outside Documents to keep paths short: Unity 6.3 doesn't support Windows long paths, and its `Library\PackageCache` files come within about 7 characters of the 259-character limit. If Unity reports path errors after installing packages, move the repo to `C:\GitHub\`. There's a `GitHub` shortcut in Documents as a reminder to back this up before any reinstall. In Unity Hub, always open the project by its real path, not through the shortcut. The old OneDrive copies are in `…\OneDrive - Royal Holloway University of London\_Personal\GitHub\seaboard-vr_OLD`, which I'll delete once everything is confirmed working.
```
pd/                     current performance patches
externals/mpe_poly/
  src/                  C++ source and .def
  help/                 help patches (mpe-midi-prep.pd is used by mpe_poly-help.pd)
  build/windows/        Visual Studio solution and project (build output in x64/ is ignored)
  build/macos/          (later) pd-lib-builder Makefile
  bin/                  Release DLLs, committed deliberately
unity/
  Patchland/            the Unity project, Unity 6000.3.25f1 (nested so Unity Hub lists it as "Patchland")
archive/                superseded patches, old externals (mpe-external, mpe-simple), old scripts
```
Exported Claude chats live in my Obsidian vault (`Hub/TIME/AI chats/`), not here.
- **Git tracks only small, authored parts of `unity/Patchland/`:** `Assets/Scripts`, `Assets/Scenes`, `Assets/InputSystem_Actions.inputactions`, `ProjectSettings/` and `Packages/manifest.json`. Everything else is ignored: Library, `Assets/BlenderFiles` (2.4 GB+), textures, audio and Starter Assets. That's on purpose, because the 3D files are far too big for GitHub. I back up the full Unity project separately (cloud drive plus external disk), and I document 3D work in my Obsidian daily notes.
- **`.blend` files live in `Assets/BlenderFiles`** so I can edit and test straight away. This only works on this PC, which is the only machine with Blender, but builds don't need Blender.
- Pd loads `mpe_poly` from Pd's own externals folder. I copy built DLLs there by hand.

## The piece
Live performance in which a ROLI Seaboard (5D MPE: Strike, Press, Glide, Slide, Lift) controls movement through a Unity 3D environment while also generating expressive audio in Pure Data. One performer operates everything; projected visuals + amplified sound. **Stability matters more than features.**

## Machines (all projects are cross-platform)
- **Development:** Windows PC (high-performance laptop, Focusrite 2i2, ASIO) — this is where current work happens. Freshly wiped and reinstalled, which is why things are being set up again.
- **Travel:** M1 MacBook Air, 256GB — very limited; nothing for this project on it yet.
- **Eventual performance targets (2027, limited access):** Zubin Kanga's very high-spec new MacBook Pro, and/or the Futures Studio computer (thought to be Windows; high-spec, runs Unreal virtual sets onto an 8K screen). Keep these in mind: anything that only works on my dev machine is a problem.

## Signal chain
```
Seaboard → Pure Data (mpe_poly external, 6 voices) → OSC (mrpeach) → Unity (OscJack)
                                                  ← (future) OSC back from Unity
```

## Pure Data side — current state
- Patch: `pd/3D_movement_test.pd`.
- **Sending behaviour (checked 2026-10-29):** the patch sends only when the note-on state or the average glide *changes*. Nothing is sent while idle or while a note is held still, so a Unity-side timeout would need a heartbeat from Pd first. **`[udpsend]` is not connected on load**: `connect localhost 8000` has to be clicked by hand, which is a performance risk to fix with `[loadbang]` → `[delay]`.
- **OSC:** mrpeach library, installed via Deken. Message box `[send /seaboard/note_active $1 $2(` → `[packOSC]` → `[udpsend]` to localhost **port 8000**.
- **Message:** `/seaboard/note_active` with two floats:
  1. note active (1 = any key held, 0 = none)
  2. average glide across active voices (Pd multiplies by 50 before sending)
- Decision: **keep mrpeach for now** to restore a known-working setup; consider switching to vanilla `[oscformat]` / `[oscparse]` + `[netsend -u -b]` later (fewer dependencies across machines). mrpeach's `[routeOSC]` is handy for receiving.
- **Custom external `mpe_poly`:** C/C++, built with Visual Studio 2026 (toolset v145), source on GitHub and local. Current DLL is a **Debug build**; it only loads because Visual Studio is installed (needs `ucrtbased.dll`). Must be rebuilt as **Release with /MT** (self-contained) well before any Futures Studio testing (unlikely before Jan 2027), and compiled for macOS later. **Correction (2026-10-30):** only *Debug x64* is set up to build a DLL. *Release x64* exists but is set to build an `.exe` (ConfigurationType `Application`), so it fails to link. It also doesn't set `/MT` yet. Both need fixing in the Release/`/MT` step.
- Pd audio: make sure the Focusrite ASIO driver is selected for both in and out and use *Save All Settings* (Pd kept reverting to the Realtek ASIO driver).

## Unity side — where we are
- **Unity project "Patchland"** is now at `unity/Patchland/`. Its scripts are `SeaboardReceiver3D.cs` (see below) and `EnvironmentalSound.cs` (written directly in Unity, Oct 2026). The scene is `Assets/Scenes/1_notein.unity`. Script backups from **11 September 2025** are in `archive/2025-09_unity-scripts/`. The root-level `seaboard_receiver_script.cs`, now in `archive/2025-08_prototypes/`, was the earlier Flappy Bird prototype.
- Using **Starter Assets – Third Person** (CharacterController + `ThirdPersonController` + `StarterAssetsInputs` + Cinemachine). The old `PlayerMovement.cs` is no longer needed.
- OSC package: **OscJack** by Keijiro (`https://github.com/keijiro/OscJack.git` via Package Manager, or check its README for the current scoped-registry method).
- **Input handling:** Project Settings → Player → Active Input Handling is set to **"Both"**. However, I've had problems in the past with Unity refusing to read old-style input, so **use the new Input System for all new code** — no `Input.GetAxis` / `Input.GetKey` etc. "Both" is a safety net, not something to rely on. **Decision: leave it on "Both"** — some visual effects I may want later come from an older source, and I want to keep things simple.

### `SeaboardReceiver3D` (rewritten 2026-11-06, attached to PlayerArmature)
Feeds **`StarterAssetsInputs`** rather than moving the character, so `ThirdPersonController` handles movement, animation and the Cinemachine camera.
- note active → `move` input forward (camera-relative and animated). Walking speed is **Move Speed** on `ThirdPersonController` (2 m/s default; the old script used 8).
- average glide → horizontal `look` input, converted so that turning isn't frame-rate dependent. Slow turns are accumulated, because `ThirdPersonController` ignores look input below 0.1.
- **Glide shaping (Inspector):** Max Turn Rate (45°/s), Glide Dead Zone (5), Glide Smooth Time (0.3 s). Added because vibrato shook the view and small bends caused sudden spins: the unshaped maximum was 500°/s. The smoothing filters out vibrato, at the cost of a slower response. Still being tuned by playing.
- **Mouse recovery:** Allow Mouse Look toggle in the Inspector. Keyboard and mouse aren't overridden when the Seaboard is idle.
- Logging and the overlay are off by default, with Inspector toggles. OSC callback fields are `volatile`; all Unity API calls stay in `Update()`.

### Open question: glide and pitch scaling in `mpe_voice`
`mpe_voice_pitchbend` multiplies bends by 50 and limits them to ±1, and the Pd patch multiplies by 50 again. If the Seaboard's bend range is ±48 semitones (not yet confirmed in ROLI Dashboard), glide reaches its maximum at about one semitone, and the same code may exaggerate and cap the audible pitch at ±2 semitones. Check the bend range and how pitch tracks the finger before changing the external.

## Next steps
1. ~~Install OscJack.~~ Done. (Active Input Handling confirmed as "Both"; new Input System only for code.)
2. ~~Rewrite `SeaboardReceiver`.~~ Done, and working with the Seaboard (2026-11-06).
3. Keep tuning glide and walking speed by playing; check the bend-range question above.
4. Later: more MPE dimensions (Press, Slide) and chord density → movement parameters.
5. Later: **Unity → Pd return OSC** (OscJack `OscClient`) on a **different port** from 8000 — e.g. entering a new zone changes the sound the Seaboard produces. Message design still to be decided.
6. Before Futures Studio: Release/`/MT` rebuild of `mpe_poly` (fix the Release ConfigurationType first, see above); macOS build via pd-lib-builder.
7. Pd robustness: auto-connect `[udpsend]` on load; consider a heartbeat so Unity can detect silence.
