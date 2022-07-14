using OVR.OpenVR;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using UnityEngine.Rendering;
using MelonLoader;

namespace ImmersiveTouch
{
    public class CameraHaptic : MonoBehaviour
    {
        public CameraHaptic(IntPtr intPtr) : base(intPtr) { }

        public RenderTexture renderTexture;
        public Camera camera;

        public ETrackedControllerRole controller = ETrackedControllerRole.Invalid;

        public bool isColliding;
        public Vector3 lastCollisionVector;
        
        private Action<AsyncGPUReadbackRequest> asyncGPUReadbackRequest;
        private bool _render = true;

        public void Awake()
        {
            renderTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.R8)
            {
                filterMode = FilterMode.Point,
                anisoLevel = 0
            };
            renderTexture.Create();
            
            camera = gameObject.AddComponent<Camera>();
            camera.depth = -5;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0, 0);
            camera.orthographic = true;
            camera.nearClipPlane = -0.1f;
            camera.farClipPlane = 0.1f;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.targetTexture = renderTexture;
            camera.stereoTargetEye = StereoTargetEyeMask.None;
            camera.enabled = false;

            MelonCoroutines.Start(Renderer());

            asyncGPUReadbackRequest = new Action<AsyncGPUReadbackRequest>((readback) =>
            {
                isColliding = Marshal.PtrToStructure<byte>(readback.GetDataRaw(0)) != 3;
            });
        }

        [HideFromIl2Cpp]
        public IEnumerator Renderer()
        {
            while (_render)
            {
                camera.enabled = true;
                yield return new WaitForSeconds(ImmersiveTouch.RENDER_INTERVAL.Value);
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture _)
        {
            camera.enabled = false;
            AsyncGPUReadback.Request(source, 0, asyncGPUReadbackRequest);
        }

        public void Update()
        {
            if (isColliding && Vector3.Distance(transform.position, lastCollisionVector) > ImmersiveTouch.m_HapticDistance)
            {
                lastCollisionVector = transform.position;

                OVRHaptics.SendHaptic(controller, (ushort)ImmersiveTouch.HAPTIC_STRENGTH.Value);
            }
        }

        public void OnDestroy()
        {
            _render = false;

            renderTexture.Release();
            renderTexture.DiscardContents();
        }
    }
}
