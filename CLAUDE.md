# Seaboard–Unity Piece: Handover for Claude Code
*Written 2026-10-27, continuing from a claude.ai chat. Also known as "the Seaboard piece" / "the VR piece" / Patchland.*

## About me and how I'd like to work
- Composer-performer and researcher. I know some Pure Data and Max/MSP but use them intermittently, so I often need reminding of basics. Background in web development; learned some C++ ~30 years ago.
- These conversations form part of my **academic documentation** of the work. Please explain *why* you're suggesting something, ask me why I want something if I haven't said, and question my assumptions rather than just agreeing.
- British English. Succinct but friendly. Prefer analytical feedback over silently rewriting things.
- For Pd help, explain techniques in prose; don't build demo/test patches unless I ask.
- Any factual overviews: give sources and flag their limitations.

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
- Patch: `3D_movement_test.pd`.
- **OSC:** mrpeach library, installed via Deken. Message box `[send /seaboard/note_active $1 $2(` → `[packOSC]` → `[udpsend]` to localhost **port 8000**.
- **Message:** `/seaboard/note_active` with two floats:
  1. note active (1 = any key held, 0 = none)
  2. average glide across active voices (Pd multiplies by 50 before sending)
- Decision: **keep mrpeach for now** to restore a known-working setup; consider switching to vanilla `[oscformat]` / `[oscparse]` + `[netsend -u -b]` later (fewer dependencies across machines). mrpeach's `[routeOSC]` is handy for receiving.
- **Custom external `mpe_poly`:** C/C++, built with Visual Studio 2026 (toolset v145), source on GitHub and local. Current DLL is a **Debug build**; it only loads because Visual Studio is installed (needs `ucrtbased.dll`). Must be rebuilt as **Release with /MT** (self-contained) well before any Futures Studio testing (unlikely before Jan 2027), and compiled for macOS later. The Release configuration already exists in the project.
- Pd audio: make sure the Focusrite ASIO driver is selected for both in and out and use *Save All Settings* (Pd kept reverting to the Realtek ASIO driver).

## Unity side — where we are
- **New Unity project**; old project files not yet found. Script backups from **11 September 2025** (may be earlier than the versions described in my Sept summary, which mentioned a modified `MouseMovement.cs` combining mouse + glide; not sure that was ever implemented).
- Using **Starter Assets – Third Person** (CharacterController + `ThirdPersonController` + `StarterAssetsInputs` + Cinemachine). The old `PlayerMovement.cs` is no longer needed.
- OSC package: **OscJack** by Keijiro (`https://github.com/keijiro/OscJack.git` via Package Manager, or check its README for the current scoped-registry method).
- **Input handling:** Project Settings → Player → Active Input Handling is set to **"Both"**. However, I've had problems in the past with Unity refusing to read old-style input, so **use the new Input System for all new code** — no `Input.GetAxis` / `Input.GetKey` etc. "Both" is a safety net, not something to rely on. **Decision: leave it on "Both"** — some visual effects I may want later come from an older source, and I want to keep things simple.

### Backed-up scripts
- `SeaboardReceiver3D.cs` — OscJack server on 8000; sets `isMovingForward` and `currentGlideValue` in the OSC callback; in `Update()` calls `controller.Move()` and `transform.Rotate()` directly.
- `MouseMovement.cs` — original first-person mouse look using old `Input.GetAxis`.
- `ClaudePlayerMovement.cs` (class `PlayerMovement`) — old WASD/jump controller; being dropped.

### Problems found in review
1. **File/class mismatch:** `SeaboardReceiver3D.cs` contains class `SeaboardReceiver` — Unity won't attach it.
2. **Dependency on `PlayerMovement`** and a GameObject named "Player" (Starter Assets uses `PlayerArmature`) — won't compile once PlayerMovement is removed.
3. **`MouseMovement.cs` uses the old Input Manager** — conflicts with the decision to use the new Input System only. Probably redundant now; Starter Assets handles look and cursor lock via the new system.
4. **Receiver bypasses the controller:** direct `Move()`/`Rotate()` fights `ThirdPersonController` — no walk animation, and glide turns the body but not the Cinemachine camera.
5. **`Debug.Log` on every OSC message** — performance cost; put behind a debug toggle.
6. **`OnGUI` overlay** would appear in the projection — make it toggleable/off by default.
7. **Threading:** OSC callback runs on a background thread; fields read in `Update()` should be `volatile` (or locked). Keep all Unity API calls in `Update()`.

### Proposed approach (agreed in principle, not yet built)
Rewrite the receiver to **feed `StarterAssetsInputs`** rather than move the character:
- note active → `move` input forward (so movement is camera-relative and animated)
- average glide → horizontal `look` input (turns the camera; check how `ThirdPersonController` scales look for mouse vs other devices so glide isn't frame-rate dependent)
- **Keep the mouse as a recovery tool** (e.g. if stuck facing a wall), with an Inspector toggle to disable it so an accidental bump can't swing the camera.
- Don't override keyboard/mouse input when the Seaboard is idle.

## Next steps
1. Install OscJack. (Active Input Handling confirmed as "Both"; new Input System only for code.)
2. Rewrite `SeaboardReceiver` as above; fix naming; remove old dependencies.
3. Test with Pd sending, debug logging on, then tune sensitivities in the Inspector.
4. Later: more MPE dimensions (Press, Slide) and chord density → movement parameters.
5. Later: **Unity → Pd return OSC** (OscJack `OscClient`) on a **different port** from 8000 — e.g. entering a new zone changes the sound the Seaboard produces. Message design still to be decided.
6. Before Futures Studio: Release/`/MT` rebuild of `mpe_poly`; macOS build via pd-lib-builder.
