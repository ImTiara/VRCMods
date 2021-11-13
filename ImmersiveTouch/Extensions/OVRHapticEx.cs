using OVR.OpenVR;
using System;

namespace ImmersiveTouch.Extensions
{
    public class OVRHapticEx
    {
        public static Array EVRButtonIds;
        public static void Setup()
        {
            EVRButtonIds = Enum.GetValues(typeof(EVRButtonId));
        }

        public static void SendLeftHaptic(ushort amplitude = 1500)
        {
            uint leftController = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            foreach (EVRButtonId id in EVRButtonIds) OpenVR.System.TriggerHapticPulse(leftController, (uint)id, amplitude);
        }

        public static void SendRightHaptic(ushort amplitude = 1500)
        {
            uint leftController = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            foreach (EVRButtonId id in EVRButtonIds) OpenVR.System.TriggerHapticPulse(leftController, (uint)id, amplitude);
        }
    }
}
