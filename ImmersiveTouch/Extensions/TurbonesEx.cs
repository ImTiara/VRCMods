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
            => isPresent = MelonHandler.Mods.FirstOrDefault(x => x.Info.Name == "Turbones") != null;

        public static void RegisterColliderForCollisionFeedback(IntPtr colliderPtr, byte group)
            => Turbones.JigglySolverApi.RegisterColliderForCollisionFeedback(colliderPtr, group);

        public static ulong GetAndClearCollidingGroupsMask()
            => Turbones.JigglySolverApi.GetAndClearCollidingGroupsMask();

        public static void ClearCollisionFeedbackColliders()
            => Turbones.JigglySolverApi.ClearCollisionFeedbackColliders();

        public static void ExcludeBoneFromCollisionFeedback(IntPtr bonePtr)
            => Turbones.JigglySolverApi.ExcludeBoneFromCollisionFeedback(bonePtr);

        public static void ClearExcludedBonesFromCollisionFeedback()
            => Turbones.JigglySolverApi.ClearExcludedBonesFromCollisionFeedback();
    }
}
