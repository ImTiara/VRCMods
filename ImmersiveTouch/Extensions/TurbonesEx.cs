using System;
using System.Linq;
using MelonLoader;

[assembly: MelonOptionalDependencies("Turbones")]

namespace ImmersiveTouch.Extensions
{
    public class TurbonesEx
    {
        public static bool isPresent;

        public static void SetIsPresent()
        {
            var turbones = MelonHandler.Mods.FirstOrDefault(x => x.Info.Name == "Turbones");
            if (turbones == null) return;

            switch(turbones.Info.Version)
            {
                case "1.0.0":
                case "1.0.1":
                case "1.0.2":
                    MelonLogger.Error("You are using an outdated version of Turbones which is incompatible with\nImmersiveTouch.");
                    MelonLogger.Error("Please update Turbones to the latest version.");
                    return;
            }

            isPresent = true;
        }

        public static void RegisterColliderForCollisionFeedback(IntPtr colliderPtr, byte group)
            => Turbones.JigglySolverApi.RegisterColliderForCollisionFeedback(colliderPtr, group);

        public static ulong GetAndClearCollidingGroupsMask()
            => Turbones.JigglySolverApi.GetAndClearCollidingGroupsMask();

        public static void ExcludeBoneFromCollisionFeedback(IntPtr bonePtr)
            => Turbones.JigglySolverApi.ExcludeBoneFromCollisionFeedback(bonePtr);

        public static void UnregisterColliderForCollisionFeedback(IntPtr colliderPtr)
            => Turbones.JigglySolverApi.UnregisterColliderForCollisionFeedback(colliderPtr);

        public static void UnExcludeBoneFromCollisionFeedback(IntPtr bonePtr)
            => Turbones.JigglySolverApi.UnExcludeBoneFromCollisionFeedback(bonePtr);

        public static void UnregisterCollisionFeedbackColliders()
        {
            foreach (var container in ImmersiveTouch.registratedColliderPtrs)
            {
                foreach (var ptr in container.Value)
                {
                    UnregisterColliderForCollisionFeedback(ptr);
                }
            }
        }

        public static void UnregisterExcludedBonesFromCollisionFeedback()
        {
            foreach (var ptr in ImmersiveTouch.localDynamicBonePtrs)
            {
                UnExcludeBoneFromCollisionFeedback(ptr);
            }
        }
    }
}
