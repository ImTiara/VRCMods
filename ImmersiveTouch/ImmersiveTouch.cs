using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using VRC.SDKBase;
using ImmersiveTouch.Extensions;

[assembly: MelonInfo(typeof(ImmersiveTouch.ImmersiveTouch), "ImmersiveTouch", "1.0.5", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmersiveTouch
{
    public class ImmersiveTouch : MelonMod
    {
        private static bool m_Enable;
        private static bool m_IgnoreSelf;
        private static bool m_IsCapable;

        private static float m_HapticAmplitude;
        private static float m_HapticDistance;

        private static Transform m_LeftWrist;
        private static Transform m_RightWrist;

        private static Vector3 m_PreviousLeftWristPosition;
        private static Vector3 m_PreviousRightWristPosition;

        public static readonly Dictionary<ulong, List<IntPtr>> m_RegistratedColliderPointers = new Dictionary<ulong, List<IntPtr>>();

        private static readonly List<IntPtr> m_LocalDynamicBonePointers = new List<IntPtr>();

        [ThreadStatic] static IntPtr m_CurrentDBI;

        private static GameObject m_CurrentAvatarObject;

        private static Animator m_CurrentAnimator;

        public override void OnApplicationStart()
            => MelonCoroutines.Start(UiManagerInitializer());

        public void OnUiManagerInit()
        {
            TurbonesEx.SetIsPresent();

            MelonPreferences.CreateCategory(GetType().Name, "Immersive Touch");
            MelonPreferences.CreateEntry(GetType().Name, "Enable", true, "Enable Immersive Touch");
            MelonPreferences.CreateEntry(GetType().Name, "HapticAmplitude", 100.0f, "Haptic Amplitude (%)");
            MelonPreferences.CreateEntry(GetType().Name, "IgnoreSelf", true, "Ignore Self Collisions");

            OnPreferencesSaved();

            Hooks.ApplyPatches();
        }

        public override void OnPreferencesSaved()
        {
            m_Enable = MelonPreferences.GetEntryValue<bool>(GetType().Name, "Enable");
            m_HapticAmplitude = MelonPreferences.GetEntryValue<float>(GetType().Name, "HapticAmplitude") / 100.0f;

            m_IgnoreSelf = MelonPreferences.GetEntryValue<bool>(GetType().Name, "IgnoreSelf");

            TryCapability();
        }

        public static unsafe void OnAvatarChanged(IntPtr instance, IntPtr __0, IntPtr __1)
        {
            Hooks.avatarChangedDelegate(instance, __0, __1);

            try
            {
                VRCAvatarManager avatarManager = new VRCAvatarManager(instance);
                if (avatarManager != null && avatarManager.GetInstanceID().Equals(Manager.GetLocalAvatarManager().GetInstanceID()))
                {
                    Animator animator = avatarManager.field_Private_Animator_0;
                    if (animator == null || !animator.isHuman) return;

                    float scale = Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.LeftHand).position, animator.GetBoneTransform(HumanBodyBones.RightHand).position);
                    m_HapticDistance = scale / 785.0f;

                    m_CurrentAvatarObject = avatarManager.prop_GameObject_0;
                    m_CurrentAnimator = animator;

                    TryCapability();
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error checking when avatar changed:\n{e}");
            }
        }

        // Credits to knah for this idea
        public static unsafe void OnUpdateParticles(IntPtr instance, bool __0)
        {
            m_CurrentDBI = instance;

            Hooks.updateParticlesDelegate(instance, __0);
        }

        public static unsafe void OnCollide(IntPtr instance, IntPtr particlePosition, float particleRadius)
        {
            void InvokeCollide() => Hooks.collideDelegate(instance, particlePosition, particleRadius);

            try
            {
                if (!m_IsCapable || (!m_RegistratedColliderPointers[1].Contains(instance) && !m_RegistratedColliderPointers[2].Contains(instance)))
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
            if (!TurbonesEx.isPresent) return;

            switch (TurbonesEx.GetAndClearCollidingGroupsMask())
            {
                case 1: SendHaptic(m_RegistratedColliderPointers[1][0]); break;
                case 2: SendHaptic(m_RegistratedColliderPointers[2][0]); break;
                case 3:
                    SendHaptic(m_RegistratedColliderPointers[1][0]);
                    SendHaptic(m_RegistratedColliderPointers[2][0]);
                    break;
            }
        }

        private static void SendHaptic(IntPtr instance)
        {
            if (m_IgnoreSelf && m_LocalDynamicBonePointers.Contains(m_CurrentDBI)) return;

            if (m_RegistratedColliderPointers[1].Contains(instance) && Vector3.Distance(m_PreviousLeftWristPosition, m_LeftWrist.position) > m_HapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.001f, m_HapticAmplitude, 0.001f);

                m_PreviousLeftWristPosition = m_LeftWrist.position;
            }

            if (m_RegistratedColliderPointers[2].Contains(instance) && Vector3.Distance(m_PreviousRightWristPosition, m_RightWrist.position) > m_HapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.001f, m_HapticAmplitude, 0.001f);

                m_PreviousRightWristPosition = m_RightWrist.position;
            }
        }

        private static void TryCapability()
        {
            if (TurbonesEx.isPresent) TurbonesEx.UnregisterTurbonesColliders();

            if (!m_Enable || Manager.GetLocalVRCPlayer() == null)
            {
                m_IsCapable = false;
                return;
            }

            try
            {
                if (m_CurrentAnimator == null || !m_CurrentAnimator.isHuman)
                {
                    NotCapable();
                    return;
                }

                m_RegistratedColliderPointers.Clear();
                m_RegistratedColliderPointers.Add(1, new List<IntPtr>());
                m_RegistratedColliderPointers.Add(2, new List<IntPtr>());

                foreach (var collider in Manager.GetDynamicBoneColliders(m_CurrentAnimator, HumanBodyBones.LeftHand))
                {
                    IntPtr pointer = collider.Pointer;

                    m_RegistratedColliderPointers[1].Add(pointer);

                    if (TurbonesEx.isPresent) TurbonesEx.RegisterColliderForCollisionFeedback(pointer, 0);
                }

                foreach (var collider in Manager.GetDynamicBoneColliders(m_CurrentAnimator, HumanBodyBones.RightHand))
                {
                    IntPtr pointer = collider.Pointer;

                    m_RegistratedColliderPointers[2].Add(pointer);

                    if (TurbonesEx.isPresent) TurbonesEx.RegisterColliderForCollisionFeedback(pointer, 1);
                }

                m_LeftWrist = m_CurrentAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                m_RightWrist = m_CurrentAnimator.GetBoneTransform(HumanBodyBones.RightHand);

                m_IsCapable = m_RegistratedColliderPointers[1].Count != 0 && m_RegistratedColliderPointers[2].Count != 0;

                if (m_IsCapable)
                {
                    m_LocalDynamicBonePointers.Clear();
                    foreach (var db in m_CurrentAvatarObject.GetDynamicBones())
                        m_LocalDynamicBonePointers.Add(db.Pointer);

                    MelonLogger.Msg($"This avatar is OK! Left count: {m_RegistratedColliderPointers[1].Count}. Right count: {m_RegistratedColliderPointers[2].Count}.");
                }
                else NotCapable();
            }
            catch (Exception e)
            {
                m_IsCapable = false;
                MelonLogger.Error($"Error when checking capability\n{e}");
            }

            static void NotCapable()
            {
                MelonLogger.Msg("This avatar is not capable for Immersive Touch.");
                m_IsCapable = false;
            }
        }

        public IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}