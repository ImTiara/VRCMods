using HarmonyLib;
using MelonLoader;
using System;
using VRC.SDKBase.Validation.Performance;

[assembly: MelonInfo(typeof(NoPerformanceStats.NoPerformanceStats), "NoPerformanceStats", "1.0.8", "ImTiara", "https://github.com/ImTiara/VRCMods")]
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
                HarmonyInstance.Patch(typeof(AvatarPerformance).GetMethod(nameof(AvatarPerformance.GetPerformanceScannerSet)), new HarmonyMethod(typeof(NoPerformanceStats).GetMethod(nameof(GetPerformanceScannerSetPatch))));
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

        public static bool GetPerformanceScannerSetPatch()
            => allowPerformanceScanner;
    }
}