using UnityEngine.UI;
using UnityEngine;
using VRC.UserCamera;
using System.Collections;

namespace ActionCamera
{
    public static class Manager
    {
        private static Button enableCamera;
        public static void EnableCamera()
        {
            if (!VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0.IsUserInVR()) return;

            if (enableCamera == null) enableCamera = QuickMenu.prop_QuickMenu_0.transform.Find("CameraMenu/PhotoMode").GetComponent<Button>();
            enableCamera.onClick.Invoke();
        }

        private static Button disableCamera;
        public static void DisableCamera()
        {
            if (!VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCPlayerApi_0.IsUserInVR()) return;

            if (disableCamera == null) disableCamera = QuickMenu.prop_QuickMenu_0.transform.Find("CameraMenu/DisableCamera").GetComponent<Button>();
            
            disableCamera.onClick.Invoke();
        }

        private static CameraInteractable cameraInteractable;
        public static void TakePicture()
        {
            if (cameraInteractable == null) cameraInteractable = GameObject.Find("TrackingVolume/PlayerObjects/UserCamera/ViewFinder").GetComponent<CameraInteractable>();
            
            if (cameraInteractable.gameObject.activeInHierarchy)
                cameraInteractable.OnPickupUseDown();
        }

        private static CameraInteractable timedInteractable;
        public static void TakeTimedPicture()
        {
            if (timedInteractable == null) timedInteractable = GameObject.Find("TrackingVolume/PlayerObjects/UserCamera/ViewFinder/PhotoControls/Right_Timer").GetComponent<CameraInteractable>();
            
            if (timedInteractable.gameObject.activeInHierarchy)
                timedInteractable.Interact();
        }

        private static bool isTakingTimedPicture;
        public static IEnumerator TakeCustomTimedPicture(float time)
        {
            if (timedInteractable.gameObject.activeInHierarchy && !isTakingTimedPicture)
            {
                isTakingTimedPicture = true;
                yield return new WaitForSeconds(time - 5.0f);
                TakeTimedPicture();
                isTakingTimedPicture = false;
            }
        }
    }
}
