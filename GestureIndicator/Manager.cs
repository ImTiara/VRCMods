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

        public static Gesture GetGesture(Hand hand)
        {
            HandGestureController handGestureController = GetHandGestureController();
            if (handGestureController == null) return Gesture.Normal;

            switch (hand == Hand.Left ? handGestureController.field_Private_EnumNPrivateSealedva9vUnique_0 : handGestureController.field_Private_EnumNPrivateSealedva9vUnique_2)
            {
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue1: return Gesture.Fist;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue2: return Gesture.Open;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue3: return Gesture.Point;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue4: return Gesture.Victory;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue5: return Gesture.RockNRoll;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue6: return Gesture.Gun;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue7: return Gesture.ThumbsUp;
            }
            return Gesture.Normal;
        }

        private static HandGestureController _GetHandGestureController;
        public static HandGestureController GetHandGestureController()
        {
            if (_GetHandGestureController == null) _GetHandGestureController = GetLocalVRCPlayer()?.field_Private_VRC_AnimationController_0.field_Private_HandGestureController_0;
            return _GetHandGestureController;
        }

        public static VRCUiManager GetVRCUiManager()
            => VRCUiManager.prop_VRCUiManager_0;
        

        private static VRC.UI.Elements.QuickMenu _QuickMenu;
        public static VRC.UI.Elements.QuickMenu GetQuickMenu()
            => _QuickMenu ??= Resources.FindObjectsOfTypeAll<VRC.UI.Elements.QuickMenu>()[0];

        public static VRCPlayer GetLocalVRCPlayer() => VRCPlayer.field_Internal_Static_VRCPlayer_0;

        public static float GetGestureLeftWeight() => GetHandGestureController().field_Private_Single_2;
        public static float GetGestureRightWeight() => GetHandGestureController().field_Private_Single_3;

        public enum Gesture
        {
            Normal,
            Fist,
            Open,
            Point,
            Victory,
            RockNRoll,
            Gun,
            ThumbsUp
        }

        public enum Hand
        {
            Left,
            Right
        }
    }
}