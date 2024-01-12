using HarmonyLib;
using System;
using TMPro;
using UnityEngine;

namespace ScruffyRuffles.LC.BatteryText
{
    [HarmonyPatch]
    class BatteryTextPatches
    {
        private static GameObject template;
        public static TextMeshProUGUI[] batteryTexts = new TextMeshProUGUI[0];

        [HarmonyPatch(typeof(HUDManager), "Update")]
        [HarmonyPostfix]
        public static void Update(HUDManager __instance)
        {
            //if (GameNetworkManager.Instance.localPlayerController != __instance) return;
            for (int i = 0; i < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; i++)
            {
                if (i >= batteryTexts.Length)
                {
                    Array.Resize(ref batteryTexts, i + 1);
                }
                if (batteryTexts[i] == null)
                {
                    var parentObj = __instance.itemSlotIconFrames[i].rectTransform;
                    var itemObj = GameObject.Instantiate(template, parentObj.parent);
                    itemObj.SetActive(true);
                    itemObj.transform.localScale = 0.5f * Vector3.one;
                    batteryTexts[i] = itemObj.GetComponent<TextMeshProUGUI>();
                    var rectTransform = batteryTexts[i].GetComponent<RectTransform>();

                    // For some reason each Slot has a different rotation, so we have to do some offsetting from parent
                    rectTransform.anchorMin = rectTransform.anchorMax = 0.5f * Vector3.one;
                    rectTransform.pivot = Vector3.one;
                    float halfSize = parentObj.sizeDelta.x * 0.5f;
                    rectTransform.anchoredPosition = parentObj.anchoredPosition + new Vector2(halfSize, -halfSize);
                    rectTransform.anchoredPosition += new Vector2(Plugin.config.offsetX, Plugin.config.offsetY);
                    rectTransform.SetParent(parentObj, true);
                }
                GrabbableObject grabbableObject = GameNetworkManager.Instance.localPlayerController.ItemSlots[i];
                if (grabbableObject != null)
                {
                    if (grabbableObject.itemProperties.requiresBattery)
                    {
                        batteryTexts[i].text = Mathf.Floor(grabbableObject.insertedBattery.charge * 100f).ToString() + "%";
                        if (Plugin.config.showTimeRemaining)
                        {
                            batteryTexts[i].text += "\n" + TimeSpan.FromSeconds(grabbableObject.itemProperties.batteryUsage * grabbableObject.insertedBattery.charge).ToString(@"mm\:ss");
                        }
                    }
                    else
                    {
                        batteryTexts[i].text = "";
                    }
                }
                else
                {
                    batteryTexts[i].text = "";
                }
            }
        }

        [HarmonyPatch(typeof(HUDManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake(HUDManager __instance)
        {
            if (__instance.weightCounter != null)
            {
                GameObject itemSlot = new($"ItemSlotBatteryText")
                {
                    layer = LayerMask.NameToLayer("UI")
                };
                itemSlot.SetActive(false);

                var tmpugui = itemSlot.AddComponent<TextMeshProUGUI>();
                tmpugui.font = __instance.weightCounter.font;
                tmpugui.fontSize = __instance.weightCounter.fontSize;
                tmpugui.color = __instance.batteryIcon.color;
                tmpugui.alignment = TextAlignmentOptions.TopRight;
                tmpugui.enableAutoSizing = __instance.weightCounter.enableAutoSizing;
                tmpugui.fontSizeMin = __instance.weightCounter.fontSizeMin;
                tmpugui.fontSizeMax = __instance.weightCounter.fontSizeMax;

                template = itemSlot;
            }
        }
    }
}
