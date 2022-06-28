using HarmonyLib;
using MelonLoader;
using System;
using System.Collections;
using System.Reflection;
using UnhollowerRuntimeLib;
using UnityEngine;
using VRC.Core;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(ImmersiveTouch.ImmersiveTouch), "ImmersiveTouch", "2.0.2", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ImmersiveTouch
{
    public class ImmersiveTouch : MelonMod
    {
        public const float ORTHOGRAPHIC_SIZE_MOD = 4.0f;
        public const float CLIP_PLANE_MOD = 40.0f;
        public const float RENDER_INTERVAL = 0.09f;

        public static MelonPreferences_Entry<bool> ENABLE;
        public static MelonPreferences_Entry<float> HAPTIC_STRENGTH;
        public static MelonPreferences_Entry<float> HAPTIC_SENSITIVITY;
        public static MelonPreferences_Entry<bool> DOUBLE_SIDED;
        public static MelonPreferences_Entry<bool> COLLIDE_PLAYERS;
        public static MelonPreferences_Entry<bool> COLLIDE_WORLD;

        public static float m_HapticDistance = 0.015f;
        public static float m_AvatarHeightMod = 1.0f;

        public static CameraHaptic m_LeftCameraHaptic;
        public static CameraHaptic m_RightCameraHaptic;
        
        public static CameraHaptic m_LeftCameraHapticDouble;
        public static CameraHaptic m_RightCameraHapticDouble;

        public override void OnApplicationStart()
        {
            var category = MelonPreferences.CreateCategory("ImmersiveTouch", "Immersive Touch");
            ENABLE = category.CreateEntry("Enabled", true, "Enable Immersive Touch");
            HAPTIC_STRENGTH = category.CreateEntry("VibrationStrength", 750.0f, "Vibration Strength");
            HAPTIC_SENSITIVITY = category.CreateEntry("StrokeSensitivity", 250.0f, "Stroke Sensitivity");
            DOUBLE_SIDED = category.CreateEntry("DoubleSidedCollision", false, "Double Sided Collision");
            COLLIDE_PLAYERS = category.CreateEntry("PlayerCollision", true, "Player Collision");
            COLLIDE_WORLD = category.CreateEntry("WorldCollision", true, "World Collision");

            ENABLE.OnValueChanged += (editedValue, defaultValue)
                => SetupAvatar(true);

            HAPTIC_SENSITIVITY.OnValueChanged += (editedValue, defaultValue)
                => SetupAvatar(false);

            DOUBLE_SIDED.OnValueChanged += (editedValue, defaultValue)
                => SetupAvatar(false);

            COLLIDE_PLAYERS.OnValueChanged += (editedValue, defaultValue)
                => UpdateCameraCullingMasks();

            COLLIDE_WORLD.OnValueChanged += (editedValue, defaultValue)
                => UpdateCameraCullingMasks();

            MelonCoroutines.Start(VRCUiManagerInit());

            HarmonyInstance.Patch(typeof(PipelineManager).GetMethod(nameof(PipelineManager.Start)), null,
                new HarmonyMethod(typeof(ImmersiveTouch).GetMethod("OnPipelineManagerStart", BindingFlags.Public | BindingFlags.Static)));

            ClassInjector.RegisterTypeInIl2Cpp<CameraHaptic>();
        }

        public override void OnApplicationLateStart()
        {
            ScaleGoesBrrEx.Setup();
        }

        public static void OnPipelineManagerStart(ref PipelineManager __instance)
        {
            try
            {
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0 == null || VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.prop_GameObject_0.Pointer != __instance.gameObject.Pointer || VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.prop_AvatarKind_0 != VRCAvatarManager.AvatarKind.Custom) return;

                m_AvatarHeightMod = 1.0f;

                SetupAvatar(true);
            }
            catch (Exception e)
            {
                MelonLogger.Error($"Error checking when avatar changed:\n{e}");
            }
        }

        public static void SetupAvatar(bool showMessages)
        {
            try
            {
                DestroyImmersiveTouch();

                if (!ENABLE.Value) return;

                Animator animator = VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0?.field_Private_Animator_0;
                if (animator == null || !animator.isHuman)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because no valid animator was found.");
                    return;
                }
                float viewHeight = VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0.field_Private_VRC_AvatarDescriptor_0.ViewPosition.y * m_AvatarHeightMod;
                m_HapticDistance = viewHeight / HAPTIC_SENSITIVITY.Value;
                
                Transform leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);

                if (leftHand == null || rightHand == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because the Left/Right hand bone are missing.");
                    return;
                }

                Transform leftMiddleProximal = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                Transform rightMiddleProximal = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);

                if (leftMiddleProximal == null || rightMiddleProximal == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because the Left/Right Middle Proximal finger bone are missing.");
                    return;
                }

                Transform leftMiddleDistal = animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
                Transform rightMiddleDistal = animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

                if (leftMiddleDistal == null || rightMiddleDistal == null)
                {
                    if (showMessages) MelonLogger.Warning("Immersive Touch cannot use this avatar because the Left/Right Middle Distal finger bone are missing.");
                    return;
                }

                m_LeftCameraHaptic = ConfigureCameraHaptic(OVR.OpenVR.ETrackedControllerRole.LeftHand, leftHand, leftMiddleProximal, leftMiddleDistal, viewHeight);
                m_RightCameraHaptic = ConfigureCameraHaptic(OVR.OpenVR.ETrackedControllerRole.RightHand, rightHand, rightMiddleProximal, rightMiddleDistal, viewHeight);

                if (DOUBLE_SIDED.Value)
                {
                    m_LeftCameraHapticDouble = ConfigureCameraHaptic(OVR.OpenVR.ETrackedControllerRole.LeftHand, leftHand, leftMiddleProximal, leftMiddleDistal, viewHeight);
                    m_RightCameraHapticDouble = ConfigureCameraHaptic(OVR.OpenVR.ETrackedControllerRole.RightHand, rightHand, rightMiddleProximal, rightMiddleDistal, viewHeight);

                    m_LeftCameraHapticDouble.transform.localEulerAngles = new Vector3(0, 0, 0);
                    m_RightCameraHapticDouble.transform.localEulerAngles = new Vector3(0, 0, 0);
                }

                UpdateCameraCullingMasks();

                if (showMessages) MelonLogger.Msg("Immersive Touch is now active on this avatar!");
            }
            catch(Exception e)
            {
                if (showMessages) MelonLogger.Error($"Error when setting up avatar: {e}");
            }
        }

        public static CameraHaptic ConfigureCameraHaptic(OVR.OpenVR.ETrackedControllerRole controllerRole, Transform hand, Transform middleProximal, Transform middleDistal, float viewHeight)
        {
            Transform cameraHapticObject = new GameObject().transform;
            cameraHapticObject.parent = middleProximal;
            cameraHapticObject.localPosition = Vector3.zero;
            cameraHapticObject.parent = hand;
            cameraHapticObject.localEulerAngles = new Vector3(0, 180, 0);

            CameraHaptic cameraHaptic = cameraHapticObject.gameObject.AddComponent<CameraHaptic>();
            cameraHaptic.controller = controllerRole;
            cameraHaptic.camera.orthographicSize = Vector3.Distance(hand.position, middleDistal.position) / ORTHOGRAPHIC_SIZE_MOD;
            cameraHaptic.camera.nearClipPlane = -(viewHeight / CLIP_PLANE_MOD);
            cameraHaptic.camera.farClipPlane = viewHeight / CLIP_PLANE_MOD;

            return cameraHaptic;
        }

        public static void DestroyImmersiveTouch()
        {
            try // The garbage collector was being too aggressive >:(
            {
                if (m_LeftCameraHaptic != null)
                {
                    Object.DestroyImmediate(m_LeftCameraHaptic.gameObject);
                }
            }
            catch { }

            try
            {
                if (m_RightCameraHaptic != null)
                {
                    Object.DestroyImmediate(m_RightCameraHaptic.gameObject);
                }
            }
            catch { }

            if (DOUBLE_SIDED.Value)
            {
                try
                {
                    if (m_LeftCameraHapticDouble != null)
                    {
                        Object.DestroyImmediate(m_LeftCameraHapticDouble.gameObject);
                    }
                }
                catch { }

                try
                {
                    if (m_RightCameraHapticDouble != null)
                    {
                        Object.DestroyImmediate(m_RightCameraHapticDouble.gameObject);
                    }
                }
                catch { }
            }
        }

        public static void UpdateCameraCullingMasks()
        {
            try
            {
                int result = CalculateLayerMask(COLLIDE_WORLD.Value, COLLIDE_PLAYERS.Value);

                if (m_LeftCameraHaptic != null) m_LeftCameraHaptic.camera.cullingMask = result;
                if (m_RightCameraHaptic != null) m_RightCameraHaptic.camera.cullingMask = result;

                if (DOUBLE_SIDED.Value)
                {
                    if (m_LeftCameraHapticDouble != null) m_LeftCameraHapticDouble.camera.cullingMask = result;
                    if (m_RightCameraHapticDouble != null) m_RightCameraHapticDouble.camera.cullingMask = result;
                }
            }
            catch(Exception e)
            {
                MelonLogger.Error($"Error updating culling masks: {e}");
            }
        }

        public static int CalculateLayerMask(bool allowWorld, bool allowPlayers)
            => (allowWorld ? (1 << 0) : 0) | (allowPlayers ? (1 << 9) : 0);

        public static IEnumerator VRCUiManagerInit()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;

            foreach (var renderModel in VRCVrCamera.field_Private_Static_VRCVrCamera_0.GetComponentsInChildren<SteamVR_RenderModel>(true))
            {
                renderModel.gameObject.layer = 10;
            }
        }
    }
}