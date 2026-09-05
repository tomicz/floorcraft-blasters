using System;
using UnityEngine;
using UnityEditor;
using UEditor = UnityEditor.Editor;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace Matterless.Floorcraft.Editor
{
    [CustomEditor(typeof(BuildConfiguration))]
    public class BuildConfigurationEditor : UEditor
    {
        public override void OnInspectorGUI()
        {
            var config = (BuildConfiguration)target;
            string what = BuildLabel(config);

            GUILayout.Label("Builds");

            if (GUILayout.Button("Set App in Editor"))
                SetAppEditorSettings(config);

            if (GUILayout.Button($"Build {what}"))
                Build(config, BuildOptions.None);

            if (GUILayout.Button($"Build {what} (Dev Build)"))
                Build(config, BuildOptions.Development);

            GUILayout.Space(10);

            base.OnInspectorGUI();
        }

        private static string BuildLabel(BuildConfiguration config)
        {
            if (config.platform == BuildConfiguration.Platform.Android)
                return config.androidAppBundle ? "Android App Bundle" : "Android APK";
            return "Xcode Project";
        }

        private static BuildTarget TargetOf(BuildConfiguration config) =>
            config.platform == BuildConfiguration.Platform.Android ? BuildTarget.Android : BuildTarget.iOS;

        private static NamedBuildTarget NamedTargetOf(BuildConfiguration config) =>
            config.platform == BuildConfiguration.Platform.Android ? NamedBuildTarget.Android : NamedBuildTarget.iOS;

        private static void SetAppEditorSettings(BuildConfiguration config)
        {
            Debug.LogFormat("Set application: {0} ({1})", config.appName, config.platform);
            var namedTarget = NamedTargetOf(config);

            // product name
            PlayerSettings.productName = config.appName;
            // application identifier
            PlayerSettings.SetApplicationIdentifier(namedTarget, config.appIdentifier);
            // version
            PlayerSettings.bundleVersion = config.appVersion;
            if (config.platform == BuildConfiguration.Platform.Android)
                PlayerSettings.Android.bundleVersionCode = config.buildNumber;
            else
                PlayerSettings.iOS.buildNumber = config.buildNumber.ToString();
            // scenes
            EditorBuildSettings.scenes = config.scenes.ToArray();
            // symbols
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, config.defines);

            AssetDatabase.Refresh();

            Debug.Log("***********************");
            Debug.Log($"   VER: {config.fullVersion}");
            Debug.Log("***********************");
        }

        // This function is used both from the manual button press, and from continuous integration builds (see BuilderForCI.cs)
        internal static BuildReport Build(BuildConfiguration config, BuildOptions buildOptions, bool interactive = true)
        {
            BuildReport buildReport = null;

            SetAppEditorSettings(config);

            var target = TargetOf(config);
            string what = BuildLabel(config);

            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            path = Path.Combine(path, config.appBuildFolder);

            if (buildOptions == BuildOptions.Development)
                path += "_dev";

            // iOS produces an Xcode project folder; Android produces a single file
            if (target == BuildTarget.Android)
                path += config.androidExtension;

            if (!interactive || EditorUtility.DisplayDialog($"Build {what}", $"Build {what} at:\n\"{path}\" ?", "Build", "Nope!"))
            {
                //config.IncreaseBuildNumber();
                SetAppEditorSettings(config);

                if (target == BuildTarget.Android)
                    PrepareAndroid(config);

                // Build player
                buildReport = BuildPipeline.BuildPlayer(config.scenePathArray, path, target, buildOptions);

                if (interactive)
                {
                    // show path
                    EditorUtility.RevealInFinder(path);
                }
            }
            else
            {
                Debug.Log("Build Canceled");
            }

            return buildReport;
        }

        /// <summary>
        /// Android specifics. Player Settings keeps the keystore path and alias, but the passwords only live
        /// for the editor session, so a fresh session or CI can pass them through environment variables.
        /// </summary>
        private static void PrepareAndroid(BuildConfiguration config)
        {
            EditorUserBuildSettings.buildAppBundle = config.androidAppBundle;

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                Debug.LogWarning("[Build] Active platform is not Android; Unity switches platform as part of this build, which takes a while.");

            if (!PlayerSettings.Android.useCustomKeystore)
            {
                Debug.LogWarning("[Build] Android build uses the debug keystore. Google Play rejects it; set the release keystore in Player Settings > Publishing Settings.");
                return;
            }

            string keystorePass = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASS");
            string keyaliasPass = Environment.GetEnvironmentVariable("ANDROID_KEYALIAS_PASS");
            if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) && !string.IsNullOrEmpty(keystorePass))
                PlayerSettings.Android.keystorePass = keystorePass;
            if (string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass) && !string.IsNullOrEmpty(keyaliasPass))
                PlayerSettings.Android.keyaliasPass = keyaliasPass;

            if (string.IsNullOrEmpty(PlayerSettings.Android.keystorePass) || string.IsNullOrEmpty(PlayerSettings.Android.keyaliasPass))
                Debug.LogWarning("[Build] Keystore passwords are not set for this editor session. Enter them in Player Settings > Publishing Settings, or set ANDROID_KEYSTORE_PASS and ANDROID_KEYALIAS_PASS.");
        }
    }
}
