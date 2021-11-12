using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using VRC.SDKBase;
using ImmersiveTouch.Extensions;

[assembly: MelonInfo(typeof(ImmersiveTouch.ImmersiveTouch), "ImmersiveTouch", "1.1.0", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmersiveTouch
{
    public class ImmersiveTouch : MelonMod
    {
        public static HarmonyLib.Harmony harmony;

        private static bool m_Enable;
        private static bool m_ColliderHaptic;
        private static bool m_ColliderHapticIgnoreSelf;
        private static bool m_MeshHaptic;
        private static bool m_MeshHapticWorld;
        private static bool m_MeshHapticPlayers;

        private static float m_HapticAmplitude;
        private static float m_HapticSensitivity;

        private static bool isColliderHapticCapable;

        private static float hapticDistance;

        private static Transform leftWrist;
        private static Transform rightWrist;

        private static Vector3 previousLeftWristPosition;
        private static Vector3 previousRightWristPosition;

        public static readonly List<DynamicBoneCollider> allRegistratedColliders = new List<DynamicBoneCollider>();
        public static readonly List<DynamicBoneCollider> registratedLeftColliders = new List<DynamicBoneCollider>();
        public static readonly List<DynamicBoneCollider> registratedRightColliders = new List<DynamicBoneCollider>();

        public static readonly List<DynamicBone> localDynamicBones = new List<DynamicBone>();

        [ThreadStatic]
        public static IntPtr currentDBI;

        public static GameObject currentAvatarObject;
        public static Animator currentAnimator;
        public static float currentViewHeight;

        public override void OnApplicationStart()
        {
            harmony = HarmonyInstance;

            MelonCoroutines.Start(UiManagerInitializer());
        }

        public void OnUiManagerInit()
        {
            TurbonesEx.SetIsPresent();
            MeshHapticEx.Setup();

            MelonPreferences.CreateCategory(GetType().Name, "Immersive Touch");
            MelonPreferences.CreateEntry(GetType().Name, "Enable", true, "Enable Immersive Touch");
            MelonPreferences.CreateEntry(GetType().Name, "HapticAmplitude", 100.0f, "Haptic Amplitude (%)");
            MelonPreferences.CreateEntry(GetType().Name, "HapticSensitivity", 70.0f, "Haptic Sensitivity");
            MelonPreferences.CreateEntry(GetType().Name, "ColliderHaptic", true, "Collider Haptic");
            MelonPreferences.CreateEntry(GetType().Name, "IgnoreSelf", false, "Ignore Self Collisions");
            MelonPreferences.CreateEntry(GetType().Name, "MeshHaptic", false, "Mesh Haptic");
            MelonPreferences.CreateEntry(GetType().Name, "MeshHapticWorld", true, "Mesh Haptic World");
            MelonPreferences.CreateEntry(GetType().Name, "MeshHapticPlayers", true, "Mesh Haptic Players");

            OnPreferencesSaved();

            Hooks.ApplyPatches(TurbonesEx.isPresent);
        }

        public override void OnPreferencesSaved()
        {
            m_Enable = MelonPreferences.GetEntryValue<bool>(GetType().Name, "Enable");
            m_HapticAmplitude = MelonPreferences.GetEntryValue<float>(GetType().Name, "HapticAmplitude") / 100.0f;
            m_HapticSensitivity = MelonPreferences.GetEntryValue<float>(GetType().Name, "HapticSensitivity") * 10;
            m_ColliderHaptic = MelonPreferences.GetEntryValue<bool>(GetType().Name, "ColliderHaptic");
            m_ColliderHapticIgnoreSelf = MelonPreferences.GetEntryValue<bool>(GetType().Name, "IgnoreSelf");
            m_MeshHaptic = MelonPreferences.GetEntryValue<bool>(GetType().Name, "MeshHaptic");
            m_MeshHapticWorld = MelonPreferences.GetEntryValue<bool>(GetType().Name, "MeshHapticWorld");
            m_MeshHapticPlayers = MelonPreferences.GetEntryValue<bool>(GetType().Name, "MeshHapticPlayers");

            MeshHapticEx.cullingMask = Manager.CalculateLayerMask(m_MeshHapticWorld, m_MeshHapticPlayers);

            SetupAvatar();
        }

        public static void OnAvatarChanged(VRCAvatarManager __instance)
        {
            try
            {
                if (__instance.Pointer != Manager.GetLocalAvatarManager().Pointer) return;

                currentAvatarObject = __instance.prop_GameObject_0;
                currentAnimator = __instance.field_Private_Animator_0;
                currentViewHeight = __instance.field_Private_VRC_AvatarDescriptor_0.ViewPosition.y;

                SetupAvatar();
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
                if (!isColliderHapticCapable || (!allRegistratedColliders.HasPointer(instance)))
                {
                    InvokeCollide();
                    return;
                }

                // Store the original particle position and invoke the original method.
                Vector3 prevParticlePos = Marshal.PtrToStructure<Vector3>(particlePosition);
                InvokeCollide();

                // If the particle position was changed after the invoke, we have a collision!
                if (prevParticlePos != Marshal.PtrToStructure<Vector3>(particlePosition))
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
            if (!TurbonesEx.isPresent || !isColliderHapticCapable) return;

            ulong mask = TurbonesEx.GetAndClearCollidingGroupsMask();

            if ((mask & 1) != 0) SendHaptic(registratedLeftColliders[0].Pointer);
            if ((mask & 2) != 0) SendHaptic(registratedRightColliders[0].Pointer);
        }

        public static void SendHaptic(IntPtr collider)
        {
            if (m_ColliderHapticIgnoreSelf && currentDBI != IntPtr.Zero && localDynamicBones.HasPointer(currentDBI)) return;

            if (registratedLeftColliders.HasPointer(collider) && leftWrist != null && Vector3.Distance(previousLeftWristPosition, leftWrist.position) > hapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.001f, m_HapticAmplitude, 0.001f);

                previousLeftWristPosition = leftWrist.position;
            }

            if (registratedRightColliders.HasPointer(collider) && rightWrist != null && Vector3.Distance(previousRightWristPosition, rightWrist.position) > hapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.001f, m_HapticAmplitude, 0.001f);

                previousRightWristPosition = rightWrist.position;
            }
        }

        public static void SendHaptic(Transform wrist)
        {
            if (leftWrist != null && wrist == leftWrist && Vector3.Distance(previousLeftWristPosition, leftWrist.position) > hapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, 0.001f, m_HapticAmplitude, 0.001f);

                previousLeftWristPosition = leftWrist.position;
            }

            if (rightWrist != null && wrist == rightWrist && Vector3.Distance(previousRightWristPosition, rightWrist.position) > hapticDistance)
            {
                Manager.GetLocalVRCPlayerApi().PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, 0.001f, m_HapticAmplitude, 0.001f);

                previousRightWristPosition = rightWrist.position;
            }
        }

        private static void SetupAvatar()
        {
            isColliderHapticCapable = false;
            currentDBI = IntPtr.Zero;

            allRegistratedColliders.Clear();
            registratedLeftColliders.Clear();
            registratedRightColliders.Clear();
            localDynamicBones.Clear();

            if (TurbonesEx.isPresent)
            {
                TurbonesEx.UnregisterCollisionFeedbackColliders();
                TurbonesEx.UnregisterExcludedBonesFromCollisionFeedback();
            }

            MeshHapticEx.Destroy();

            if (!m_Enable || Manager.GetLocalVRCPlayer() == null) return;

            try
            {
                if (currentAnimator == null || !currentAnimator.isHuman)
                {
                    FailedCapabilityResult("Invalid avatar animator.");
                    return;
                }

                hapticDistance = currentViewHeight / m_HapticSensitivity;

                leftWrist = currentAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                rightWrist = currentAnimator.GetBoneTransform(HumanBodyBones.RightHand);

                if (m_ColliderHaptic)
                {
                    foreach (var collider in leftWrist.GetComponentsInChildren<DynamicBoneCollider>(true))
                    {
                        registratedLeftColliders.Add(collider);

                        if (TurbonesEx.isPresent) TurbonesEx.RegisterColliderForCollisionFeedback(collider.Pointer, 0);
                    }

                    foreach (var collider in rightWrist.GetComponentsInChildren<DynamicBoneCollider>(true))
                    {
                        registratedRightColliders.Add(collider);

                        if (TurbonesEx.isPresent) TurbonesEx.RegisterColliderForCollisionFeedback(collider.Pointer, 1);
                    }

                    isColliderHapticCapable = registratedLeftColliders.Count != 0 && registratedRightColliders.Count != 0;

                    if (isColliderHapticCapable)
                    {
                        allRegistratedColliders.AddRange(registratedLeftColliders);
                        allRegistratedColliders.AddRange(registratedRightColliders);

                        foreach (var db in currentAvatarObject.GetDynamicBones())
                        {
                            localDynamicBones.Add(db);

                            if (TurbonesEx.isPresent && m_ColliderHapticIgnoreSelf) TurbonesEx.ExcludeBoneFromCollisionFeedback(db.Pointer);
                        }

                        MelonLogger.Msg($"Collider Haptic: OK!(Left collider count: {registratedLeftColliders.Count}. Right collider count: {registratedRightColliders.Count})");
                    }
                    else FailedCapabilityResult("No hand colliders found on avatar.");
                }

                if (m_MeshHaptic && (m_MeshHapticPlayers || m_MeshHapticWorld))
                {
                    MeshHapticEx.SetupAvatar(currentAnimator);
                    MelonLogger.Msg($"Mesh Haptic: OK!");
                }
            }
            catch (Exception e)
            {
                isColliderHapticCapable = false;
                MelonLogger.Error($"Error when checking capability\n{e}");
            }

            static void FailedCapabilityResult(string reason)
            {
                MelonLogger.Warning($"Capability Result: {reason}");
                isColliderHapticCapable = false;
            }
        }

        public IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}