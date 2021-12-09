using OVR.OpenVR;
using System;

namespace ImmersiveTouch.Extensions
{
    public class OVRHapticEx
    {
        public static void SendLeftHaptic(ushort amplitude)
        {
            if (OpenVR.System == null) return;

            uint controller = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);

            for (int i = 0; i < 21; i++) OpenVR.System.TriggerHapticPulse(controller, (uint)i, amplitude);
        }

        public static void SendRightHaptic(ushort amplitude)
        {
            if (OpenVR.System == null) return;

            uint controller = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);

            for (int i = 0; i < 21; i++) OpenVR.System.TriggerHapticPulse(controller, (uint)i, amplitude);
        }
    }
}
