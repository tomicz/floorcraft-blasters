# Floorcraft: Blasters

A multiplayer augmented reality racing game built with Unity, featuring real-time spatial multiplayer powered by Auki's Posemesh SDK, Web3 wallet integration, and NFT support.

## Features

- **AR Foundation** - Cross-platform AR for iOS and Android
- **Spatial Multiplayer** - Share AR experiences using Auki's Posemesh (ConjureKit, Manna, Grund)
- **Domain Sessions** - Join persistent shared AR spaces via QR codes
- **Web3 Integration** - Wallet connectivity via Reown (WalletConnect)
- **NFT Support** - ERC-1155 token ownership verification on Base blockchain
- **Multiple Game Modes**:
  - Free For All - Solo racing
  - Join Session - Multiplayer via QR code
  - Mayhem Mode - PvE with AI enemies

## Paid Assets (optional)

The official builds use two paid assets that are not included here: AVPro Movie
Capture (video recording) and the EffectCore explosion pack (particle materials).
The project compiles and runs without them; recording is disabled and the
affected particles show missing materials. Licensed developers can install them
with `tools/paid-assets.sh install`. See [Docs/PaidAssets.md](Docs/PaidAssets.md).

## Getting Started

### Prerequisites

- **Unity 2022.3.62f2** (or compatible 2022.3.x LTS version)
- **Platform SDKs:**
  - iOS: Xcode 14+ and iOS 14+ device
  - Android: Android SDK with API Level 24+ (Android 7.0+)
