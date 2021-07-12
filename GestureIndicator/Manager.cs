using UnityEngine;

namespace GestureIndicator
{
    public class Manager
    {
        private static HandGestureController _getHandGestureController;
        private static VRCUiManager _getVRCUiManager;
        private static QuickMenu _getQuickMenu;

        public static Color HexToColor(string hex)
        {
            hex = !hex.StartsWith("#") ? "#" + hex : hex;

            ColorUtility.TryParseHtmlString(hex, out Color c);

            return c;
        }

        public static Gesture GetGesture(Hand hand)
        {
            Gesture gesture = Gesture.Normal;

            HandGestureController handGestureController = GetHandGestureController();
            if (handGestureController == null) return gesture;

            switch (hand == Hand.Left ? handGestureController.field_Private_EnumNPrivateSealedva9vUnique_0 : handGestureController.field_Private_EnumNPrivateSealedva9vUnique_2)
            {
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue1:
                    gesture = Gesture.Fist;
                    break;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue2:
                    gesture = Gesture.Open;
                    break;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue3:
                    gesture = Gesture.Point;
                    break;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue4:
                    gesture = Gesture.Victory;
                    break;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue5:
                    gesture = Gesture.RockNRoll;
                    break;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue6:
                    gesture = Gesture.Gun;
                    break;
                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue7:
                    gesture = Gesture.ThumbsUp;
                    break;
            }
            return gesture;
        }

        public static HandGestureController GetHandGestureController()
        {
            VRCPlayer p = GetLocalVRCPlayer();
            if (p == null) return null;

            if (_getHandGestureController == null) _getHandGestureController = p.field_Private_VRC_AnimationController_0.field_Private_HandGestureController_0;
            return _getHandGestureController;
        }

        public static VRCUiManager GetVRCUiManager()
        {
            if (_getVRCUiManager == null) _getVRCUiManager = VRCUiManager.prop_VRCUiManager_0;
            return _getVRCUiManager;
        }

        public static QuickMenu GetQuickMenu()
        {
            if (_getQuickMenu == null) _getQuickMenu = QuickMenu.prop_QuickMenu_0;
            return _getQuickMenu;
        }

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