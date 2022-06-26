using OVR.OpenVR;

namespace ImmersiveTouch
{
    public class OVRHaptics
    {
        public static void SendHaptic(ETrackedControllerRole controller, ushort duration)
        {
            if (OpenVR.System == null) return;

            uint ucontroller = OpenVR.System.GetTrackedDeviceIndexForControllerRole(controller);

            for (uint i = 0; i < 21; i++) OpenVR.System.TriggerHapticPulse(ucontroller, i, duration);
        }
    }
}
