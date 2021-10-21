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

        private TextMeshProUGUI m_LeftGestureText;
        private TextMeshProUGUI m_RightGestureText;

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
                        if (Manager.GetGestureLeftWeight() >= 0.1f)
                        {
                            switch (Manager.GetLeftGesture())
                            {
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue1: m_LeftGestureText.text = "Fist"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue2: m_LeftGestureText.text = "Hand Open"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue3: m_LeftGestureText.text = "Point"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue4: m_LeftGestureText.text = "Victory"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue5: m_LeftGestureText.text = "RockNRoll"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue6: m_LeftGestureText.text = "Hand Gun"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue7: m_LeftGestureText.text = "Thumbs Up"; break;
                            }
                        }
                        else m_LeftGestureText.text = "";

                        if (Manager.GetGestureRightWeight() >= 0.1f)
                        {
                            switch (Manager.GetRightGesture())
                            {
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue1: m_RightGestureText.text = "Fist"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue2: m_RightGestureText.text = "Hand Open"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue3: m_RightGestureText.text = "Point"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue4: m_RightGestureText.text = "Victory"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue5: m_RightGestureText.text = "RockNRoll"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue6: m_RightGestureText.text = "Hand Gun"; break;
                                case HandGestureController.EnumNPrivateSealedva9vUnique.EnumValue7: m_RightGestureText.text = "Thumbs Up"; break;
                            }
                        }
                        else m_RightGestureText.text = "";
                    }
                }
                catch (Exception e) { MelonLogger.Error("Error checking gestures: " + e); }

                yield return new WaitForSeconds(.1f);
            }
        }

        private void CreateIndicators()
        {
            Transform hud = Manager.GetVRCUiManager().transform.Find("UnscaledUI/HudContent");
            GameObject textTemplate = Manager.GetQuickMenu().transform.Find("Container/Window/QMNotificationsArea/DebugInfoPanel/Panel/Text_FPS").gameObject;

            m_LeftGestureText = UnityEngine.Object.Instantiate(textTemplate, hud, true).GetComponent<TextMeshProUGUI>();
            UnityEngine.Object.Destroy(m_LeftGestureText.GetComponent<TextBinding>());
            m_LeftGestureText.name = "GestureIndicator(Left)";
            RectTransform rectTransformLeft = m_LeftGestureText.GetComponent<RectTransform>();
            rectTransformLeft.localScale = new Vector2(1.0f, 1.0f);
            rectTransformLeft.sizeDelta = new Vector2(200f, -946f);
            m_LeftGestureText.text = "";
            m_LeftGestureText.alignment = TextAlignmentOptions.MidlineLeft;

            m_RightGestureText = UnityEngine.Object.Instantiate(textTemplate, hud, true).GetComponent<TextMeshProUGUI>();
            UnityEngine.Object.Destroy(m_RightGestureText.GetComponent<TextBinding>());
            m_RightGestureText.name = "GestureIndicator(Right)";
            RectTransform rectTransformRight = m_RightGestureText.GetComponent<RectTransform>();
            rectTransformRight.localScale = new Vector2(1.0f, 1.0f);
            rectTransformRight.sizeDelta = new Vector2(200f, -946f);
            m_RightGestureText.text = "";
            m_RightGestureText.alignment = TextAlignmentOptions.MidlineRight;

            ApplyTextColors();
            ApplyTextPositions();
        }

        private void ApplyTextColors()
        {
            float op = m_TextOpacity / 100.0f;

            Color colorL = m_LeftTextColor;
            colorL.a = op;
            m_LeftGestureText.color = colorL;

            Color colorR = m_RightTextColor;
            colorR.a = op;
            m_RightGestureText.color = colorR;
        }

        private void ApplyTextPositions()
        {
            m_LeftGestureText.GetComponent<RectTransform>().anchoredPosition = new Vector2((-200f * m_X_Position) - 102.5f, -415f * m_Y_Position);
            m_RightGestureText.GetComponent<RectTransform>().anchoredPosition = new Vector2((200f * m_X_Position) - 102.5f, -415f * m_Y_Position);
        }

        private void ToggleIndicators(bool enable)
        {
            if (enable) MelonCoroutines.Start(CheckGesture());

            m_LeftGestureText.gameObject.SetActive(enable);
            m_RightGestureText.gameObject.SetActive(enable);
        }

        public IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}