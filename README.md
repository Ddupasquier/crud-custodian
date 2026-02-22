# Crud Custodian

> **Your character is a janitor that goes around the Safari Zone and picks up poop.**

A 2D idle / walking game built with **Unity 2022 LTS**.  Clean up after Pokemon in their stalls, earn coins, unlock new stalls, and eventually automate the whole operation.

---

## Supported Platforms

| Platform | Target | Notes |
|---|---|---|
| **iOS** | iPhone / iPad (iOS 14+) | Portrait orientation; touch input |
| **Android** | Android 5.1+ (API 22+) | Portrait orientation; touch input |
| **Windows** | Windows 10/11 (x64) | 1280×720 windowed default; keyboard + mouse |
| **macOS** | macOS 12+ (Intel & Apple Silicon via Rosetta 2 / Universal build) | 1280×720 windowed default; keyboard + mouse |

> Desktop builds start windowed (resizable). Press **Alt+Enter** or click the 🖥 toolbar button to toggle full-screen.

---

## Game Overview

```
Safari Zone map
  └─ 12 Pokemon stalls (unlocked progressively)
       └─ Each stall spawns poop piles at random intervals
            └─ Player walks over piles to collect them → earns 🪙 coins
                 └─ Spend coins to unlock the next stall or automate an existing one
```

### Progression

| Stall # | Unlock cost | Notes |
|---|---|---|
| 1 | **FREE** | Always unlocked on first launch |
| 2 | 100 🪙 | |
| 3 | 250 🪙 | |
| n | `100 × 2.5^(n-2)` 🪙 | Exponential scaling |

Automation cost per stall: `1000 + (stallIndex × 500)` 🪙  
Automated stalls self-collect poop and earn 2 coins per pile passively (vs 5 for manual).

---

## Project Structure

```
Assets/
  Scripts/
    Core/               # GameManager, CurrencyManager, SaveDataManager,
    │                   # GameStateController, GameConstants, PlatformManager,
    │                   # ReadOnlyAttribute
    Player/             # PlayerCharacter (movement), PlayerCustomization,
    │                   # PlayerPoopCollector
    Stalls/             # PokemonStall, PoopPile, StallUnlockManager,
    │                   # StallAutomationManager
    Map/                # SafariZoneMap
    UI/                 # MainMenuUI, HUDManager, StallUnlockUI,
    │                   # StallListItemUI, CharacterCustomizationUI,
    │                   # DesktopInputHandler
    Authentication/     # AuthenticationManager, GoogleAuthManager
    Data/
      ScriptableObjects/  # PokemonData, StallData, CharacterCustomizationConfig
      SaveData/           # PlayerProgressionData
  Settings/
    CrudCustodianInputActions.inputactions   # All input bindings
  Resources/
    PokemonData/        # One PokemonData .asset per Pokemon
    StallData/          # One StallData .asset per stall
  Prefabs/
    Pokemon/  Stalls/  Player/  UI/
  Scenes/
    MainMenu  SafariZone  CharacterCustomization
  Art/   Audio/
Packages/
  manifest.json         # Unity package dependencies
ProjectSettings/
  ProjectSettings.asset # Build targets, window size, orientations
```

Every folder has a single responsibility. Add new Pokemon by creating a new `PokemonData` asset in `Resources/PokemonData/` — no code changes required.

---

## Input

| Action | Keyboard / Mouse (Desktop) | Gamepad | Touch (Mobile) |
|---|---|---|---|
| Move | WASD or Arrow keys | Left Stick / D-Pad | Touch-drag (delta) |
| Sprint | Left / Right Shift | Button South (A/Cross) | — |
| Interact | E or Space | Button West (X/Square) | Tap |
| Pause | Escape | Start | — |
| Full-screen toggle | **Alt+Enter** | — | — (not applicable) |

---

## Google Sign-In Setup

### Mobile (iOS + Android)

1. Create a Firebase project at <https://console.firebase.google.com>.
2. Add your Android (`com.defaultcompany.crudcustodian`) and iOS bundle IDs.
3. Download `google-services.json` (Android) and `GoogleService-Info.plist` (iOS) into `Assets/`.
4. Import the **Google Sign-In Unity Plugin** package.
5. Replace the stub in `GoogleAuthManager.cs` (`StartNativeMobileGoogleSignIn`) with the real plugin API calls.
6. Set `GOOGLE_OAUTH_WEB_CLIENT_ID` in `GameConstants.cs`.

### Desktop (Windows + macOS)

1. In Google Cloud Console, create an **OAuth 2.0 Desktop application** credential.
2. Add `http://localhost:8080/` as an authorized redirect URI.
3. Set `GOOGLE_OAUTH_WEB_CLIENT_ID` and `GOOGLE_OAUTH_DESKTOP_REDIRECT_URI` in `GameConstants.cs`.
4. In production, move the code→token exchange to a backend server to protect client secrets.

> Guest play (no account) is always available and saves progress locally.

---

## Building

### iOS
1. Switch platform to **iOS** in Build Settings.
2. Place `GoogleService-Info.plist` in `Assets/`.
3. Build → open the Xcode project → Archive → distribute via App Store Connect.

### Android
1. Switch platform to **Android** in Build Settings.
2. Place `google-services.json` in `Assets/`.
3. Set a keystore in Player Settings → Publishing Settings.
4. Build APK / AAB → upload to Google Play Console.

### Windows (Standalone)
1. Switch platform to **Windows, Mac, Linux Standalone** → set Architecture to **x86_64**.
2. Build → distribute the resulting folder (exe + Data/).

### macOS (Standalone)
1. Switch platform to **Windows, Mac, Linux Standalone** → set Architecture to **x86_64** (Intel) or **Apple Silicon** (arm64), or enable **Universal** for both.
2. Set the macOS minimum OS version to **12.0** in Player Settings.
3. Build → notarize the `.app` bundle before distribution outside the Mac App Store.

---

## Key Design Decisions

- **All magic numbers live in `GameConstants.cs`** — tune game feel in one place.
- **`PlatformManager`** is the only place that calls `Application.platform`; every other script queries it.
- **`SaveDataManager`** wraps PlayerPrefs with a JSON envelope — swap to a cloud backend by changing one class.
- **`InputActions.inputactions`** has three control schemes (`KeyboardMouse`, `Gamepad`, `Touch`) so Unity's Input System automatically activates the right one per device.
- **Desktop window state** (size, windowed vs full-screen) is persisted in PlayerPrefs and restored on next launch.

