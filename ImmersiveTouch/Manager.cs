using System;
using System.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;
using VRC.SDKBase;

namespace ImmersiveTouch
{
    public static class Manager
    {
        public static VRCPlayer GetLocalVRCPlayer()
            => VRCPlayer.field_Internal_Static_VRCPlayer_0;

        public static VRCPlayerApi GetLocalVRCPlayerApi()
            => VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0;

        public static VRCAvatarManager GetLocalAvatarManager()
            => VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0;

        public static Il2CppArrayBase<DynamicBoneCollider> GetDynamicBoneColliders(this Animator animator, HumanBodyBones bone)
            => animator.GetBoneTransform(bone).GetComponentsInChildren<DynamicBoneCollider>(true);

        public static Il2CppArrayBase<DynamicBone> GetDynamicBones(this GameObject gameObject)
            => gameObject.GetComponentsInChildren<DynamicBone>(true);

        public static bool HasPointer(this List<DynamicBoneCollider> colliders, IntPtr pointer)
        {
            foreach (var collider in colliders)
                if (collider.Pointer == pointer) return true;
            return false;
        }

        public static bool HasPointer(this List<DynamicBone> bones, IntPtr pointer)
        {
            foreach (var collider in bones)
                if (collider.Pointer == pointer) return true;
            return false;
        }

        public static int CalculateLayerMask(bool allowWorld, bool allowPlayers)
            => (allowWorld ? (1 << 0) : 0) | (allowPlayers ? (1 << 9) : 0);
    }
}
