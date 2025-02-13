using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace FontPatcher;

// Serializable classes for JSON configuration
[Serializable]
public class CharacterConfig
{
    // List of character entries to be loaded from configuration
    public List<CharacterEntry> Characters = new List<CharacterEntry>();
}

[Serializable]
public class CharacterEntry
{
    public string Character;       // The character to be patched (e.g., 'Ç', 'Ş')
    public string SpriteName;      // Base name for sprite image files
    public float Width;            // Width adjustment for the character
}

// Main plugin class with metadata for BepInEx
[BepInPlugin("skipperskip.nitw.fontpatcher", "NITW Font Patcher", "1.0")]
public class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource LoggerInstance;

    // Dictionary to store loaded characters and their corresponding data
    private static readonly Dictionary<string, CharacterData> loadedCharacters = new();

    // Awake is called when the plugin is initialized
    private void Awake()
    {
        LoggerInstance = Logger;
        Logger.LogInfo("Initializing NITW Font Patcher");

        try
        {
            // Load character configurations and apply Harmony patches
            LoadCharacterConfig();
            Harmony.CreateAndPatchAll(typeof(SpriteFontPatches));
            Logger.LogInfo($"Successfully loaded {loadedCharacters.Count} characters");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Plugin initialization failed: {ex}");
        }
    }

    // Loads character configuration from a JSON file
    private void LoadCharacterConfig()
    {
        // Path to the configuration file
        string configPath = Path.Combine(
            Paths.PluginPath,
            "FontPatcher\\config\\character_mappings.json"
        );

        Logger.LogInfo($"Loading configuration from: {configPath}");

        // Check if the configuration file exists
        if (!File.Exists(configPath))
        {
            Logger.LogError("Configuration file not found!");
            return;
        }

        try
        {
            // Read and deserialize the JSON configuration
            string json = File.ReadAllText(configPath);
            Logger.LogDebug($"Raw JSON content:\n{json}");

            var config = JsonConvert.DeserializeObject<CharacterConfig>(json);

            // Validate the deserialized configuration
            if (config?.Characters == null)
            {
                Logger.LogError("Invalid JSON structure!");
                return;
            }

            Logger.LogInfo($"Found {config.Characters.Count} character entries");

            // Load character frames for each entry in the configuration
            foreach (var entry in config.Characters)
            {
                LoadCharacterFrames(entry);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"JSON parsing error: {ex}");
        }
    }

    // Loads sprite frames for a character entry
    private void LoadCharacterFrames(CharacterEntry entry)
    {
        const float BASE_PPU = 64f; // Base Pixels Per Unit for sprites

        try
        {
            // Construct the base path for sprite assets
            string basePath = Path.Combine(
                Path.Combine(Path.Combine(Paths.PluginPath, "FontPatcher"), "assets"),
                $"chars/{entry.SpriteName}"
            );

            List<Sprite> frames = new();

            // Load up to 3 animation frames for the character
            for (int i = 0; i < 3; i++)
            {
                string framePath = Path.Combine(basePath, $"frame_{i}.png");

                // Load image data from file
                byte[] fileData = File.ReadAllBytes(framePath);

                // Create a texture and configure its settings
                Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0
                };
                tex.LoadImage(fileData);

                // Create a sprite from the texture
                Sprite frame = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    BASE_PPU,
                    0,
                    SpriteMeshType.Tight
                );

                frames.Add(frame);
            }

            // Only store characters with exactly 3 frames
            if (frames.Count == 3)
            {
                // Calculate character width adjustment
                float baseWidth = (64 / BASE_PPU) * entry.Width;

                // Store the character data in the dictionary
                loadedCharacters[entry.Character] = new CharacterData
                {
                    Frames = frames.ToArray(),
                    Width = baseWidth
                };
            }
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error loading {entry.Character}: {ex}");
        }
    }

    // Internal class to store character data
    private class CharacterData
    {
        public Sprite[] Frames;  // Array of sprite frames for animation
        public float Width;      // Width adjustment for the character
    }

    // Harmony patches for SpriteFont class
    [HarmonyPatch(typeof(SpriteFont))]
    private static class SpriteFontPatches
    {
        // Postfix patch for the Awake method in SpriteFont
        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void PostfixAwake(SpriteFont __instance)
        {
            try
            {
                // Access the private charDataMap field using Harmony's Traverse
                var charDataMap = Traverse.Create(__instance)
                    .Field<Dictionary<string, SpriteFont.CharData>>("charDataMap")
                    .Value;

                // Add custom characters to the character map
                foreach (var entry in loadedCharacters)
                {
                    if (!charDataMap.ContainsKey(entry.Key))
                    {
                        charDataMap.Add(entry.Key, new SpriteFont.CharData
                        {
                            character = entry.Key,
                            frames = entry.Value.Frames,
                            width = entry.Value.Width,
                            additionalCharacters = new string[0] // Preserve original structure
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LoggerInstance.LogError($"Awake patch failed: {e}");
            }
        }

        // Prefix patch for the GetCharData method in SpriteFont
        [HarmonyPrefix]
        [HarmonyPatch("GetCharData")]
        private static bool PrefixGetCharData(SpriteFont __instance, char character, ref SpriteFont.CharData __result)
        {
            try
            {
                // Check if the requested character is a custom one
                string key = character.ToString();
                if (loadedCharacters.TryGetValue(key, out CharacterData data))
                {
                    // Return the custom character data
                    __result = new SpriteFont.CharData
                    {
                        character = key,
                        frames = data.Frames,
                        width = data.Width,
                        additionalCharacters = new string[0]
                    };
                    return false; // Skip the original method
                }
            }
            catch (Exception e)
            {
                LoggerInstance.LogError($"GetCharData patch failed: {e}");
            }
            return true; // Continue to the original method if not custom
        }
    }
}
