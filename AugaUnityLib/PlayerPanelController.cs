using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EpicLoot;

namespace AugaUnity
{
    public class PlayerPanelController : MonoBehaviour
    {
        [Header("Colors")]
        public Color HighlightColor1 = Color.white;
        public Color HighlightColor2 = Color.white;

        [Header("Stats data")]
        public Text HealthText;
        public Text HealthTextSecondary;
        public Text StaminaText;
        public Text StaminaTextSecondary;

        [Header("Combat data")]
        public Text AttackText;
        public Text AttackTextSecondary;
        public Text ArmorText;
        public Text ArmorTextSecondary;

        protected string _highlightColor1;
        protected string _highlightColor2;

        public virtual void Start()
        {
            _highlightColor1 = ColorUtility.ToHtmlStringRGB(HighlightColor1);
            _highlightColor2 = ColorUtility.ToHtmlStringRGB(HighlightColor2);
            Update();
        }

        public virtual void Update()
        {
            var player = Player.m_localPlayer;
            if (player == null)
            {
                return;
            }

            try
            {
                UpdateHealth(player);
                UpdateStamina(player);
                UpdateAttack(player);
                UpdateArmor(player);
            }
            catch (Exception ex)
            {
                ZLog.Log(ex);
            }
        }

        public void UpdateHealth(Player player)
        {
            var healthDisplay = Mathf.CeilToInt(player.GetHealth());
            HealthText.text = $"<color={_highlightColor1}>{healthDisplay}</color> / {Mathf.CeilToInt(player.GetMaxHealth())}";

            player.GetTotalFoodValue(out var hp, out var stamina);
            var healthFoodDisplay = Mathf.CeilToInt(hp - player.m_baseHP);
            HealthTextSecondary.text = Localization.instance.Localize("$status_desc", _highlightColor2, player.m_baseHP.ToString("0"), _highlightColor2, healthFoodDisplay.ToString());
        }

        public void UpdateStamina(Player player)
        {
            var staminaDisplay = Mathf.CeilToInt(player.GetStamina());
            StaminaText.text = $"<color={_highlightColor1}>{staminaDisplay}</color> / {Mathf.CeilToInt(player.GetMaxStamina())}";

            player.GetTotalFoodValue(out var hp, out var stamina);
            var staminaFoodDisplay = Mathf.CeilToInt(stamina - player.m_baseStamina);
            StaminaTextSecondary.text = Localization.instance.Localize("$status_desc", _highlightColor2, player.m_baseStamina.ToString("0"), _highlightColor2, staminaFoodDisplay.ToString());
        }

        public void UpdateAttack(Player player)
        {
            float physicalDamage = Mathf.CeilToInt(
                player.GetCurrentWeapon().GetDamage().m_blunt + 
                player.GetCurrentWeapon().GetDamage().m_pierce + 
                player.GetCurrentWeapon().GetDamage().m_slash
            );

            Dictionary<string, string> effectChecks = new Dictionary<string, string>();
            effectChecks.Add(MagicEffectType.AddPoisonDamage, "$element_poison");
            effectChecks.Add(MagicEffectType.AddLightningDamage, "$element_lightning");
            effectChecks.Add(MagicEffectType.AddSpiritDamage, "$element_spirit");
            effectChecks.Add(MagicEffectType.AddFireDamage, "$element_fire");
            effectChecks.Add(MagicEffectType.AddFrostDamage, "$element_frost");

            List<string> elementalDamageList = new List<string>();
            foreach (var item in player.GetInventory().GetEquipedtems())
            {
                if (item.IsMagic(out MagicItem magicItem))
                {
                    foreach (var effectCheck in effectChecks)
                    {
                        try
                        {
                            if (magicItem.HasEffect(effectCheck.Key))
                            {
                                elementalDamageList.Add(Localization.instance.Localize(
                                    "$elemental_desc",
                                    Localization.instance.Localize(effectCheck.Value),
                                    $"{magicItem.GetTotalEffectValue(effectCheck.Key)}"
                                ));
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }

            AttackText.text = $"<color={_highlightColor1}>{physicalDamage.ToString("0")}</color>";
            AttackTextSecondary.text = String.Join(", ", elementalDamageList);
        }

        public void UpdateArmor(Player player)
        {
            float armorDisplay = Mathf.CeilToInt(player.GetBodyArmor());
            float blockDisplay = Mathf.CeilToInt(player.GetCurrentBlocker().GetBlockPower(player.GetSkillFactor(Skills.SkillType.Blocking)));

            ArmorText.text = Localization.instance.Localize("$armor_desc", _highlightColor1, armorDisplay.ToString("0"));
            ArmorTextSecondary.text = Localization.instance.Localize("$blocking_desc", _highlightColor1, blockDisplay.ToString("0"));
        }
    }
}
