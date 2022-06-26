using MelonLoader;
using System.Collections;
using UnityEngine;
using System.Reflection;
using System;

[assembly: MelonOptionalDependencies("ScaleGoesBrr")]

namespace ImmersiveTouch
{
    public class ScaleGoesBrrEx
    {
        public static bool isApplyingChange;

        public static void Setup()
        {
            foreach (var mod in MelonHandler.Mods)
            {
                if (mod.Info.Name != "Scale Goes Brr") continue;
                foreach (var type in mod.Assembly.GetTypes())
                {
                    if (type.FullName != "ScaleGoesBrr.ScaleGoesBrrMod") continue;

                    EventInfo eventInfo = type.GetEvent("OnAvatarScaleChanged", BindingFlags.Public | BindingFlags.Static);

                    Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, null, typeof(ScaleGoesBrrEx).GetMethod("OnAvatarScaleChanged", BindingFlags.Public | BindingFlags.Static));

                    eventInfo.AddEventHandler(null, handler);

                    break;
                }
                break;
            }
        }

        public static void OnAvatarScaleChanged(Transform avatarRoot, float newScale)
        {
            ImmersiveTouch.m_AvatarHeightMod = newScale;

            if (isApplyingChange) return;
            MelonCoroutines.Start(DelayedChange());
            isApplyingChange = true;
        }
        
        public static IEnumerator DelayedChange()
        {
            yield return new WaitForSeconds(2.5f);
            ImmersiveTouch.SetupAvatar();
            isApplyingChange = false;
        }
    }
}