- **Git** for cloning the repository
- **ARCore/ARKit compatible device** (AR features won't work in Unity Editor camera view)

### Installation

1. **Clone the repository:**

   ```bash
   git clone <repository-url>
   cd floorcraft-blasters
   ```

2. **Open in Unity:**

   - Launch Unity Hub
   - Click "Open" or "Add"
   - Select the `floorcraft-blasters` folder
   - Unity will import all assets (this may take several minutes on first open)

3. **Verify dependencies:**
   - Unity Package Manager should automatically resolve packages
   - Check that these packages are installed:
     - AR Foundation (4.2.9)
     - Auki ConjureKit
     - Matterless modules (Audio, Localisation, Inject)

### Initial Configuration

Before running the project, you **must** configure the following:

1. **API keys** go into `.env`, never into the config assets (see [Docs/Secrets.md](Docs/Secrets.md)):

   ```bash
   cp .env.example .env
   ```

   - `AUKI_APP_KEY` and `AUKI_APP_SECRET` from [Auki Console](https://console.auki.network), required for multiplayer
   - `AMPLITUDE_API_KEY` from [Amplitude](https://amplitude.com), required for analytics
   - `REOWN_PROJECT_ID` and `ALCHEMY_API_KEY` for the optional wallet and NFT features (see the sections below)

   The editor generates the gitignored `AppSecrets` asset from this file when the project opens.

2. **Auki domain** (optional, for testing): open `Assets/_matterless/Data/Blasters Configs` and set the App Domain ID.

3. **Optional Integrations:**
   - Reown WalletConnect (see Wallet Integration section below)
   - Alchemy/Base NFT (see NFT Integration section below)
   - Backtrace crash reporting
   - Joystick remote config

### Building and Running

#### Testing in Unity Editor

**Note:** AR camera won't work in Editor, but you can test multiplayer logic:

1. **Quick Session Testing (Editor Only):**

   - Press **D-O-M** keys in sequence to simulate scanning a domain QR code
   - Press **D-E-V** keys in sequence to open debug menu

2. **Start Play Mode:**
   - Click the Play button
   - You'll see UI but no AR camera feed (expected in Editor)

#### Building for iOS

1. **Configure iOS Build Settings:**

   - File → Build Settings
   - Select iOS platform
   - Switch Platform (if needed)

2. **Player Settings:**

   - Set Bundle Identifier (e.g., `com.yourcompany.floorcraft`)
   - Set Camera Usage Description
   - Minimum iOS Version: 14.0

3. **Build:**
   - Click "Build" and select output folder
   - Open generated Xcode project
   - Connect iOS device and run from Xcode

#### Building for Android

1. **Configure Android Build Settings:**

   - File → Build Settings
   - Select Android platform
   - Switch Platform (if needed)

2. **Player Settings:**

   - Set Package Name
   - Minimum API Level: 24 (Android 7.0)

3. **Build and Run:**
   - Connect Android device via USB
   - Enable USB Debugging on device
   - Click "Build and Run"

### Project Structure

```
Assets/
├── _matterless/
│   ├── Data/                    # ScriptableObject configs (⚠️ Add your API keys here)
│   ├── Scenes/                  # Main game scene
│   ├── Scripts/
│   │   ├── Runtime/
│   │   │   ├── Multiplayer/     # Auki/ConjureKit integration
│   │   │   ├── MainHUD/         # UI services
│   │   │   ├── ECS/             # Entity Component System for networked objects
│   │   │   ├── NFT/             # Web3 wallet & NFT integration
│   │   │   └── Analytics/       # Amplitude analytics
│   │   └── Editor/              # Unity Editor tools
│   └── Modules/                 # Matterless framework modules
└── Packages/
    └── manifest.json            # Package dependencies
```

### First Run Checklist

- [ ] Unity version 2022.3.62f2 installed
- [ ] Project opens without errors
- [ ] Auki App Key and App Secret configured
- [ ] Amplitude analytics key configured
- [ ] AR-capable device available for testing
- [ ] Build successfully deploys to device
- [ ] AR camera activates on device launch

### Troubleshooting

**"Missing interface binding" errors:**

- Check that all API keys are filled in `Blasters Configs`
- Restart Unity after configuration changes

**AR camera shows black screen:**

- Ensure you're running on a real device (not simulator/editor)
- Check camera permissions in device settings

**Multiplayer connection fails:**

- Verify Auki App Key and App Secret are correct
- Check device internet connection
- Ensure you're on the correct build environment (staging/prod)

**Build errors on iOS/Android:**

- Verify minimum OS versions meet requirements
- Check that AR Foundation and ARCore/ARKit XR Plugins are installed

---

## Configuration Details

The `_matterless/Data` folder contains configurations, some of which you'll need to update with your own information.

- `Blasters Configs` and `Floorcraft Configs` hold configurations for privacy policy, terms of services, Auki posemesh domain id, wallet metadata, and NFT contracts. API keys are not in these assets; they come from `.env` (see [Docs/Secrets.md](Docs/Secrets.md))
- `Backtrace Configuration` has the settings for [https://backtrace.io/](https://backtrace.io/).
- `Blasters Environment Settings` contains API Key settings for [https://www.getjoystick.com/](https://www.getjoystick.com/).

You will also need an [Amplitude](https://amplitude.com/) API key, set as `AMPLITUDE_API_KEY` in `.env`, for tracking analytics.

## Wallet Integration with Reown WalletConnect

This project uses [Reown (formerly WalletConnect)](https://reown.com/) for blockchain wallet connectivity. To set up wallet functionality:

1. **Get your Reown Project ID:**

   - Visit [https://cloud.reown.com](https://cloud.reown.com)
   - Create a free account or sign in
   - Create a new project and copy your Project ID

2. **Configure in Unity:**
   - Open Unity Editor
   - Navigate to `Assets/_matterless/Data/`
   - Select `Blasters Configs`
   - In the Inspector, find the "Wallet Settings" section
   - Put the Project ID from step 1 in `.env` as `REOWN_PROJECT_ID` (it is not stored in the asset)
   - Replace the placeholder values with your actual information:
     - **Project Name:** Your application name
     - **Project Description:** Brief description of your app
     - **Project URL:** Your project website
     - **Project Icon URL:** URL to your app icon (recommended size: 512x512px)

## NFT Integration with ERC-1155

This project includes NFT (Non-Fungible Token) integration using [Nethereum](https://nethereum.com/) and JSON-RPC fallbacks for reading ERC-1155 tokens on the Base blockchain. The implementation is read-only (no transactions), making it safe and free to use on mainnet.

1. **Get your Alchemy API Key:**

   - Visit [https://alchemy.com](https://alchemy.com)
   - Create a free account or sign in
   - Create a new App and select "Base" as the network
   - Copy your API Key from the dashboard

2. **Configure in Unity:**

   - Open Unity Editor
   - Navigate to `Assets/_matterless/Data/`
   - Select `Blasters Configs`
   - In the Inspector, find the "Chain Settings" section
   - Replace the placeholder values:
     - **NFT Contract Address:** Your ERC-1155 contract address on Base blockchain (e.g., `0x1234...`)
     - **RPC Endpoint:** Keep as `https://base-mainnet.g.alchemy.com/v2/`
   - Put the API Key from step 1 in `.env` as `ALCHEMY_API_KEY` (it is not stored in the asset)

3. **Features:**

   - Check ownership of specific token IDs (per-vehicle gating)
   - Retrieve token metadata URIs (supports `{id}` replacement and IPFS gateways)
   - Load NFT images from metadata for UI display
   - Read-only operations (no gas fees or transactions)

4. **Usage in Code:**
   ```csharp
   // NFTService is available through dependency injection
   bool owns = await nftService.OwnsToken(walletAddress, tokenId);
   string tokenURI = await nftService.GetTokenURI(tokenId);
   var sprite = await nftService.LoadNFTImage(tokenId);
   ```

## Important Security Note

**Never commit API keys to the repository.** The Auki app key and secret, the
Reown project ID, the Alchemy API key, and the Amplitude API key live only in
the gitignored `.env` file and the asset generated from it (see
[Docs/Secrets.md](Docs/Secrets.md)). The pre-push hook refuses commits that
contain them. Backtrace and Joystick settings are still stored in their config
assets, so keep those as placeholders in version control.

## Removed Dependencies

The original project had dependencies on two paid assets which were removed from the open-source version:

- [Effectcore Stylized Explosion Pack 1](https://assetstore.unity.com/packages/vfx/particles/stylized-explosion-pack-1-79037) for explosion effects.
- [AVPro Movie Capture](https://assetstore.unity.com/packages/tools/video/avpro-movie-capture-mobile-edition-221852) for recording functionality.

The project also has Unity package manager dependencies to the following repositories

- [https://github.com/matterless/audio-module](https://github.com/matterless/audio-module)
- [https://github.com/matterless/localisation-module](https://github.com/matterless/localisation-module)
- [https://github.com/matterless/inject-module](https://github.com/matterless/inject-module)
