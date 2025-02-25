﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using fastJSON;
using HarmonyLib;
using UnityEngine;

namespace Auga
{
    public class AugaAssets
    {
        public GameObject AugaLogo;
        public GameObject InventoryScreen;
        public GameObject Hud;
        public Texture2D Cursor;
        public GameObject MenuPrefab;
        public GameObject TextViewerPrefab;
        public GameObject MainMenuPrefab;
        public GameObject BuildHudElement;
        public GameObject SettingsPrefab;
        public GameObject MessageHud;
        public GameObject TextInput;
        public GameObject AugaChat;
        public GameObject DamageText;
        public GameObject EnemyHud;
        public GameObject StoreGui;
        public GameObject WorldListElement;
        public GameObject ServerListElement;
        public GameObject PasswordDialog;
        public GameObject ConnectingDialog;
        public GameObject PanelBase;
        public GameObject ButtonSmall;
        public GameObject ButtonMedium;
        public GameObject ButtonFancy;
        public GameObject ButtonSettings;
        public GameObject DiamondButton;
        public Font SourceSansProBold;
        public Font SourceSansProSemiBold;
        public Font SourceSansProRegular;
        public Sprite ItemBackgroundSprite;
        public GameObject InventoryTooltip;
        public GameObject SimpleTooltip;
        public GameObject DividerSmall;
        public GameObject DividerMedium;
        public GameObject DividerLarge;
        public GameObject ConfirmDialog;

        public GameObject LeftWristMountUI;
    }

    public class AugaColors
    {
        public string BrightestGold = "#FFBF1B";
        public string Topic = "#EAA800";
        public string Emphasis = "#1AACEF";
        public Color Healing = new Color(0.5f, 1.0f, 0.5f, 0.7f);
        public Color PlayerDamage = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        public Color PlayerNoDamage = new Color(0.5f, 0.5f, 0.5f, 1f);
        public Color NormalDamage = new Color(1f, 1f, 1f, 1f);
        public Color ResistDamage = new Color(0.6f, 0.6f, 0.6f, 1f);
        public Color WeakDamage = new Color(1f, 1f, 0.0f, 1f);
        public Color ImmuneDamage = new Color(0.6f, 0.6f, 0.6f, 1f);
        public Color TooHard = new Color(0.8f, 0.7f, 0.7f, 1f);
    }

    [BepInPlugin(PluginID, PluginName, Version)]
    public class Auga : BaseUnityPlugin
    {
        public const string PluginID = "randyknapp.mods.auga";
        public const string PluginName = "Project Auga";
        public const string Version = "1.2.0";
        public const string FallbackLanguage = "English";

        private static ConfigEntry<bool> _loggingEnabled;
        private static ConfigEntry<LogLevel> _logLevel;
        public static ConfigEntry<bool> UseAugaTrash;
        public static ConfigEntry<bool> UseAugaVR;

        public static readonly AugaAssets Assets = new AugaAssets();
        public static readonly AugaColors Colors = new AugaColors();

        public static bool HasBetterTrader = false;

        private static Auga _instance;
        private Harmony _harmony;

        public void Awake()
        {
            _instance = this;

            LoadDependencies();
            LoadTranslations();
            LoadConfig();
            LoadAssets();

            ApplyCursor();

            HasBetterTrader = gameObject.GetComponent("BetterTrader") != null;

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginID);
        }

        public void OnDestroy()
        {
            _harmony.UnpatchSelf();
            _instance = null;
        }

        private void LoadDependencies()
        {
            var assembly = Assembly.GetCallingAssembly();
            LoadEmbeddedAssembly(assembly, "fastJSON.dll");
            LoadEmbeddedAssembly(assembly, "Unity.Auga.dll");
        }

        private static void LoadEmbeddedAssembly(Assembly assembly, string assemblyName)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{assembly.GetName().Name}.{assemblyName}");
            if (stream == null)
            {
                LogError($"Could not load embedded assembly ({assemblyName})!");
                return;
            }

