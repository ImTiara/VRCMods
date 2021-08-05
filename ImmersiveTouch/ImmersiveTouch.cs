using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using VRC.SDKBase;
using ImmersiveTouch.Extensions;

[assembly: MelonInfo(typeof(ImmersiveTouch.ImmersiveTouch), "ImmersiveTouch", "1.0.7", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmersiveTouch
{
    public class ImmersiveTouch : MelonMod
    {
        private static bool m_Enable;
        private static bool m_IgnoreSelf;

        private static float m_HapticAmplitude;
        private static float m_HapticSensitivity;

        private static bool isCapable;

        private static float hapticDistance;

        private static Transform leftWrist;
        private static Transform rightWrist;

        private static Vector3 previousLeftWristPosition;
        private static Vector3 previousRightWristPosition;

        public static readonly Dictionary<int, List<IntPtr>> registratedColliderPtrs = new Dictionary<int, List<IntPtr>>();

        public static readonly List<IntPtr> localDynamicBonePtrs = new List<IntPtr>();
        [ThreadStatic] static IntPtr currentDBI;

        private static GameObject currentAvatarObject;
        private static Animator currentAnimator;
        private static float currentViewHeight;

        public override void OnApplicationStart()
            => MelonCoroutines.Start(UiManagerInitializer());

        public void OnUiManagerInit()
        {
            TurbonesEx.SetIsPresent();

            MelonPreferences.CreateCategory(GetType().Name, "Immersive Touch");
            MelonPreferences.CreateEntry(GetType().Name, "Enable", true, "Enable Immersive Touch");
            MelonPreferences.CreateEntry(GetType().Name, "HapticAmplitude", 100.0f, "Haptic Amplitude (%)");
            MelonPreferences.CreateEntry(GetType().Name, "HapticSensitivity", 70.0f, "Haptic Sensitivity");
            MelonPreferences.CreateEntry(GetType().Name, "IgnoreSelf", true, "Ignore Self Collisions");

            OnPreferencesSaved();

            Hooks.ApplyPatches();
        }

        public override void OnPreferencesSaved()
        {
            m_Enable = MelonPreferences.GetEntryValue<bool>(GetType().Name, "Enable");
            m_HapticAmplitude = MelonPreferences.GetEntryValue<float>(GetType().Name, "HapticAmplitude") / 100.0f;
            m_HapticSensitivity = MelonPreferences.GetEntryValue<float>(GetType().Name, "HapticSensitivity") * 10;
            m_IgnoreSelf = MelonPreferences.GetEntryValue<bool>(GetType().Name, "IgnoreSelf");

            TryCapability();
        }

        public static void OnAvatarChanged(VRCAvatarManager __instance)
        {
            try
            {
                if (__instance.Pointer != Manager.GetLocalAvatarManager().Pointer) return;

                currentAvatarObject = __instance.prop_GameObject_0;
                currentAnimator = __instance.field_Private_Animator_0;
                currentViewHeight = __instance.field_Private_VRC_AvatarDescriptor_0.ViewPosition.y;

                TryCapability();
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error checking when avatar changed:\n{e}");
            }
        }

        // Credits to knah for this idea
        public static unsafe void OnUpdateParticles(IntPtr instance, bool __0)
        {
            currentDBI = instance;

            Hooks.updateParticlesDelegate(instance, __0);
        }

        public static unsafe void OnCollide(IntPtr instance, IntPtr particlePosition, float particleRadius)
        {
            void InvokeCollide() => Hooks.collideDelegate(instance, particlePosition, particleRadius);

            try
            {
                if (!isCapable || (!registratedColliderPtrs[1].Contains(instance) && !registratedColliderPtrs[2].Contains(instance)))
                {
                    InvokeCollide();
                    return;
                }

                // Store the original particle position and invoke the original method.
                Vector3 prevParticlePos = Marshal.PtrToStructure<Vector3>(particlePosition);
                InvokeCollide();

                // If the particle position was changed after the invoke, we have a collision!
                if (!prevParticlePos.Equals(Marshal.PtrToStructure<Vector3>(particlePosition)))
                {
                    SendHaptic(instance);
                }
            }
            catch
            {
                InvokeCollide();
            }
        }

        public override void OnUpdate()
        {
            if (!TurbonesEx.isPresent || !isCapable) return;

            ulong mask = TurbonesEx.GetAndClearCollidingGroupsMask();

            if ((mask & 1) != 0) SendHaptic(registratedColliderPtrs[1][0]);
            if ((mask & 2) != 0) SendHaptic(registratedColliderPtrs[2][0]);
        }

        private static void SendHaptic(IntPtr instance)
        {
            if (m_IgnoreSelf && localDynamicBonePtrs.Contains(currentDBI)) return;

            if (registratedColliderPtrs[1].Contains(instance) && Vector3.Distance(previousLeftWristPosition, leftWrist.position) > hapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.001f, m_HapticAmplitude, 0.001f);

                previousLeftWristPosition = leftWrist.position;
            }

            if (registratedColliderPtrs[2].Contains(instance) && Vector3.Distance(previousRightWristPosition, rightWrist.position) > hapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.001f, m_HapticAmplitude, 0.001f);

                previousRightWristPosition = rightWrist.position;
            }
        }

        private static void TryCapability()
        {
            currentDBI = IntPtr.Zero;

            if (TurbonesEx.isPresent)
            {
                TurbonesEx.UnregisterCollisionFeedbackColliders();
                TurbonesEx.UnregisterExcludedBonesFromCollisionFeedback();
            }

            if (!m_Enable || Manager.GetLocalVRCPlayer() == null)
            {
                isCapable = false;
                return;
            }

            try
            {
                if (currentAnimator == null || !currentAnimator.isHuman)
                {
                    NotCapable();
                    return;
                }

                hapticDistance = currentViewHeight / m_HapticSensitivity;

                registratedColliderPtrs.Clear();
                registratedColliderPtrs.Add(1, new List<IntPtr>());
                registratedColliderPtrs.Add(2, new List<IntPtr>());

                foreach (var collider in Manager.GetDynamicBoneColliders(currentAnimator, HumanBodyBones.LeftHand))
                {
                    IntPtr pointer = collider.Pointer;

                    registratedColliderPtrs[1].Add(pointer);

                    if (TurbonesEx.isPresent) TurbonesEx.RegisterColliderForCollisionFeedback(pointer, 0);
                }

                foreach (var collider in Manager.GetDynamicBoneColliders(currentAnimator, HumanBodyBones.RightHand))
                {
                    IntPtr pointer = collider.Pointer;

                    registratedColliderPtrs[2].Add(pointer);

                    if (TurbonesEx.isPresent) TurbonesEx.RegisterColliderForCollisionFeedback(pointer, 1);
                }

                leftWrist = currentAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                rightWrist = currentAnimator.GetBoneTransform(HumanBodyBones.RightHand);

                isCapable = registratedColliderPtrs[1].Count != 0 && registratedColliderPtrs[2].Count != 0;

                if (isCapable)
                {
                    localDynamicBonePtrs.Clear();
                    foreach (var db in currentAvatarObject.GetDynamicBones())
                    {
                        IntPtr pointer = db.Pointer;

                        localDynamicBonePtrs.Add(pointer);

                        if (TurbonesEx.isPresent && m_IgnoreSelf) TurbonesEx.ExcludeBoneFromCollisionFeedback(pointer);
                    }

                    MelonLogger.Msg($"This avatar is OK! Left collider count: {registratedColliderPtrs[1].Count}. Right collider count: {registratedColliderPtrs[2].Count}.");
                }
                else NotCapable();
            }
            catch (Exception e)
            {
                isCapable = false;
                MelonLogger.Error($"Error when checking capability\n{e}");
            }

            static void NotCapable()
            {
                MelonLogger.Msg("This avatar is not capable for Immersive Touch.");
                isCapable = false;
            }
        }

        public IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}