using HarmonyLib;
using MelonLoader;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine.UI;
using VRC.SDKBase.Validation.Performance;

[assembly: MelonInfo(typeof(NoPerformanceStats.NoPerformanceStats), "NoPerformanceStats", "1.0.5", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace NoPerformanceStats
{
    public class NoPerformanceStats : MelonMod
    {
        private static readonly HarmonyLib.Harmony _harmonyInstance = new HarmonyLib.Harmony("NoPerformanceStatsPatcher");

        private static bool allowPerformanceScanner;

        private static Text avatarStatsButtonText;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory(GetType().Name, "No Performance Stats");
            MelonPreferences.CreateEntry(GetType().Name, "DisablePerformanceStats", true, "Disable Performance Stats");

            ApplyPatches();

            MelonCoroutines.Start(UiManagerInitializer());
        }

        private void OnUiManagerInit()
        {
            avatarStatsButtonText = VRCUiManager.prop_VRCUiManager_0.transform.Find("MenuContent/Screens/Avatar/Stats Button/Text").GetComponent<Text>();

            OnPreferencesSaved();
        }

        public override void OnPreferencesSaved()
        {
            allowPerformanceScanner = !MelonPreferences.GetEntryValue<bool>(GetType().Name, "DisablePerformanceStats");

            RefreshPerfStuff();
        }

        private void ApplyPatches()
        {
            try
            {
                Patch(
                    typeof(PerformanceScannerSet),
                    nameof(PerformanceScannerSet.Method_Public_IEnumerator_GameObject_AvatarPerformanceStats_MulticastDelegateNPublicSealedBoCoUnique_0),

                    typeof(NoPerformanceStats),
                    nameof(CalculatePerformance),

                    BindingFlags.Public | BindingFlags.Instance,
                    BindingFlags.NonPublic | BindingFlags.Static
                );
            }
            catch (Exception e) { MelonLogger.Error("Failed to patch Performance Scanner: " + e); }
        }

        private static void RefreshPerfStuff()
        {
            avatarStatsButtonText.text = allowPerformanceScanner ? "Avatar Stats" : "<color=#ff6464>Stats Disabled!</color>";
        }

        private static bool CalculatePerformance()
            => allowPerformanceScanner;

        private static void Patch(Type targetClass, string target, Type detourClass, string detour, BindingFlags targetBindingFlags = BindingFlags.Public | BindingFlags.Instance, BindingFlags detourBindingFlags = BindingFlags.NonPublic | BindingFlags.Static)
            => _harmonyInstance.Patch(targetClass.GetMethod(target, targetBindingFlags), new HarmonyMethod(detourClass.GetMethod(detour, detourBindingFlags)), null, null);

        private IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}