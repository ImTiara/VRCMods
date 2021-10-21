using UnityEngine;

namespace GestureIndicator
{
    public class Manager
    {
        public static Color HexToColor(string hex)
        {
            hex = !hex.StartsWith("#") ? "#" + hex : hex;
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }

        public static HandGestureController.EnumNPrivateSealedva9vUnique GetLeftGesture()
            => GetHandGestureController().field_Private_EnumNPrivateSealedva9vUnique_0;
        
        public static HandGestureController.EnumNPrivateSealedva9vUnique GetRightGesture()
            => GetHandGestureController().field_Private_EnumNPrivateSealedva9vUnique_2;
        
        public static HandGestureController GetHandGestureController()
            => GetLocalVRCPlayer().field_Private_VRC_AnimationController_0.field_Private_HandGestureController_0;
        
        public static VRCUiManager GetVRCUiManager()
            => VRCUiManager.prop_VRCUiManager_0;
        
        private static VRC.UI.Elements.QuickMenu _QuickMenu;
        public static VRC.UI.Elements.QuickMenu GetQuickMenu()
            => _QuickMenu ??= Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>()[0];

        public static VRCPlayer GetLocalVRCPlayer() => VRCPlayer.field_Internal_Static_VRCPlayer_0;

        public static float GetGestureLeftWeight() => GetHandGestureController().field_Private_Single_2;
        public static float GetGestureRightWeight() => GetHandGestureController().field_Private_Single_3;

        public enum Hand
        {
            Left,
            Right
        }
    }
}