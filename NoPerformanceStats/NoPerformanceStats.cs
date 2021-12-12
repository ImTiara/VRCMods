using HarmonyLib;
using MelonLoader;
using System;
using VRC.SDKBase.Validation.Performance;

[assembly: MelonInfo(typeof(NoPerformanceStats.NoPerformanceStats), "NoPerformanceStats", "1.0.7", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace NoPerformanceStats
{
    public class NoPerformanceStats : MelonMod
    {
        public static MelonLogger.Instance Logger;

        private static bool allowPerformanceScanner;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance(GetType().Name);

            MelonPreferences.CreateCategory(GetType().Name, "No Performance Stats");
            MelonPreferences.CreateEntry(GetType().Name, "DisablePerformanceStats", true, "Disable Performance Stats");

            OnPreferencesSaved();

            try
            {
                HarmonyInstance.Patch(typeof(PerformanceScannerSet).GetMethod(nameof(PerformanceScannerSet.Method_Public_IEnumerator_GameObject_AvatarPerformanceStats_MulticastDelegateNPublicSealedBoCoUnique_0)),
                    new HarmonyMethod(typeof(NoPerformanceStats).GetMethod("CalculatePerformance")));
            }
            catch (Exception e) { Logger.Error("Failed to patch Performance Scanner: " + e); }
        }

        public override void OnPreferencesSaved()
        {
            bool disablePerformanceStats = MelonPreferences.GetEntryValue<bool>(GetType().Name, "DisablePerformanceStats");

            if (disablePerformanceStats == allowPerformanceScanner)
            {
                allowPerformanceScanner = !disablePerformanceStats;

                Logger.Msg($"Performance stats will now be {(allowPerformanceScanner ? "shown" : "hidden and avatars should load quicker")}.");
            }
        }

        public static bool CalculatePerformance()
            => allowPerformanceScanner;
    }
}