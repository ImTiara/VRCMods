using MelonLoader;
using System.Collections;
using ActionMenuApi.Api;

[assembly: MelonInfo(typeof(ActionCamera.ActionCamera), "ActionCamera", "1.0.0", "ImTiara", "https://github.com/ImTiara/VRCMods")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace ActionCamera
{
    public class ActionCamera : MelonMod
    {
        public override void OnApplicationStart()
            => MelonCoroutines.Start(UiManagerInitializer());

        private void OnUiManagerInit()
        {
            MelonPreferences.CreateCategory(GetType().Name, "Action Camera");
            MelonPreferences.CreateEntry(GetType().Name, "OptionsMenu", false, "Use Options Menu (requires restart)");

            VRCActionMenuPage.AddSubMenu(MelonPreferences.GetEntryValue<bool>(GetType().Name, "OptionsMenu") ? ActionMenuPage.Options : ActionMenuPage.Main, "Camera", () =>
            {
                CustomSubMenu.AddButton("Spawn Camera", () =>
                {
                    Manager.EnableCamera();
                });

                CustomSubMenu.AddButton("Timed Picture (5 seconds)", () =>
                {
                    Manager.TakeTimedPicture();
                });

                CustomSubMenu.AddButton("Take Picture", () =>
                {
                    Manager.TakePicture();
                });

                CustomSubMenu.AddButton("Timed Picture (10 seconds)", () =>
                {
                    MelonCoroutines.Start(Manager.TakeCustomTimedPicture(10.0f));
                });

                CustomSubMenu.AddButton("Remove Camera", () =>
                {
                    Manager.DisableCamera();
                });
            });
        }

        private IEnumerator UiManagerInitializer()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null) yield return null;
            OnUiManagerInit();
        }
    }
}