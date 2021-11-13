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
            foreach (var collider in ImmersiveTouch.allRegistratedColliders)
            {
                UnregisterColliderForCollisionFeedback(collider.Pointer);
            }
        }

        public static void UnregisterExcludedBonesFromCollisionFeedback()
        {
            foreach (var bone in ImmersiveTouch.localDynamicBones)
            {
                UnExcludeBoneFromCollisionFeedback(bone.Pointer);
            }
        }
    }
}
