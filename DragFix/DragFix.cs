using HarmonyLib;
using MelonLoader;
using OVR.OpenVR;
using System;
using UnityEngine;
using VRC.Core;

[assembly: MelonInfo(typeof(DragFix.DragFix), "DragFix", "1.0.2", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace DragFix
{
    public class DragFix : MelonMod
    {
        public static MelonLogger.Instance Logger;

        public static bool m_Enable;

        public static float m_AvatarHeight;
        public static float m_DragThreshold;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance(GetType().Name);

            MelonPreferences.CreateCategory(GetType().Name, "DragFix");
            MelonPreferences.CreateEntry(GetType().Name, "Enable", true, "Enable DragFix");
            MelonPreferences.CreateEntry(GetType().Name, "VRDragThreshold", 2.0f, "VR Drag Threshold");
            MelonPreferences.CreateEntry(GetType().Name, "DesktopDragThreshold", 2.0f, "Desktop Drag Threshold");

            OnPreferencesSaved();

            try
            {
                HarmonyInstance.Patch(typeof(PipelineManager).GetMethod(nameof(PipelineManager.Start)), null,
                    new HarmonyMethod(typeof(DragFix).GetMethod("OnAvatarChanged")));
            }
            catch (Exception e) { Logger.Error("Failed to patch AvatarChanged: " + e); }

            try
            {
                HarmonyInstance.Patch(typeof(VRCStandaloneInputModule).GetMethod(nameof(VRCStandaloneInputModule.Method_Private_Static_Boolean_Vector3_Vector3_Single_Boolean_0)),
                    new HarmonyMethod(typeof(DragFix).GetMethod("ShouldStartDragPatch")));
            }
            catch (Exception e) { Logger.Error("Failed to patch ShouldStartDrag: " + e); }
        }

        public override void OnPreferencesSaved()
        {
            m_Enable = MelonPreferences.GetEntryValue<bool>(GetType().Name, "Enable");

            float vrDragThreshold = MelonPreferences.GetEntryValue<float>(GetType().Name, "VRDragThreshold") / 1000;
            float desktopDragThreshold = MelonPreferences.GetEntryValue<float>(GetType().Name, "DesktopDragThreshold") / 1000;

            m_DragThreshold = OpenVR.System != null ? vrDragThreshold : desktopDragThreshold;
        }

        public static void OnAvatarChanged(ref PipelineManager __instance)
        {
            if (__instance.gameObject.Pointer != VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0.field_Private_GameObject_0.Pointer) return;

            m_AvatarHeight = VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.field_Private_VRC_AvatarDescriptor_0.ViewPosition.y;
        }

        public static bool ShouldStartDragPatch(ref bool __result, Vector3 __0, Vector3 __1, bool __3)
        {
            if (!m_Enable) return true;

            __result = !__3 || (__0 / m_AvatarHeight - __1 / m_AvatarHeight).sqrMagnitude >= m_DragThreshold;
            return false;
        }
    }
}