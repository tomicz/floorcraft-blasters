using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Matterless.Floorcraft.Editor
{
    /// <summary>
    /// Generates Assets/_matterless/Resources/AppSecrets.asset from the project's .env file.
    /// Process environment variables with the same names take precedence, which is what CI uses.
    /// Runs when the editor loads, before every build, and from the Matterless > Secrets menu.
    /// See Docs/Secrets.md.
    /// </summary>
    [InitializeOnLoad]
    public class SecretsSync : IPreprocessBuildWithReport
    {
        private const string AssetPath = "Assets/_matterless/Resources/AppSecrets.asset";
        private const string EnvFileName = ".env";

        // Environment variable name -> serialized field on AppSecrets.
        private static readonly (string env, string field)[] s_Keys =
        {
            ("AUKI_APP_KEY", "m_AukiAppKey"),
            ("AUKI_APP_SECRET", "m_AukiAppSecret"),
            ("AMPLITUDE_API_KEY", "m_AmplitudeApiKey"),
            ("REOWN_PROJECT_ID", "m_ReownProjectId"),
            ("ALCHEMY_API_KEY", "m_AlchemyApiKey"),
        };

        public int callbackOrder => -100;

        static SecretsSync()
        {
            EditorApplication.delayCall += () => Sync(verbose: false);
        }

        public void OnPreprocessBuild(BuildReport report) => Sync(verbose: true);

        [MenuItem("Matterless/Secrets/Sync from .env")]
        public static void SyncFromMenu() => Sync(verbose: true);

        private static string EnvPath => Path.GetFullPath(Path.Combine(Application.dataPath, "..", EnvFileName));

        private static void Sync(bool verbose)
        {
            var values = ReadEnvFile(EnvPath);
            foreach (var (env, _) in s_Keys)
            {
                var fromProcess = Environment.GetEnvironmentVariable(env);
                if (!string.IsNullOrEmpty(fromProcess))
                {
                    values[env] = fromProcess;
                }
            }

            var anyValue = false;
            foreach (var (env, _) in s_Keys)
            {
                anyValue |= values.TryGetValue(env, out var v) && !string.IsNullOrEmpty(v);
            }

            var asset = AssetDatabase.LoadAssetAtPath<AppSecrets>(AssetPath);
            if (!anyValue)
            {
                if (asset == null)
                {
                    Debug.LogWarning($"[Secrets] No {EnvFileName} at {EnvPath} and no environment variables set, so " +
                                     $"{AssetPath} was not generated. Copy .env.example to .env and fill it in. See Docs/Secrets.md.");
                }
                return;
            }

            if (asset == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
                asset = ScriptableObject.CreateInstance<AppSecrets>();
                AssetDatabase.CreateAsset(asset, AssetPath);
            }

            var serialized = new SerializedObject(asset);
            var changed = false;
            var missing = new List<string>();
            foreach (var (env, field) in s_Keys)
            {
                var value = values.TryGetValue(env, out var v) ? v : string.Empty;
                if (string.IsNullOrEmpty(value))
                {
                    missing.Add(env);
                }
                var property = serialized.FindProperty(field);
                if (property.stringValue != value)
                {
                    property.stringValue = value;
                    changed = true;
                }
            }

            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
                Debug.Log($"[Secrets] {AssetPath} updated from {EnvFileName}.");
            }

            if (verbose && missing.Count > 0)
            {
                Debug.LogWarning($"[Secrets] Missing values for: {string.Join(", ", missing)}. The related features will be disabled.");
            }
        }

        /// <summary>Parses KEY=VALUE lines. Supports comments, blank lines, "export KEY=", and quoted values.</summary>
        private static Dictionary<string, string> ReadEnvFile(string path)
        {
            var result = new Dictionary<string, string>();
            if (!File.Exists(path))
            {
                return result;
            }

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                {
                    continue;
                }
                if (line.StartsWith("export "))
                {
                    line = line.Substring("export ".Length).TrimStart();
                }
                var eq = line.IndexOf('=');
                if (eq <= 0)
                {
                    continue;
                }
                var key = line.Substring(0, eq).Trim();
                var value = line.Substring(eq + 1).Trim();
                if (value.Length >= 2 && (value[0] == '"' || value[0] == '\'') && value[value.Length - 1] == value[0])
                {
                    value = value.Substring(1, value.Length - 2);
                }
                result[key] = value;
            }
            return result;
        }
    }
}
