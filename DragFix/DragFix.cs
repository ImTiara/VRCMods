using HarmonyLib;
using MelonLoader;
using OVR.OpenVR;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using VRC.Core;

[assembly: MelonInfo(typeof(DragFix.DragFix), "DragFix", "1.0.2", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace DragFix
{
    public class DragFix : MelonMod
    {
        public static MelonPreferences_Entry<bool> ENABLE;
        public static MelonPreferences_Entry<float> VR_DRAG_THRESHOLD;
        public static MelonPreferences_Entry<float> DESKTOP_DRAG_THRESHOLD;

        public static float m_AvatarHeight = 1.6f;
        public static float m_AvatarHeightMod = 1.0f;
        public static float m_DragThreshold = 0.002f;

        public override void OnApplicationStart()
        {
            var category = MelonPreferences.CreateCategory("DragFix", "DragFix");
            ENABLE = category.CreateEntry("Enable", true, "Enable DragFix");
            VR_DRAG_THRESHOLD = category.CreateEntry("VRDragThreshold", 2.0f, "VR Drag Threshold");
            DESKTOP_DRAG_THRESHOLD = category.CreateEntry("DesktopDragThreshold", 2.0f, "Desktop Drag Threshold");

            VR_DRAG_THRESHOLD.OnValueChanged += (editedValue, defaultValue)
                => RefreshDragThreshold();
            
            DESKTOP_DRAG_THRESHOLD.OnValueChanged += (editedValue, defaultValue)
                => RefreshDragThreshold();

            RefreshDragThreshold();

            try
            {
                HarmonyInstance.Patch(typeof(PipelineManager).GetMethod(nameof(PipelineManager.Start)), null,
                    new HarmonyMethod(typeof(DragFix).GetMethod("OnAvatarChanged")));
            }
            catch (Exception e) { MelonLogger.Error("Failed to patch PipelineManager.Start: " + e); }

            try
            {
                HarmonyInstance.Patch(typeof(VRCStandaloneInputModule).GetMethod(nameof(VRCStandaloneInputModule.ProcessDrag)),
                    new HarmonyMethod(typeof(DragFix).GetMethod("ProcessDragPatch")));
            }
            catch (Exception e) { MelonLogger.Error("Failed to patch VRCStandaloneInputModule.ProcessDrag: " + e); }
        }

        public override void OnApplicationLateStart()
            => ScaleGoesBrrEx.Setup();
        
        public static void RefreshDragThreshold()
            => m_DragThreshold = OpenVR.System != null ? VR_DRAG_THRESHOLD.Value / 1000 : DESKTOP_DRAG_THRESHOLD.Value / 1000;
        
        public static void RefreshAvatarHeight()
            => m_AvatarHeight = VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.field_Private_VRC_AvatarDescriptor_0.ViewPosition.y * m_AvatarHeightMod;

        public static void OnAvatarChanged(ref PipelineManager __instance)
        {
            if (__instance.gameObject.Pointer != VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0.field_Private_GameObject_0.Pointer) return;

            m_AvatarHeightMod = 1.0f;

            RefreshAvatarHeight();
        }

        public static bool ProcessDragPatch(ref VRCStandaloneInputModule __instance, ref PointerEventData __0)
        {
            if (!ENABLE.Value) return true;
            
            bool isPointerMoving = __0.IsPointerMoving();
            if (isPointerMoving && __0.pointerDrag != null && !__0.dragging && ShouldStartDrag(__instance.field_Private_Dictionary_2_PointerEventData_Vector3_0[__0], __instance.field_Public_Vector3_0, __0.useDragThreshold))
            {
                ExecuteEvents.Execute(__0.pointerDrag, __0, ExecuteEvents.beginDragHandler);
                __0.dragging = true;
            }
            
            if (__0.dragging && isPointerMoving && __0.pointerDrag != null)
            {
                if (__0.pointerPress != __0.pointerDrag)
                {
                    ExecuteEvents.Execute(__0.pointerPress, __0, ExecuteEvents.pointerUpHandler);
                    __0.eligibleForClick = false;
                    __0.pointerPress = null;
                    __0.rawPointerPress = null;
                }
                ExecuteEvents.Execute(__0.pointerDrag, __0, ExecuteEvents.dragHandler);
            }

            return false;
        }

        public static bool ShouldStartDrag(Vector3 pressPos, Vector3 currentPos, bool useDragThreshold)
            => !useDragThreshold || (pressPos / m_AvatarHeight - currentPos / m_AvatarHeight).sqrMagnitude >= m_DragThreshold;
    }
}