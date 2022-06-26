using MelonLoader;
using System;
using System.Reflection;
using UnityEngine;

[assembly: MelonOptionalDependencies("ScaleGoesBrr")]

namespace DragFix
{
    public class ScaleGoesBrrEx
    {
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
            DragFix.m_AvatarHeightMod = newScale;
            DragFix.RefreshAvatarHeight();
        }
        
    }
}
