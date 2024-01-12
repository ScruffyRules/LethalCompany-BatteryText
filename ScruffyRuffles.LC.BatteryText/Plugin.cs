using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using System.IO;
using System;
using UnityEngine;

namespace ScruffyRuffles.LC.BatteryText
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "ScruffyRuffles.BatteryText";
        private const string modName = "BatteryText";
        private const string modVersion = "1.0.0";

        public static BepInEx.Logging.ManualLogSource PluginLogger;
        public static ConfigA config;

        private void Awake()
        {
            PluginLogger = Logger;
            // Plugin startup logic
            PluginLogger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            // Initialize Harmony
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

            config = new ConfigA(this);
        }

        public class ConfigA
        {
            private readonly ConfigFile config;
            private readonly FileSystemWatcher watcher;
            private readonly ConfigEntry<bool> _showTimeRemaining;
            private readonly ConfigEntry<float> _offsetX;
            private readonly ConfigEntry<float> _offsetY;

            public bool showTimeRemaining => _showTimeRemaining.Value;
            public float offsetX => _offsetX.Value;
            public float offsetY => _offsetY.Value;
            public ConfigA(Plugin plugin)
            {
                config = new ConfigFile(Path.Combine(BepInEx.Paths.ConfigPath, modGUID + ".cfg"), saveOnInit: true, MetadataHelper.GetMetadata(plugin));

                _showTimeRemaining = config.Bind("General", "Show Time Remaining", true, "Shows the remaining time under the percentage");
                _offsetX = config.Bind("General", "Offset X", 0.0f, "Position Offset on the X Axis");
                _offsetY = config.Bind("General", "Offset Y", -2.0f, "Position Offset on the Y Axis");

                watcher = new FileSystemWatcher(Path.GetDirectoryName(config.ConfigFilePath), "*.cfg")
                {
                    NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size
                };

                watcher.Changed += ConfigFileChanged;
                watcher.EnableRaisingEvents = true;
            }

            private void OnConfigReloaded()
            {
                for (int i = 0; i < BatteryTextPatches.batteryTexts.Length; i++)
                {
                    var rectTransform = BatteryTextPatches.batteryTexts[i].GetComponent<RectTransform>();
                    var parentObj = rectTransform.parent.GetComponent<RectTransform>();
                    rectTransform.SetParent(parentObj.parent);
                    rectTransform.anchoredPosition = parentObj.anchoredPosition + (new Vector2(parentObj.sizeDelta.x * 0.5f, -parentObj.sizeDelta.x * 0.5f) * parentObj.localScale);
                    rectTransform.anchoredPosition += new Vector2(Plugin.config.offsetX, Plugin.config.offsetY) * parentObj.localScale;
                    rectTransform.SetParent(parentObj, true);
                }
            }

            private void ConfigFileChanged(object sender, FileSystemEventArgs e)
            {
                //PluginLogger.LogInfo($"{e.ChangeType} : {e.FullPath}");
                if (e.FullPath != config.ConfigFilePath) return;
                //PluginLogger.LogInfo("Config File Modified, Reloading!");
                config.Reload();
                OnConfigReloaded();
            }
        }
    }
}