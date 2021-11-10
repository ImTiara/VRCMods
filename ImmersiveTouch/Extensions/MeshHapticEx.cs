using System;
using System.Runtime.InteropServices;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Rendering;

namespace ImmersiveTouch.Extensions
{
    public class MeshHapticEx : MonoBehaviour
    {
        public MeshHapticEx(IntPtr pointer) : base(pointer) { }

        public static int cullingMask;

        public static float cameraWidthModifier = 4.0f;

        private static RenderTexture leftTexture;
        private static RenderTexture rightTexture;

        private static GameObject leftObject;
        private static GameObject rightObject;

        private static GameObject leftSteamVRModel;
        private static GameObject rightSteamVRModel;

        private Il2CppSystem.Action<AsyncGPUReadbackRequest> asyncGPUReadbackRequest;

        private Transform wrist;

        public static void Setup()
        {
            ClassInjector.RegisterTypeInIl2Cpp<MeshHapticEx>();

            leftTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            leftTexture.Create();

            rightTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            rightTexture.Create();

            foreach (var model in VRCVrCamera.field_Private_Static_VRCVrCamera_0.GetComponentsInChildren<SteamVR_RenderModel>(true))
            {
                switch(model.transform.parent.name)
                {
                    case "Controller (left)": leftSteamVRModel = model.gameObject; break;
                    case "Controller (right)": rightSteamVRModel = model.gameObject; break;
                }
            }
        }

        public static void Destroy()
        {
            if (leftObject != null) Destroy(leftObject);
            if (rightObject != null) Destroy(rightObject);
        }

        public static void SetupAvatar(Animator animator)
        {
            #region Left
            Transform left = new GameObject("ImmersiveTouch_Left_Camera").transform;
            leftObject = left.gameObject;

            left.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            left.localPosition = Vector3.zero;
            left.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            left.localEulerAngles = new Vector3(0, 180, 0);

            Camera leftCamera = SetupCamera(left.gameObject);
            leftCamera.orthographicSize = Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.LeftHand).position, animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal).position) / cameraWidthModifier;

            var leftCameraHaptic = left.gameObject.AddComponent<MeshHapticEx>();
            leftCameraHaptic.wrist = left.transform.parent;
            #endregion

            #region Right
            Transform right = new GameObject("ImmersiveTouch_Right_Camera").transform;
            rightObject = right.gameObject;

            right.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            right.localPosition = Vector3.zero;
            right.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            right.localEulerAngles = new Vector3(0, 180, 0);

            Camera rightCamera = SetupCamera(right.gameObject);
            rightCamera.orthographicSize = Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.RightHand).position, animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal).position) / cameraWidthModifier;

            var rightCameraHaptic = right.gameObject.AddComponent<MeshHapticEx>();
            rightCameraHaptic.wrist = right.transform.parent;
            #endregion

            static Camera SetupCamera(GameObject _gameObject)
            {
                Camera camera = _gameObject.AddComponent<Camera>();
                camera.depth = -5;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = cullingMask;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.nearClipPlane = -(ImmersiveTouch.currentViewHeight / 40.0f);
                camera.farClipPlane = ImmersiveTouch.currentViewHeight / 40.0f;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = rightTexture;
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.pixelRect = new Rect(0, 0, 1, 1);
                return camera;
            }
        }

        public void Awake()
        {
            asyncGPUReadbackRequest = new Action<AsyncGPUReadbackRequest>((readback) =>
            {
                if (Marshal.PtrToStructure<Color32>(readback.GetDataRaw(0)) != Color.black)
                {
                    if (leftSteamVRModel.activeSelf || rightSteamVRModel.activeSelf) return;

                    ImmersiveTouch.SendHaptic(wrist);
                }
            });
        }

        public void OnRenderImage(RenderTexture source, RenderTexture _)
            => AsyncGPUReadback.Request(source, 0, asyncGPUReadbackRequest); // haha camera go brrrr
    }
}