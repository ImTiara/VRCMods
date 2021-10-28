using MelonLoader;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

[assembly: MelonInfo(typeof(GestureIndicator.GestureIndicator), "GestureIndicator", "1.0.5", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace GestureIndicator
{
    public class GestureIndicator : MelonMod
    {
        private bool m_Enable;
        private Color m_LeftTextColor = Color.cyan;
        private Color m_RightTextColor = Color.cyan;
        private float m_TextOpacity;
        private float m_X_Position;
        private float m_Y_Position;
        private float m_HideAfterSeconds;

        private TextMeshProUGUI leftGestureText;
        private TextMeshProUGUI rightGestureText;

        private HandGestureController.Gesture prevLeftGesture;
        private HandGestureController.Gesture prevRightGesture;

        private DateTime leftTime;
        private DateTime rightTime;

        public override void OnApplicationStart()
            => MelonCoroutines.Start(UiManagerInitializer());

        public void OnUiManagerInit()
        {
            MelonPreferences.CreateCategory(GetType().Name, "Gesture Indicator");
            MelonPreferences.CreateEntry(GetType().Name, "Enable", true, "Enable Gesture Indicator");
            MelonPreferences.CreateEntry(GetType().Name, "TextOpacity", 85f, "Text Opacity (%)");
            MelonPreferences.CreateEntry(GetType().Name, "LeftTextColor", "#00FFFF", "Left Text Color");
            MelonPreferences.CreateEntry(GetType().Name, "RightTextColor", "#00FFFF", "Right Text Color");
            MelonPreferences.CreateEntry(GetType().Name, "TextXPosition", 1.0f, "Text X Position");
            MelonPreferences.CreateEntry(GetType().Name, "TextYPosition", 1.0f, "Text Y Position");
            MelonPreferences.CreateEntry(GetType().Name, "HideAfterSeconds", 0.0f, "Hide After Seconds (0 = never)");

            CreateIndicators();

            OnPreferencesSaved();
        }

        public override void OnPreferencesSaved()
        {
            m_Enable = MelonPreferences.GetEntryValue<bool>(GetType().Name, "Enable");
            m_TextOpacity = MelonPreferences.GetEntryValue<float>(GetType().Name, "TextOpacity");
            m_LeftTextColor = Manager.HexToColor(MelonPreferences.GetEntryValue<string>(GetType().Name, "LeftTextColor"));
            m_RightTextColor = Manager.HexToColor(MelonPreferences.GetEntryValue<string>(GetType().Name, "RightTextColor"));
            m_X_Position = MelonPreferences.GetEntryValue<float>(GetType().Name, "TextXPosition");
            m_Y_Position = MelonPreferences.GetEntryValue<float>(GetType().Name, "TextYPosition");
            m_HideAfterSeconds = MelonPreferences.GetEntryValue<float>(GetType().Name, "HideAfterSeconds");

            ToggleIndicators(m_Enable);
            ApplyTextColors();
            ApplyTextPositions();
        }

        private IEnumerator CheckGesture()
        {
            while (m_Enable)
            {
                try
                {
                    if (Manager.GetLocalVRCPlayer() != null)
                    {
                        if (m_HideAfterSeconds > 0)
                            ShowTimedGestures();
                        else
                            ShowStaticGestures();
                    }
                }
                catch (Exception e) { MelonLogger.Error("Error checking gestures: " + e); }

                yield return new WaitForSeconds(.1f);
            }
        }

        private void ShowStaticGestures()
        {
            if (Manager.GetGestureLeftWeight() >= 0.1f)
            {
                switch (Manager.GetLeftGesture())
                {
                    case HandGestureController.Gesture.Fist: leftGestureText.text = "Fist"; break;
                    case HandGestureController.Gesture.Open: leftGestureText.text = "Hand Open"; break;
                    case HandGestureController.Gesture.Point: leftGestureText.text = "Point"; break;
                    case HandGestureController.Gesture.Peace: leftGestureText.text = "Victory"; break;
                    case HandGestureController.Gesture.RockNRoll: leftGestureText.text = "RockNRoll"; break;
                    case HandGestureController.Gesture.Gun: leftGestureText.text = "Hand Gun"; break;
                    case HandGestureController.Gesture.ThumbsUp: leftGestureText.text = "Thumbs Up"; break;
                }
            }
            else leftGestureText.text = "";

            if (Manager.GetGestureRightWeight() >= 0.1f)
            {
                switch (Manager.GetRightGesture())
                {
                    case HandGestureController.Gesture.Fist: rightGestureText.text = "Fist"; break;
                    case HandGestureController.Gesture.Open: rightGestureText.text = "Hand Open"; break;
                    case HandGestureController.Gesture.Point: rightGestureText.text = "Point"; break;
                    case HandGestureController.Gesture.Peace: rightGestureText.text = "Victory"; break;
                    case HandGestureController.Gesture.RockNRoll: rightGestureText.text = "RockNRoll"; break;
                    case HandGestureController.Gesture.Gun: rightGestureText.text = "Hand Gun"; break;
                    case HandGestureController.Gesture.ThumbsUp: rightGestureText.text = "Thumbs Up"; break;
                }
            }
            else rightGestureText.text = "";

        }

        private void ShowTimedGestures()
        {
            // Credits to MarkViews for their original pull request

            var miliseconds = m_HideAfterSeconds * 1000;

            var leftGesture = Manager.GetLeftGesture();
            if (leftGesture != prevLeftGesture)
            {
                prevLeftGesture = leftGesture;
                leftTime = DateTime.Now.AddMilliseconds(miliseconds);
                if (Manager.GetGestureLeftWeight() >= 0.1f)
                {
                    switch (leftGesture)
                    {
                        case HandGestureController.Gesture.Fist: leftGestureText.text = "Fist"; break;
                        case HandGestureController.Gesture.Open: leftGestureText.text = "Hand Open"; break;
                        case HandGestureController.Gesture.Point: leftGestureText.text = "Point"; break;
                        case HandGestureController.Gesture.Peace: leftGestureText.text = "Victory"; break;
                        case HandGestureController.Gesture.RockNRoll: leftGestureText.text = "RockNRoll"; break;
                        case HandGestureController.Gesture.Gun: leftGestureText.text = "Hand Gun"; break;
                        case HandGestureController.Gesture.ThumbsUp: leftGestureText.text = "Thumbs Up"; break;
                    }
                }
                else leftGestureText.text = "";
            }
            else if (leftTime < DateTime.Now) leftGestureText.text = "";

            var rightGesture = Manager.GetRightGesture();
            if (rightGesture != prevRightGesture)
            {
                prevRightGesture = rightGesture;
                rightTime = DateTime.Now.AddMilliseconds(miliseconds);
                if (Manager.GetGestureRightWeight() >= 0.1f)
                {
                    switch (rightGesture)
                    {
                        case HandGestureController.Gesture.Fist: rightGestureText.text = "Fist"; break;
                        case HandGestureController.Gesture.Open: rightGestureText.text = "Hand Open"; break;
                        case HandGestureController.Gesture.Point: rightGestureText.text = "Point"; break;
                        case HandGestureController.Gesture.Peace: rightGestureText.text = "Victory"; break;
                        case HandGestureController.Gesture.RockNRoll: rightGestureText.text = "RockNRoll"; break;
                        case HandGestureController.Gesture.Gun: rightGestureText.text = "Hand Gun"; break;
                        case HandGestureController.Gesture.ThumbsUp: rightGestureText.text = "Thumbs Up"; break;
                    }
                }
                else rightGestureText.text = "";
            }
            else if (rightTime < DateTime.Now) rightGestureText.text = "";
        }

        private void CreateIndicators()
        {
            Transform hud = Manager.GetVRCUiManager().transform.Find("UnscaledUI/HudContent");
            GameObject textTemplate = Manager.GetQuickMenu().transform.Find("Container/Window/QMNotificationsArea/DebugInfoPanel/Panel/Text_FPS").gameObject;

            leftGestureText = UnityEngine.Object.Instantiate(textTemplate, hud, true).GetComponent<TextMeshProUGUI>();
            UnityEngine.Object.Destroy(leftGestureText.GetComponent<TextBinding>());
            leftGestureText.name = "GestureIndicator(Left)";
            leftGestureText.text = "";
            leftGestureText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rectTransformLeft = leftGestureText.GetComponent<RectTransform>();
            rectTransformLeft.localScale = new Vector2(1.0f, 1.0f);
            rectTransformLeft.sizeDelta = new Vector2(200f, -946f);

            rightGestureText = UnityEngine.Object.Instantiate(textTemplate, hud, true).GetComponent<TextMeshProUGUI>();
            UnityEngine.Object.Destroy(rightGestureText.GetComponent<TextBinding>());
            rightGestureText.name = "GestureIndicator(Right)";
            rightGestureText.text = "";
            rightGestureText.alignment = TextAlignmentOptions.MidlineRight;
            RectTransform rectTransformRight = rightGestureText.GetComponent<RectTransform>();
            rectTransformRight.localScale = new Vector2(1.0f, 1.0f);
            rectTransformRight.sizeDelta = new Vector2(200f, -946f);

            ApplyTextColors();
            ApplyTextPositions();
        }

        private void ApplyTextColors()
        {
            float op = m_TextOpacity / 100.0f;

            Color colorL = m_LeftTextColor;
            colorL.a = op;
            leftGestureText.color = colorL;

            Color colorR = m_RightTextColor;
            colorR.a = op;
            rightGestureText.color = colorR;
        }

        private void ApplyTextPositions()
        {
            leftGestureText.GetComponent<RectTransform>().anchoredPosition = new Vector2((-200f * m_X_Position) - 102.5f, -415f * m_Y_Position);
            rightGestureText.GetComponent<RectTransform>().anchoredPosition = new Vector2((200f * m_X_Position) - 102.5f, -415f * m_Y_Position);
        }

        private void ToggleIndicators(bool enable)
        {
            if (enable) MelonCoroutines.Start(CheckGesture());

            leftGestureText.gameObject.SetActive(enable);
            rightGestureText.gameObject.SetActive(enable);
        }

        public IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}