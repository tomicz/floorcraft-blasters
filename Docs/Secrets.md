# Secrets

API keys are never committed. They live in a `.env` file at the project root,
which the Unity editor turns into a generated `AppSecrets` asset that ships in
the build. Both files are gitignored.

| Key | Used by |
|---|---|
| `AUKI_APP_KEY`, `AUKI_APP_SECRET` | Auki posemesh session and DDS authentication |
| `AMPLITUDE_API_KEY` | Analytics |
| `REOWN_PROJECT_ID` | Wallet connection (Reown AppKit) |
| `ALCHEMY_API_KEY` | NFT ownership checks on Base |

## Setup

```bash
cp .env.example .env
# fill in the values
```

Open the project in Unity. `SecretsSync` (an editor script) reads `.env` on
editor load, before every build, and from **Matterless > Secrets > Sync from
.env**, and writes `Assets/_matterless/Resources/AppSecrets.asset`. Process
environment variables with the same names override `.env`, which is how CI
supplies them.

At runtime `RootInstaller` loads the asset with `AppSecrets.Load()` and pushes
the values into the settings objects that need them (`AukiSettings`,
`AnalyticsSettings`, `WalletSettings`, `ChainSettings`). The receiving fields
are not serialized, so nothing is written back into the tracked config assets.

## Without secrets

The project compiles and runs. `AppSecrets.Load()` logs one error, and the
features that need a key log their own warning and stay disabled: no Auki
session, no analytics, no wallet, no NFT checks.

## Guard rails

- `.env`, `AppSecrets.asset`, and its meta file are gitignored.
- The pre-push hook installed by `tools/paid-assets.sh` refuses to push a
  commit that contains those files, or a config asset where one of the old key
  fields still carries a value.

## Adding a new secret

1. Add the variable to `.env.example` and to your `.env`.
2. Add a serialized field and accessor to `AppSecrets`, and the mapping in
   `SecretsSync.s_Keys`.
3. Give the consuming settings class a non-serialized field plus a `SetSecrets`
   method, and wire it in `AppConfigs.ApplySecrets`.
