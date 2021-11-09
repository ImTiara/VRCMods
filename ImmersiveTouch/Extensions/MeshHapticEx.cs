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

        private static RenderTexture leftTexture;
        private static RenderTexture rightTexture;

        private static GameObject leftObject;
        private static GameObject rightObject;

        private Il2CppSystem.Action<AsyncGPUReadbackRequest> asyncGPUReadbackRequest;

        private DynamicBoneCollider collider;

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
        }

        public static void Destroy()
        {
            if (leftObject != null) Destroy(leftObject);
            if (rightObject != null) Destroy(rightObject);
        }

        public static void SetupAvatar(Animator animator, DynamicBoneCollider leftCollider, DynamicBoneCollider rightCollider)
        {
            #region Left
            Transform left = new GameObject("ImmersiveTouch_Left_Camera").transform;
            leftObject = left.gameObject;

            left.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
            left.localPosition = Vector3.zero;
            left.transform.parent = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            left.localEulerAngles = new Vector3(0, 180, 0);

            Camera leftCamera = SetupCamera(left.gameObject);
            leftCamera.orthographicSize = Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.LeftHand).position, animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal).position) / 4.0f;

            var leftCameraHaptic = left.gameObject.AddComponent<MeshHapticEx>();
            leftCameraHaptic.collider = leftCollider;
            #endregion

            #region Right
            Transform right = new GameObject("ImmersiveTouch_Right_Camera").transform;
            rightObject = right.gameObject;

            right.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
            right.localPosition = Vector3.zero;
            right.transform.parent = animator.GetBoneTransform(HumanBodyBones.RightHand);
            right.localEulerAngles = new Vector3(0, 180, 0);

            Camera rightCamera = SetupCamera(right.gameObject);
            rightCamera.orthographicSize = Vector3.Distance(animator.GetBoneTransform(HumanBodyBones.RightHand).position, animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal).position) / 4.0f;

            var rightCameraHaptic = right.gameObject.AddComponent<MeshHapticEx>();
            rightCameraHaptic.collider = rightCollider;
            #endregion

            static Camera SetupCamera(GameObject _gameObject)
            {
                Camera camera = _gameObject.AddComponent<Camera>();
                camera.depth = -5;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = cullingMask;
                camera.backgroundColor = Color.black;
                camera.orthographic = true;
                camera.nearClipPlane = -0.04f;
                camera.farClipPlane = 0.04f;
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
                    ImmersiveTouch.currentDBI = IntPtr.Zero;
                    ImmersiveTouch.SendHaptic(collider.Pointer);
                }
            });
        }

        public void OnRenderImage(RenderTexture source, RenderTexture _)
            => AsyncGPUReadback.Request(source, 0, asyncGPUReadbackRequest); // haha camera go brrrr
    }
}