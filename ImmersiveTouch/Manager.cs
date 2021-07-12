using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnhollowerBaseLib;
using UnityEngine;
using VRC.SDKBase;

namespace ImmersiveTouch
{
    public static class Manager
    {
        public static VRCPlayer GetLocalVRCPlayer() => VRCPlayer.field_Internal_Static_VRCPlayer_0;

        public static VRCPlayerApi GetLocalVRCPlayerApi() => GetLocalVRCPlayer()?.prop_VRCPlayerApi_0;

        public static VRCAvatarManager GetLocalAvatarManager() => GetLocalVRCPlayer()?.prop_VRCAvatarManager_0;

        public static GameObject GetLocalAvatarObject() => GetLocalAvatarManager()?.prop_GameObject_0;

        public static Animator GetLocalAvatarAnimator() => GetLocalAvatarManager()?.field_Private_Animator_0;

        public static Il2CppArrayBase<DynamicBoneCollider> GetDynamicBoneColliders(this Animator animator, HumanBodyBones bone) => animator.GetBoneTransform(bone).GetComponentsInChildren<DynamicBoneCollider>(true);

        public static Il2CppArrayBase<DynamicBone> GetDynamicBones(this GameObject gameObject) => gameObject.GetComponentsInChildren<DynamicBone>(true);

        #region UIExpansionKit
        public static void UIExpansionKit_RegisterSettingAsStringEnum(string categoryName, string settingName, IList<(string SettingsValue, string DisplayName)> possibleValues)
        {
            if (_UIExpansionKit == null) return;

            _UIExpansionKit_RegisterSettingAsStringEnum.Invoke(null, new object[] { categoryName, settingName, possibleValues });
        }

        public static void RegisterUIExpansionKit()
        {
            _UIExpansionKit = MelonHandler.Mods.FirstOrDefault(x => x.Info.Name == "UI Expansion Kit")?.Assembly;

            if (_UIExpansionKit == null)
            {
                MelonLogger.Warning("For the best experience, it is highly recommended to use UIExpansionKit.");
                return;
            }

            _UIExpansionKit_RegisterSettingAsStringEnum = _UIExpansionKit.GetType("UIExpansionKit.API.ExpansionKitApi").GetMethod("RegisterSettingAsStringEnum");
        }

        private static Assembly _UIExpansionKit;
        private static MethodInfo _UIExpansionKit_RegisterSettingAsStringEnum;
        #endregion

        #region Turbones
        public static void Turbones_RegisterColliderForCollisionFeedbackDelegate(IntPtr colliderPtr, byte group)
        {
            if (_Turbones == null) return;

            _Turbones_RegisterColliderForCollisionFeedbackDelegate.Invoke(null, new object[] { colliderPtr, group });
        }

        public static void RegisterTurbones()
        {
            _Turbones = MelonHandler.Mods.FirstOrDefault(x => x.Info.Name == "Turbones")?.Assembly;

            if (_Turbones == null) return;

            _Turbones_RegisterColliderForCollisionFeedbackDelegate = _Turbones.GetType("Turbones.JigglySolverApi").GetMethod("RegisterColliderForCollisionFeedbackDelegate");
        }

        private static Assembly _Turbones;
        private static MethodInfo _Turbones_RegisterColliderForCollisionFeedbackDelegate;
        #endregion
    }
}