            using (stream)
            {
                var data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
                Assembly.Load(data);
            }
        }

        private static void LoadTranslations()
        {
            try
            {
                // get the game selected language
                string selectedLanguage = Localization.instance.GetSelectedLanguage();
                string languageLoaded = selectedLanguage;

                var translationsJsonText = LoadJsonText($"translations/{selectedLanguage}.json");
                if (string.IsNullOrEmpty(translationsJsonText))
                {
                    // fallback language : English by default
                    languageLoaded = FallbackLanguage;
                    translationsJsonText = LoadJsonText($"translations/{FallbackLanguage}.json");
                    if (string.IsNullOrEmpty(translationsJsonText))
                    {
                        return; // in case that even the English translations could not be found
                    }
                }

                var translations = (IDictionary<string, object>)JSON.Parse(translationsJsonText);
                foreach (var translation in translations)
                {
                    if (!string.IsNullOrEmpty(translation.Key) && !string.IsNullOrEmpty(translation.Value.ToString()))
                    {
                        Localization.instance.AddWord(translation.Key, translation.Value.ToString());
                    }
                }

                _instance.Logger.LogMessage($"Auga language loaded: {languageLoaded}");
            }
            catch (Exception ex)
            {
                _instance.Logger.LogError($"Auga language loading error : {ex.Message} : {ex.StackTrace}");
            }
        }

        private void LoadConfig()
        {
            _loggingEnabled = Config.Bind("Logging", "LoggingEnabled", false, "Enable logging");
            _logLevel = Config.Bind("Logging", "LogLevel", LogLevel.Info, "Only log messages of the selected level or higher");
            UseAugaTrash = Config.Bind("Options", "UseAugaTrash", false, "Enable Auga's built in trash button. Click on the button while holding an item or part of a stack with the mouse.");
            UseAugaVR = Config.Bind("Options", "UseAugaVR", false, "Enable Auga's custom Valheim VR mode.");
        }

        private static void LoadAssets()
        {
            var assetBundle = LoadAssetBundle("augaassets");
            Assets.AugaLogo = assetBundle.LoadAsset<GameObject>("AugaLogo");
            Assets.InventoryScreen = assetBundle.LoadAsset<GameObject>("Inventory_screen");
            Assets.Cursor = assetBundle.LoadAsset<Texture2D>("Cursor2");
            Assets.MenuPrefab = assetBundle.LoadAsset<GameObject>("AugaMenu");
            Assets.TextViewerPrefab = assetBundle.LoadAsset<GameObject>("AugaTextViewer");
            Assets.Hud = assetBundle.LoadAsset<GameObject>("HUD");
            Assets.MainMenuPrefab = assetBundle.LoadAsset<GameObject>("MainMenu");
            Assets.BuildHudElement = assetBundle.LoadAsset<GameObject>("BuildHudElement");
            Assets.SettingsPrefab = assetBundle.LoadAsset<GameObject>("AugaSettings");
            Assets.MessageHud = assetBundle.LoadAsset<GameObject>("AugaMessageHud");
            Assets.TextInput = assetBundle.LoadAsset<GameObject>("AugaTextInput");
            Assets.AugaChat = assetBundle.LoadAsset<GameObject>("AugaChat");
            Assets.DamageText = assetBundle.LoadAsset<GameObject>("AugaDamageText");
            Assets.EnemyHud = assetBundle.LoadAsset<GameObject>("AugaEnemyHud");
            Assets.StoreGui = assetBundle.LoadAsset<GameObject>("AugaStoreScreen");
            Assets.WorldListElement = assetBundle.LoadAsset<GameObject>("WorldListElement");
            Assets.ServerListElement = assetBundle.LoadAsset<GameObject>("ServerListElement");
            Assets.PasswordDialog = assetBundle.LoadAsset<GameObject>("AugaPassword");
            Assets.ConnectingDialog = assetBundle.LoadAsset<GameObject>("AugaConnecting");
            Assets.PanelBase = assetBundle.LoadAsset<GameObject>("AugaPanelBase");
            Assets.ButtonSmall = assetBundle.LoadAsset<GameObject>("ButtonSmall");
            Assets.ButtonMedium = assetBundle.LoadAsset<GameObject>("ButtonMedium");
            Assets.ButtonFancy = assetBundle.LoadAsset<GameObject>("ButtonFancy");
            Assets.ButtonSettings = assetBundle.LoadAsset<GameObject>("ButtonSettings");
            Assets.DiamondButton = assetBundle.LoadAsset<GameObject>("DiamondButton");
            Assets.SourceSansProBold = assetBundle.LoadAsset<Font>("SourceSansPro-Bold");
            Assets.SourceSansProSemiBold = assetBundle.LoadAsset<Font>("SourceSansPro-SemiBold");
            Assets.SourceSansProRegular = assetBundle.LoadAsset<Font>("SourceSansPro-Regular");
            Assets.ItemBackgroundSprite = assetBundle.LoadAsset<Sprite>("Container_Square_A");
            Assets.InventoryTooltip = assetBundle.LoadAsset<GameObject>("InventoryTooltip");
            Assets.SimpleTooltip = assetBundle.LoadAsset<GameObject>("SimpleTooltip");
            Assets.DividerSmall = assetBundle.LoadAsset<GameObject>("DividerSmall");
            Assets.DividerMedium = assetBundle.LoadAsset<GameObject>("DividerMedium");
            Assets.DividerLarge = assetBundle.LoadAsset<GameObject>("DividerLarge");
            Assets.ConfirmDialog = assetBundle.LoadAsset<GameObject>("ConfirmDialog");
            Assets.LeftWristMountUI = assetBundle.LoadAsset<GameObject>("LeftWristMountUI");
        }

        private static void ApplyCursor()
        {
            Cursor.SetCursor(Assets.Cursor, new Vector2(6, 5), CursorMode.Auto);
        }

        public static AssetBundle LoadAssetBundle(string filename)
        {
            // Optionally load asset bundle from path, if it exists
            var assetBundlePath = GetAssetPath(filename);
            if (!string.IsNullOrEmpty(assetBundlePath))
            {
                return AssetBundle.LoadFromFile(assetBundlePath);
            }

            var assembly = Assembly.GetCallingAssembly();
            var assetBundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{filename}"));

            return assetBundle;
        }

        public static string LoadJsonText(string filename)
        {
            var jsonFileName = GetAssetPath(filename);
            return !string.IsNullOrEmpty(jsonFileName) ? File.ReadAllText(jsonFileName) : null;
        }

        public static string GetAssetPath(string assetName)
        {
            var assetFileName = Path.Combine(Paths.PluginPath, "Auga", assetName);
            if (!File.Exists(assetFileName))
            {
                var assembly = typeof(Auga).Assembly;
                assetFileName = Path.Combine(Path.GetDirectoryName(assembly.Location) ?? string.Empty, assetName);
                if (!File.Exists(assetFileName))
                {
                    LogError($"Could not find asset ({assetName})");
                    return null;
                }
            }

            return assetFileName;
        }

        public static void Log(string message)
        {
            if (_loggingEnabled.Value && _logLevel.Value <= LogLevel.Info)
            {
                _instance.Logger.LogInfo(message);
            }
        }

        public static void LogWarning(string message)
        {
            if (_loggingEnabled.Value && _logLevel.Value <= LogLevel.Warning)
            {
                _instance.Logger.LogWarning(message);
            }
        }

        public static void LogError(string message)
        {
            if (_loggingEnabled.Value && _logLevel.Value <= LogLevel.Error)
            {
                _instance.Logger.LogError(message);
            }
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    public static class Terminal_InitTerminal_Patch
    {
        public static void Postfix()
        {
            new Terminal.ConsoleCommand("resetbiomes", "", args =>
            {
                Player.m_localPlayer.m_knownBiome = new HashSet<Heightmap.Biome>();
            });
        }
    }
}
