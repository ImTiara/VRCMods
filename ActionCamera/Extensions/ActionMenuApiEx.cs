using System;
using System.Linq;
using ActionMenuApi.Api;
using ActionMenuApi.Types;
using MelonLoader;
using UnityEngine;

[assembly: MelonOptionalDependencies("ActionMenuApi")]

namespace ActionCamera.Extensions
{
    public class ActionMenuApiEx
    {
        public static bool isPresent;

        public static void SetIsPresent()
            => isPresent = MelonHandler.Mods.FirstOrDefault(x => x.Info.Name == "ActionMenuApi") != null;

        public static void AddSubMenu(ActionMenuPage pageType, string text, Action openFunc, Texture2D icon = null, bool locked = false, Action closeFunc = null, Insertion insertion = Insertion.Post)
            => VRCActionMenuPage.AddSubMenu(pageType, text, openFunc, icon, locked, closeFunc, insertion);

        public static void AddButton(string text, Action triggerEvent, Texture2D icon = null, bool locked = false)
            => CustomSubMenu.AddButton(text, triggerEvent, icon, locked);
    }
}
