using MelonLoader;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(GestureIndicator.GestureIndicator), "GestureIndicator", "1.0.8", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace GestureIndicator
{
    public class GestureIndicator : MelonMod
    {
        public static MelonLogger.Instance Logger;

        private bool m_Enable;
        private Color m_LeftTextColor = Color.cyan;
        private Color m_RightTextColor = Color.cyan;
        private float m_TextOpacity;
        private float m_IconOpacity;
        private float m_X_Position;
        private float m_Y_Position;
        private float m_HideAfterSeconds;
        private bool m_UseIcons;
        private bool m_IconsOnly;

        private TextMeshProUGUI leftGestureText;
        private TextMeshProUGUI rightGestureText;
        private Image leftIcon, rightIcon;

        private HandGestureController.Gesture prevLeftGesture;
        private HandGestureController.Gesture prevRightGesture;

        private DateTime leftTime;
        private DateTime rightTime;

        public override void OnApplicationStart()
        {
            Logger = new MelonLogger.Instance(GetType().Name);

            MelonPreferences.CreateCategory(GetType().Name, "Gesture Indicator");
            MelonPreferences.CreateEntry(GetType().Name, "Enable", true, "Enable Gesture Indicator");
            MelonPreferences.CreateEntry(GetType().Name, "TextOpacity", 85f, "Text Opacity (%)");
            MelonPreferences.CreateEntry(GetType().Name, "IconOpacity", 100f, "Icon Opacity (%)");
            MelonPreferences.CreateEntry(GetType().Name, "LeftTextColor", "#00FFFF", "Left Text Color");
            MelonPreferences.CreateEntry(GetType().Name, "RightTextColor", "#00FFFF", "Right Text Color");
            MelonPreferences.CreateEntry(GetType().Name, "TextXPosition", 1.0f, "Text X Position");
            MelonPreferences.CreateEntry(GetType().Name, "TextYPosition", 1.0f, "Text Y Position");
            MelonPreferences.CreateEntry(GetType().Name, "HideAfterSeconds", 0.0f, "Hide After Seconds (0 = never)");
            MelonPreferences.CreateEntry(GetType().Name, "UseIcons", true, "Use Icons");
            MelonPreferences.CreateEntry(GetType().Name, "IconsOnly", false, "Icons Only");

            LoadAssets.loadAssets();
            MelonCoroutines.Start(UiManagerInitializer());
        }

        public void OnUiManagerInit()
        {
            CreateIndicators();

            OnPreferencesSaved();
        }

        public override void OnPreferencesSaved()
        {
            m_Enable = MelonPreferences.GetEntryValue<bool>(GetType().Name, "Enable");
            m_TextOpacity = MelonPreferences.GetEntryValue<float>(GetType().Name, "TextOpacity");
            m_IconOpacity = MelonPreferences.GetEntryValue<float>(GetType().Name, "IconOpacity");
            m_LeftTextColor = Manager.HexToColor(MelonPreferences.GetEntryValue<string>(GetType().Name, "LeftTextColor"));
            m_RightTextColor = Manager.HexToColor(MelonPreferences.GetEntryValue<string>(GetType().Name, "RightTextColor"));
            m_X_Position = MelonPreferences.GetEntryValue<float>(GetType().Name, "TextXPosition");
            m_Y_Position = MelonPreferences.GetEntryValue<float>(GetType().Name, "TextYPosition");
            m_HideAfterSeconds = MelonPreferences.GetEntryValue<float>(GetType().Name, "HideAfterSeconds");
            m_UseIcons = MelonPreferences.GetEntryValue<bool>(GetType().Name, "UseIcons");
            m_IconsOnly = MelonPreferences.GetEntryValue<bool>(GetType().Name, "IconsOnly");

            ToggleIndicators(m_Enable);
            ApplyColors();
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
                catch (Exception e) { Logger.Error("Error checking gestures: " + e); }

                yield return new WaitForSeconds(.1f);
            }
        }

        private void ShowStaticGestures()
        {
            if (Manager.GetGestureLeftWeight() >= 0.1f)
            {
                switch (Manager.GetLeftGesture())
                {
                    case HandGestureController.Gesture.Fist: leftGestureText.text = m_IconsOnly ? "" : "Fist"; leftIcon.sprite = LoadAssets.Fist; break;
                    case HandGestureController.Gesture.Open: leftGestureText.text = m_IconsOnly ? "" : "Hand Open"; leftIcon.sprite = LoadAssets.OpenHand; break;
                    case HandGestureController.Gesture.Point: leftGestureText.text = m_IconsOnly ? "" : "Point"; leftIcon.sprite = LoadAssets.Point; break;
                    case HandGestureController.Gesture.Peace: leftGestureText.text = m_IconsOnly ? "" : "Victory"; leftIcon.sprite = LoadAssets.Victory; break;
                    case HandGestureController.Gesture.RockNRoll: leftGestureText.text = m_IconsOnly ? "" : "RockNRoll"; leftIcon.sprite = LoadAssets.RockAndRoll; break;
                    case HandGestureController.Gesture.Gun: leftGestureText.text = m_IconsOnly ? "" : "Hand Gun"; leftIcon.sprite = LoadAssets.FingerGun; break;
                    case HandGestureController.Gesture.ThumbsUp: leftGestureText.text = m_IconsOnly ? "" : "Thumbs Up"; leftIcon.sprite = LoadAssets.ThumbsUp; break;
                }
                if (m_UseIcons) leftIcon.gameObject.SetActive(true);
            }
            else
            {
                leftGestureText.text = "";
                leftIcon.gameObject.SetActive(false);
            }

            if (Manager.GetGestureRightWeight() >= 0.1f)
            {
                switch (Manager.GetRightGesture())
                {
                    case HandGestureController.Gesture.Fist: rightGestureText.text = m_IconsOnly ? "" : "Fist"; rightIcon.sprite = LoadAssets.Fist; break;
                    case HandGestureController.Gesture.Open: rightGestureText.text = m_IconsOnly ? "" : "Hand Open"; rightIcon.sprite = LoadAssets.OpenHand; break;
                    case HandGestureController.Gesture.Point: rightGestureText.text = m_IconsOnly ? "" : "Point"; rightIcon.sprite = LoadAssets.Point; break;
                    case HandGestureController.Gesture.Peace: rightGestureText.text = m_IconsOnly ? "" : "Victory"; rightIcon.sprite = LoadAssets.Victory; break;
                    case HandGestureController.Gesture.RockNRoll: rightGestureText.text = m_IconsOnly ? "" : "RockNRoll"; rightIcon.sprite = LoadAssets.RockAndRoll; break;
                    case HandGestureController.Gesture.Gun: rightGestureText.text = m_IconsOnly ? "" : "Hand Gun"; rightIcon.sprite = LoadAssets.FingerGun; break;
                    case HandGestureController.Gesture.ThumbsUp: rightGestureText.text = m_IconsOnly ? "" : "Thumbs Up"; rightIcon.sprite = LoadAssets.ThumbsUp; break;
                }
                if (m_UseIcons) rightIcon.gameObject.SetActive(true);
            }
            else
            {
                rightGestureText.text = "";
                rightIcon.gameObject.SetActive(false);
            }
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
                        case HandGestureController.Gesture.Fist: leftGestureText.text = m_IconsOnly ? "" : "Fist"; leftIcon.sprite = LoadAssets.Fist; break;
                        case HandGestureController.Gesture.Open: leftGestureText.text = m_IconsOnly ? "" : "Hand Open"; leftIcon.sprite = LoadAssets.OpenHand; break;
                        case HandGestureController.Gesture.Point: leftGestureText.text = m_IconsOnly ? "" : "Point"; leftIcon.sprite = LoadAssets.Point; break;
                        case HandGestureController.Gesture.Peace: leftGestureText.text = m_IconsOnly ? "" : "Victory"; leftIcon.sprite = LoadAssets.Victory; break;
                        case HandGestureController.Gesture.RockNRoll: leftGestureText.text = m_IconsOnly ? "" : "RockNRoll"; leftIcon.sprite = LoadAssets.RockAndRoll; break;
                        case HandGestureController.Gesture.Gun: leftGestureText.text = m_IconsOnly ? "" : "Hand Gun"; leftIcon.sprite = LoadAssets.FingerGun; break;
                        case HandGestureController.Gesture.ThumbsUp: leftGestureText.text = m_IconsOnly ? "" : "Thumbs Up"; leftIcon.sprite = LoadAssets.ThumbsUp; break;
                    }
                    if (m_UseIcons) leftIcon.gameObject.SetActive(true);

                }
                else
                {
                    leftGestureText.text = "";
                    leftIcon.gameObject.SetActive(false);
                    prevLeftGesture = HandGestureController.Gesture.None;

                }
            }
            else if (leftTime < DateTime.Now)
            {
                leftGestureText.text = "";
                leftIcon.gameObject.SetActive(false);
            }

            var rightGesture = Manager.GetRightGesture();
            if (rightGesture != prevRightGesture)
            {
                prevRightGesture = rightGesture;
                rightTime = DateTime.Now.AddMilliseconds(miliseconds);
                if (Manager.GetGestureRightWeight() >= 0.1f)
                {
                    switch (rightGesture)
                    {
                        case HandGestureController.Gesture.Fist: rightGestureText.text = m_IconsOnly ? "" : "Fist"; rightIcon.sprite = LoadAssets.Fist; break;
                        case HandGestureController.Gesture.Open: rightGestureText.text = m_IconsOnly ? "" : "Hand Open"; rightIcon.sprite = LoadAssets.OpenHand; break;
                        case HandGestureController.Gesture.Point: rightGestureText.text = m_IconsOnly ? "" : "Point"; rightIcon.sprite = LoadAssets.Point; break;
                        case HandGestureController.Gesture.Peace: rightGestureText.text = m_IconsOnly ? "" : "Victory"; rightIcon.sprite = LoadAssets.Victory; break;
                        case HandGestureController.Gesture.RockNRoll: rightGestureText.text = m_IconsOnly ? "" : "RockNRoll"; rightIcon.sprite = LoadAssets.RockAndRoll; break;
                        case HandGestureController.Gesture.Gun: rightGestureText.text = m_IconsOnly ? "" : "Hand Gun"; rightIcon.sprite = LoadAssets.FingerGun; break;
                        case HandGestureController.Gesture.ThumbsUp: rightGestureText.text = m_IconsOnly ? "" : "Thumbs Up"; rightIcon.sprite = LoadAssets.ThumbsUp; break;
                    }
                    if (m_UseIcons) rightIcon.gameObject.SetActive(true);
                }
                else
                {
                    rightGestureText.text = "";
                    rightIcon.gameObject.SetActive(false);
                    prevRightGesture = HandGestureController.Gesture.None;
                }
            }
            else if (rightTime < DateTime.Now)
            {
                rightGestureText.text = "";
                rightIcon.gameObject.SetActive(false);
            }
        }

        private void CreateIndicators()
        {
            Transform hud = Manager.GetVRCUiManager().transform.Find("UnscaledUI/HudContent_Old");
            GameObject textTemplate = Manager.GetQuickMenu().transform.Find("Container/Window/QMNotificationsArea/DebugInfoPanel/Panel/Text_FPS").gameObject;
            GameObject iconTemplate = Manager.GetVRCUiManager().transform.Find("UnscaledUI/HudContent_Old/Hud/GestureToggleParent/GesturesON").gameObject;

            leftGestureText = UnityEngine.Object.Instantiate(textTemplate, hud, true).GetComponent<TextMeshProUGUI>();
            UnityEngine.Object.Destroy(leftGestureText.GetComponent<TextBinding>());
            leftGestureText.name = "GestureIndicator(Left)";
            leftGestureText.text = "";
            leftGestureText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rectTransformLeft = leftGestureText.GetComponent<RectTransform>();
            rectTransformLeft.localScale = new Vector2(1.0f, 1.0f);
            rectTransformLeft.sizeDelta = new Vector2(200f, -946f);
            leftIcon = UnityEngine.Object.Instantiate(iconTemplate, leftGestureText.transform, true).GetComponent<Image>();
            leftIcon.gameObject.name = "Icon";
            leftIcon.transform.localPosition = new Vector3(48f, 70f, 0f);
            leftIcon.gameObject.SetActive(false);
            leftIcon.sprite = null;

            rightGestureText = UnityEngine.Object.Instantiate(textTemplate, hud, true).GetComponent<TextMeshProUGUI>();
            UnityEngine.Object.Destroy(rightGestureText.GetComponent<TextBinding>());
            rightGestureText.name = "GestureIndicator(Right)";
            rightGestureText.text = "";
            rightGestureText.alignment = TextAlignmentOptions.MidlineRight;
            RectTransform rectTransformRight = rightGestureText.GetComponent<RectTransform>();
            rectTransformRight.localScale = new Vector2(1.0f, 1.0f);
            rectTransformRight.sizeDelta = new Vector2(200f, -946f);
            rightIcon = UnityEngine.Object.Instantiate(iconTemplate, rightGestureText.transform, true).GetComponent<Image>();
            rightIcon.gameObject.name = "Icon";
            rightIcon.transform.localPosition = new Vector3(154f, 70f, 0f);
            rightIcon.transform.localScale = new Vector3(-1f, 1f, 0f);
            rightIcon.gameObject.SetActive(false);
            rightIcon.sprite = null;

            ApplyColors();
            ApplyTextPositions();
        }

        private void ApplyColors()
        {
            var textOpacity = m_TextOpacity / 100.0f;
            var iconOpacity = m_IconOpacity / 100.0f;

            var colorL = m_LeftTextColor;
            colorL.a = textOpacity;

            if(leftGestureText != null)
                leftGestureText.color = colorL;
            
            colorL.a = iconOpacity;
            if(leftIcon != null)
                leftIcon.color = colorL;

            
            var colorR = m_RightTextColor;
            colorR.a = textOpacity;
            
            if(rightGestureText != null)
                rightGestureText.color = colorR;
            
            colorR.a = iconOpacity;
            if(rightIcon != null)
                rightIcon.color = colorR;
        }

        private void ApplyTextPositions()
        {
            if (leftGestureText != null) 
                leftGestureText.GetComponent<RectTransform>().anchoredPosition = new Vector2((-200f * m_X_Position) - 102.5f, -415f * m_Y_Position);
            if (rightGestureText != null) 
                rightGestureText.GetComponent<RectTransform>().anchoredPosition = new Vector2((200f * m_X_Position) - 102.5f, -415f * m_Y_Position);
        }

        private void ToggleIndicators(bool enable)
        {
            if (enable) MelonCoroutines.Start(CheckGesture());

            if(leftGestureText != null)
                leftGestureText.gameObject.SetActive(enable);
            if(rightGestureText != null)
                rightGestureText.gameObject.SetActive(enable);
        }

        public IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}
