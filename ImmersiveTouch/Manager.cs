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
            => GetLocalVRCPlayer()?.prop_VRCPlayerApi_0;

        public static VRCAvatarManager GetLocalAvatarManager()
            => GetLocalVRCPlayer()?.prop_VRCAvatarManager_0;

        public static GameObject GetLocalAvatarObject()
            => GetLocalAvatarManager()?.prop_GameObject_0;

        public static Animator GetLocalAvatarAnimator()
            => GetLocalAvatarManager()?.field_Private_Animator_0;

        public static Il2CppArrayBase<DynamicBoneCollider> GetDynamicBoneColliders(this Animator animator, HumanBodyBones bone)
            => animator.GetBoneTransform(bone).GetComponentsInChildren<DynamicBoneCollider>(true);

        public static Il2CppArrayBase<DynamicBone> GetDynamicBones(this GameObject gameObject)
            => gameObject.GetComponentsInChildren<DynamicBone>(true);
    }
}
