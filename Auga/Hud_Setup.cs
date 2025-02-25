﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using AugaUnity;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Auga
{
    [HarmonyPatch(typeof(Hud))]
    public static class Hud_Setup
    {
        [HarmonyPatch(nameof(Hud.Awake))]
        [HarmonyPostfix]
        public static void Hud_Awake_Postfix(Hud __instance)
        {
            __instance.Replace("hudroot/HotKeyBar", Auga.Assets.Hud, "hudroot/HotKeyBar");

            __instance.m_statusEffectListRoot = null;
            __instance.m_statusEffectTemplate = new GameObject("DummyStatusEffectTemplate", typeof(RectTransform)).RectTransform();
            __instance.Replace("hudroot/StatusEffects", Auga.Assets.Hud);

            __instance.m_saveIcon = __instance.Replace("hudroot/SaveIcon", Auga.Assets.Hud).gameObject;
            __instance.m_badConnectionIcon = __instance.Replace("hudroot/BadConnectionIcon", Auga.Assets.Hud).gameObject;

            var originalDreamTexts = __instance.m_sleepingProgress.GetComponent<SleepText>().m_dreamTexts;
            var loadingScreen = __instance.Replace("LoadingBlack", Auga.Assets.Hud);
            __instance.m_loadingScreen = loadingScreen.GetComponent<CanvasGroup>();
            __instance.m_loadingProgress = loadingScreen.Find("Loading").gameObject;
            __instance.m_sleepingProgress = loadingScreen.Find("Sleeping").gameObject;
            __instance.m_teleportingProgress = loadingScreen.Find("Teleporting").gameObject;
            __instance.m_loadingImage = loadingScreen.Find("Loading/Image").GetComponent<Image>();
            __instance.m_loadingTip = loadingScreen.Find("Loading/Tip").GetComponent<Text>();
            __instance.m_sleepingProgress.GetComponent<SleepText>().m_dreamTexts = originalDreamTexts;

            __instance.m_eventBar = __instance.Replace("hudroot/EventBar", Auga.Assets.Hud).gameObject;
            __instance.m_eventName = __instance.m_eventBar.GetComponentInChildren<Text>();

            __instance.m_damageScreen = __instance.Replace("hudroot/Damaged", Auga.Assets.Hud).GetComponent<Image>();

            var newCrosshair = __instance.Replace("hudroot/crosshair", Auga.Assets.Hud);
            __instance.m_crosshair = newCrosshair.Find("crosshair").GetComponent<Image>();
            __instance.m_crosshairBow = newCrosshair.Find("crosshair_bow").GetComponent<Image>();
            __instance.m_hoverName = newCrosshair.Find("Dummy/HoverName").GetComponent<Text>();
            __instance.m_pieceHealthRoot = (RectTransform) newCrosshair.Find("PieceHealthRoot");
            __instance.m_pieceHealthBar = newCrosshair.Find("PieceHealthRoot/PieceHealthBar").GetComponent<GuiBar>();
            __instance.m_targetedAlert = newCrosshair.Find("Sneak/Alert").gameObject;
            __instance.m_targeted = newCrosshair.Find("Sneak/Detected").gameObject;
            __instance.m_hidden = newCrosshair.Find("Sneak/Hidden").gameObject;
            __instance.m_stealthBar = newCrosshair.Find("Sneak/StealthBar").GetComponent<GuiBar>();

            var originalGuardianPowerMaterial = __instance.m_gpIcon.material;
            __instance.m_gpRoot = (RectTransform)__instance.Replace("hudroot/GuardianPower", Auga.Assets.Hud);
            __instance.m_gpName = __instance.m_gpRoot.Find("Name").GetComponent<Text>();
            __instance.m_gpIcon = __instance.m_gpRoot.Find("Icon").GetComponent<Image>();
            __instance.m_gpIcon.material = originalGuardianPowerMaterial;
            __instance.m_gpCooldown = __instance.m_gpRoot.Find("TimeText").GetComponent<Text>();

            var newHealthPanel = __instance.Replace("hudroot/healthpanel", Auga.Assets.Hud);
            Object.Destroy(__instance.m_staminaBar2Root.gameObject);
            __instance.m_healthBarRoot = null;
            __instance.m_healthPanel = null;
            __instance.m_healthAnimator = newHealthPanel.Find("HealthBar").GetComponent<Animator>();
            __instance.m_healthBarFast = null;
            __instance.m_healthBarSlow = null;
            __instance.m_healthText = null;
            __instance.m_foodBars = new Image[0];
            __instance.m_foodIcons = new Image[0];
            __instance.m_foodBarRoot = null;
            __instance.m_foodBaseBar = null;
            __instance.m_foodText = null;
            __instance.m_staminaAnimator = newHealthPanel.Find("StaminaBar").GetComponent<Animator>();
            __instance.m_staminaBar2Root = null;
            __instance.m_staminaBar2Fast = null;
            __instance.m_staminaBar2Slow = null;

            __instance.m_actionBarRoot = __instance.Replace("hudroot/action_progress", Auga.Assets.Hud).gameObject;
            __instance.m_actionName = __instance.m_actionBarRoot.GetComponentInChildren<Text>();
            __instance.m_actionProgress = __instance.m_actionBarRoot.GetComponent<GuiBar>();

            var newStaggerPanel = __instance.Replace("hudroot/staggerpanel", Auga.Assets.Hud);
            __instance.m_staggerAnimator = newStaggerPanel.GetComponent<Animator>();
            __instance.m_staggerProgress = newStaggerPanel.Find("staggerbar/RightBar/Background/FillMask/FillFast").GetComponent<GuiBar>();

            // Setup the icon material to grayscale the piece icons
            var iconMaterial = __instance.m_pieceIconPrefab.transform.Find("icon").GetComponent<Image>().material;
            Auga.Assets.BuildHudElement.transform.Find("icon").GetComponent<Image>().material = iconMaterial;

            __instance.m_buildHud = __instance.Replace("hudroot/BuildHud/", Auga.Assets.Hud).gameObject;
            var tabContainer = __instance.m_buildHud.transform.Find("BuildHud/DividerLarge/Tabs");
            __instance.m_pieceCategoryTabs = new[] {
                tabContainer.Find("Misc").gameObject,
                tabContainer.Find("Crafting").gameObject,
                tabContainer.Find("Building").gameObject,
                tabContainer.Find("Furniture").gameObject,
            };
            Localization.instance.Localize(tabContainer);

            for (var index = 0; index < __instance.m_pieceCategoryTabs.Length; index++)
            {
                var categoryTab = __instance.m_pieceCategoryTabs[index];
                var i = index;
                categoryTab.GetComponent<Button>().onClick.AddListener(() => SetBuildCategory(i));
            }

            __instance.m_pieceSelectionWindow = __instance.m_buildHud.transform.Find("BuildHud").gameObject;
            __instance.m_pieceCategoryRoot = __instance.m_buildHud.transform.Find("BuildHud/DividerLarge").gameObject;
            __instance.m_pieceListRoot = (RectTransform)__instance.m_buildHud.transform.Find("BuildHud/PieceList/Root");
            __instance.m_pieceListMask = null;
            __instance.m_pieceIconPrefab = Auga.Assets.BuildHudElement;
            __instance.m_closePieceSelectionButton = __instance.m_buildHud.transform.Find("CloseButton").GetComponent<UIInputHandler>();

            var selectedPiece = __instance.m_buildHud.transform.Find("SelectedPiece");
            __instance.m_buildSelection = selectedPiece.Find("Name").GetComponent<Text>();
            __instance.m_pieceDescription = selectedPiece.Find("Info").GetComponent<Text>();
            __instance.m_buildIcon = selectedPiece.Find("Darken/IconBG/PieceIcon").GetComponent<Image>();
            var requirements = selectedPiece.Find("Requirements");
            __instance.m_requirementItems = new [] {
                requirements.GetChild(0).gameObject,
                requirements.GetChild(1).gameObject,
                requirements.GetChild(2).gameObject,
                requirements.GetChild(3).gameObject,
                requirements.GetChild(4).gameObject,
                requirements.GetChild(5).gameObject,
            };

            __instance.transform.Replace("hudroot/KeyHints", Auga.Assets.Hud);

            var shipHud = __instance.transform.Replace("hudroot/ShipHud", Auga.Assets.Hud);
            __instance.m_shipHudRoot = shipHud.gameObject;
            __instance.m_shipControlsRoot = shipHud.Find("Controls").gameObject;
            __instance.m_rudderLeft = shipHud.Find("Dummy").gameObject;
            __instance.m_rudderRight = shipHud.Find("Dummy").gameObject;
            __instance.m_rudderSlow = shipHud.Find("WindIndicator/Ship/Slow").gameObject;
            __instance.m_rudderForward = shipHud.Find("WindIndicator/Ship/Forward").gameObject;
            __instance.m_rudderFastForward = shipHud.Find("WindIndicator/Ship/FastForward").gameObject;
            __instance.m_rudderBackward = shipHud.Find("WindIndicator/Ship/Backward").gameObject;
            __instance.m_halfSail = shipHud.Find("PowerIcon/SailRotation/HalfSail").gameObject;
            __instance.m_fullSail = shipHud.Find("PowerIcon/SailRotation/FullSail").gameObject;
            __instance.m_rudder = shipHud.Find("PowerIcon/Rudder").gameObject;
            __instance.m_shipWindIndicatorRoot = (RectTransform)shipHud.Find("WindIndicator");
            __instance.m_shipWindIcon = shipHud.Find("WindIndicator/Wind/WindIcon").GetComponent<Image>();
            __instance.m_shipWindIconRoot = (RectTransform)shipHud.Find("WindIndicator/Wind");
            __instance.m_shipRudderIndicator = shipHud.Find("Controls/RudderIndicatorBG/RudderIndicatorMask/RudderIndicator").GetComponent<Image>();
            __instance.m_shipRudderIcon = shipHud.Find("Controls/RudderIcon").GetComponent<Image>();

            Localization.instance.Localize(__instance.transform);
        }

        private static GameObject _leftWristMountUI;

        [HarmonyPatch(nameof(Hud.Update))]
        [HarmonyPostfix]
        private static void Hud_Update_Postfix()
        {
            var player = Player.m_localPlayer;
            if (Auga.UseAugaVR.Value && player != null && _leftWristMountUI == null)
            {
                var leftForearm = player.transform.Find("Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm");
                if (leftForearm != null)
                {
                    _leftWristMountUI = Object.Instantiate(Auga.Assets.LeftWristMountUI, leftForearm, false);
                    var canvas = _leftWristMountUI.GetComponentInChildren<Canvas>();
                    canvas.worldCamera = Camera.main;
                    Debug.LogWarning("Created left wrist mount UI!");
                }
                else
                {
                    Debug.LogWarning("No LeftForeArm!");
                }
            }

            if (player == null && _leftWristMountUI != null)
            {
                Object.Destroy(_leftWristMountUI);
                _leftWristMountUI = null;
            }
        }

        [HarmonyPatch(nameof(Hud.UpdateStatusEffects))]
        [HarmonyPrefix]
        public static bool Hud_UpdateStatusEffects_Prefix()
        {
            return false;
        }

        [HarmonyPatch(nameof(Hud.UpdateFood))]
        [HarmonyPrefix]
        public static bool Hud_UpdateFood_Prefix()
        {
            return false;
        }

        [HarmonyPatch(nameof(Hud.SetHealthBarSize))]
        [HarmonyPrefix]
        public static bool Hud_SetHealthBarSize_Prefix()
        {
            return false;
        }

        [HarmonyPatch(nameof(Hud.SetStaminaBarSize))]
        [HarmonyPrefix]
        public static bool Hud_SetStaminaBarSize_Prefix()
        {
            return false;
        }

        [HarmonyPatch(nameof(Hud.UpdateHealth))]
        [HarmonyPrefix]
        public static bool Hud_UpdateHealth_Prefix()
        {
            return false;
        }

        [HarmonyPatch(nameof(Hud.UpdateStamina))]
        [HarmonyPrefix]
        public static bool Hud_UpdateStamina_Prefix()
        {
            return false;
        }

        public static void SetBuildCategory(int index)
        {
            if (Player.m_localPlayer != null)
            {
                Player.m_localPlayer.SetBuildCategory(index);
            }
        }
    }

    [HarmonyPatch(typeof(HotkeyBar), nameof(HotkeyBar.UpdateIcons))]
    public static class HotkeyBar_UpdateIcons_Patch
    {
        public static void Postfix(HotkeyBar __instance)
        {
            if (Player.m_localPlayer == null || Player.m_localPlayer.IsDead())
            {
                return;
            }

            for (var index = 0; index < __instance.m_items.Count; ++index)
            {
                var itemData = __instance.m_items[index];
                if (itemData.m_gridPos.x < 0 || itemData.m_gridPos.x >= __instance.m_elements.Count)
                {
                    continue;
                }

                var element = __instance.m_elements[itemData.m_gridPos.x];
                var itemTooltip = element.m_go.GetComponent<ItemTooltip>();
                if (itemTooltip != null)
                {
                    itemTooltip.Item = itemData;
                }
            }
        }
    }

    // Hud.UpdateShipHud (0.145 either way)
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateShipHud))]
    public static class Hud_UpdateShipHud_Patch
    {
        public const float RudderMinMax = 0.145f;

        public static void Postfix(Hud __instance, Player player, float dt)
        {
            var ship = player.GetControlledShip();
            if (ship == null || !__instance.m_shipRudderIndicator.gameObject.activeSelf)
            {
                return;
            }

            var rudderValue = ship.GetRudderValue();
            if (rudderValue > 0)
            {
                __instance.m_shipRudderIndicator.fillClockwise = true;
                __instance.m_shipRudderIndicator.fillAmount = rudderValue * RudderMinMax;
            }
            else
            {
                __instance.m_shipRudderIndicator.fillClockwise = false;
                __instance.m_shipRudderIndicator.fillAmount = -rudderValue * RudderMinMax;
            }
        }
    }

    //public void SetupPieceInfo(Piece piece)
    [HarmonyPatch(typeof(Hud), nameof(Hud.SetupPieceInfo))]
    public static class Hud_SetupPieceInfo_Patch
    {
        public static void Postfix(Hud __instance, Piece piece)
        {
            __instance.m_pieceDescription.gameObject.SetActive(!string.IsNullOrEmpty(__instance.m_pieceDescription.text));

            var requireItemsContainer = __instance.m_requirementItems[0].transform.parent;
            requireItemsContainer.gameObject.SetActive(piece.m_resources?.Length > 0);
        }
    }

    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateCrosshair))]
    public static class Hud_UpdateCrosshair_Patch
    {
        public static Transform AugaHoverText;
        public static Text HoverTextPrefab;
        public static GameObject HoverTextWithButtonPrefab;
        public static GameObject HoverTextWithButtonRangePrefab;

        private static string _lastHoverText;
        private static readonly Dictionary<string, string> _cachedKeyNames = new Dictionary<string, string>();

        public static void Postfix(Hud __instance)
        {
            if (HoverTextPrefab == null)
            {
                AugaHoverText = __instance.m_crosshair.transform.parent.Find("AugaHoverText");
                HoverTextPrefab = __instance.m_hoverName;
                HoverTextWithButtonPrefab = __instance.m_crosshair.transform.parent.Find("Dummy/HoverNameWithButton").gameObject;
                HoverTextWithButtonRangePrefab = __instance.m_crosshair.transform.parent.Find("Dummy/HoverNameWithButtonRange").gameObject;
            }

            if (!string.IsNullOrEmpty(__instance.m_hoverName.text))
            {
                ColorUtility.TryParseHtmlString(Auga.Colors.BrightestGold, out var brightestGold);
                __instance.m_crosshair.color = brightestGold;
            }
            else
            {
                __instance.m_crosshair.color = new Color(1f, 1f, 1f, 0.5f);
            }

            AugaHoverText.gameObject.SetActive(__instance.m_hoverName.gameObject.activeSelf);

            if (_lastHoverText != __instance.m_hoverName.text)
            {
                _lastHoverText = __instance.m_hoverName.text;
                foreach (Transform child in AugaHoverText)
                {
                    Object.Destroy(child.gameObject);
                }

                var parts = _lastHoverText.Split('\n');
                foreach (var part in parts)
                {
                    if (part.StartsWith("["))
                    {
                        var closingBracketIndex = part.IndexOf(']');
                        if (closingBracketIndex > 0)
                        {
                            var textInBracket = part.Substring(1, closingBracketIndex - 1);
                            textInBracket = textInBracket.Replace("<color=yellow>", "").Replace("<b>", "").Replace("</b>", "").Replace("</color>", "").Trim();
                            var otherText = part.Substring(closingBracketIndex + 1).Trim();

                            if (textInBracket == "1-8")
                            {
                                var lineWithRange = Object.Instantiate(HoverTextWithButtonRangePrefab, AugaHoverText, false);
                                lineWithRange.SetActive(true);
                                var bindings = lineWithRange.GetComponentsInChildren<AugaBindingDisplay>();
                                bindings[0].SetText("1");
                                bindings[1].SetText("8");
                                var text = lineWithRange.transform.Find("Text").GetComponent<Text>();
                                text.text = otherText;

                                continue;
                            }

                            if (_cachedKeyNames.Count == 0)
                            {
                                foreach (var buttonEntry in ZInput.instance.m_buttons)
                                {
                                    var bindingDisplay = Localization.instance.GetBoundKeyString(buttonEntry.Key);
                                    if (!_cachedKeyNames.ContainsKey(bindingDisplay))
                                    {
                                        _cachedKeyNames.Add(bindingDisplay, buttonEntry.Key);
                                    }
                                }
                            }

                            if (_cachedKeyNames.TryGetValue(textInBracket, out var keyName))
                            {
                                var lineWithBinding = Object.Instantiate(HoverTextWithButtonPrefab, AugaHoverText, false);
                                lineWithBinding.SetActive(true);
                                var binding = lineWithBinding.GetComponentInChildren<AugaBindingDisplay>();
                                binding.SetBinding(keyName);
                                var text = lineWithBinding.transform.Find("Text").GetComponent<Text>();
                                text.text = otherText;
                                continue;
                            }
                        }
                    }

                    var line = Object.Instantiate(HoverTextPrefab, AugaHoverText, false);
                    line.gameObject.SetActive(true);
                    line.text = part;
                }
            }
        }

        [HarmonyPatch(typeof(ZInput), nameof(ZInput.Save))]
        public static class ZInput_Save_Patch
        {
            public static void Postfix()
            {
                _cachedKeyNames.Clear();
            }
        }
    }

    //StaminaBarNoStaminaFlash
    [HarmonyPatch(typeof(Hud), nameof(Hud.StaminaBarNoStaminaFlash))]
    public static class Hud_StaminaBarNoStaminaFlash_Patch
    {
        private const float FlashDuration = 0.15f;
        private static Coroutine _staminaFlashCoroutine;

        public static bool Prefix(Hud __instance)
        {
            if (_staminaFlashCoroutine != null)
            {
                __instance.StopCoroutine(_staminaFlashCoroutine);
            }

            _staminaFlashCoroutine = __instance.StartCoroutine(FlashStaminaBar(__instance));
            return false;
        }

        private static IEnumerator FlashStaminaBar(Hud instance)
        {
            var staminaBarBorder = instance.m_staminaAnimator.transform.Find("Border").GetComponent<Image>();

            for (var i = 0; i < 3; i++)
            {
                staminaBarBorder.color = Color.white;
                yield return new WaitForSeconds(FlashDuration);
                staminaBarBorder.color = Color.black;
                yield return new WaitForSeconds(FlashDuration);
            }
        }
    }

    //UpdateBuild
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdateBuild))]
    public static class Hud_UpdateBuild_Patch
    {
        [UsedImplicitly]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldstr && instruction.OperandIs(" [<color=yellow>"))
                {
                    yield return new CodeInstruction(OpCodes.Ldstr, $" [<color={Auga.Colors.BrightestGold}>");
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    //UpdatePieceList
    [HarmonyPatch(typeof(Hud), nameof(Hud.UpdatePieceList))]
    public static class Hud_UpdatePieceList_Patch
    {
        public static bool Prefix(Hud __instance, Player player, Vector2Int selectedNr, Piece.PieceCategory category, bool updateAllBuildStatuses)
        {
            var buildPieces = player.GetBuildPieces();
            var pieceIcons = __instance.m_pieceIcons;
            var selectedIndex = selectedNr.x + selectedNr.y * 13;
            selectedNr.x = selectedIndex % 10;
            selectedNr.y = selectedIndex / 10;

            var i = 0;
            for (; i < buildPieces.Count; ++i)
            {
                if (i >= pieceIcons.Count)
                {
                    // Create icon
                    var icon = Object.Instantiate(__instance.m_pieceIconPrefab, __instance.m_pieceListRoot);
                    var pieceIconData = new Hud.PieceIconData();
                    pieceIconData.m_go = icon;
                    pieceIconData.m_tooltip = icon.GetComponent<UITooltip>();
                    pieceIconData.m_icon = icon.transform.Find("icon").GetComponent<Image>();
                    pieceIconData.m_marker = icon.transform.Find("selected").gameObject;
                    pieceIconData.m_upgrade = icon.transform.Find("upgrade").gameObject;
                    pieceIconData.m_icon.color = new Color(1f, 0.0f, 1f, 0.0f);
                    var component = icon.GetComponent<UIInputHandler>();
                    component.m_onLeftDown += __instance.OnLeftClickPiece;
                    component.m_onRightDown += __instance.OnRightClickPiece;
                    component.m_onPointerEnter += __instance.OnHoverPiece;
                    component.m_onPointerExit += __instance.OnHoverPieceExit;
                    pieceIcons.Add(pieceIconData);
                }

                // Update icon
                var pieceIcon = pieceIcons[i];
                pieceIcon.m_marker.SetActive(i == selectedIndex);

                var piece = buildPieces[i];
                pieceIcon.m_icon.sprite = piece.m_icon;
                pieceIcon.m_icon.enabled = true;
                pieceIcon.m_tooltip.m_text = piece.m_name;
                pieceIcon.m_upgrade.SetActive(piece.m_isUpgrade);
            }

            for (; i < pieceIcons.Count; ++i)
            {
                Object.Destroy(pieceIcons[i].m_go);
                pieceIcons[i] = null;
            }

            pieceIcons.RemoveAll(x => x == null);

            __instance.UpdatePieceBuildStatus(buildPieces, player);
            if (updateAllBuildStatuses)
            {
                __instance.UpdatePieceBuildStatusAll(buildPieces, player);
            }

            if (__instance.m_lastPieceCategory == category)
            {
                return false;
            }

            __instance.m_lastPieceCategory = category;
            __instance.m_pieceBarPosX = __instance.m_pieceBarTargetPosX;
            __instance.UpdatePieceBuildStatusAll(buildPieces, player);

            return false;
        }
    }

    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.PrevCategory))]
    public static class PieceTable_PrevCategory_Patch
    {
        public static bool Prefix(ref PieceTable __instance)
        {
            return Input.GetAxis("Mouse ScrollWheel") == 0;
        }
    }

    [HarmonyPatch(typeof(PieceTable), nameof(PieceTable.NextCategory))]
    public static class PieceTable_NextCategory_Patch
    {
        public static bool Prefix(ref PieceTable __instance)
        {
            return Input.GetAxis("Mouse ScrollWheel") == 0;
        }
    }
}
