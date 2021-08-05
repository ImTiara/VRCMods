using MelonLoader;
using System.Collections;
using ActionCamera.Extensions;

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
            ActionMenuApiEx.SetIsPresent();

            if (!ActionMenuApiEx.isPresent)
            {
                MelonLogger.Error("Missing ActionMenuApi");
                return;
            }

            ActionMenuApiEx.AddSubMenu(ActionMenuApi.Api.ActionMenuPage.Main, "Camera", () =>
            {
                ActionMenuApiEx.AddButton("Take Picture", () =>
                {
                    
                });

                ActionMenuApiEx.AddButton("Timed Picture (5 seconds)", () =>
                {

                });

                ActionMenuApiEx.AddButton("Timed Picture (10 seconds)", () =>
                {

                });

                ActionMenuApiEx.AddButton("Remove Camera", () =>
                {

                });

                ActionMenuApiEx.AddButton("Spawn Camera", () =>
                {

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