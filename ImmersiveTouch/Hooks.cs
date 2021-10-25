using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UnhollowerBaseLib;

namespace ImmersiveTouch
{
    public class Hooks
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UpdateParticlesDelegate(IntPtr __0, bool __1);
        public static UpdateParticlesDelegate updateParticlesDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CollideDelegate(IntPtr __0, IntPtr __1, float __2);
        public static CollideDelegate collideDelegate;

        public static void ApplyPatches(bool hasTurbones)
        {
            if (!hasTurbones) ApplyHooks();
            ApplyHarmonyHooks();
        }

        public static unsafe void ApplyHooks()
        {
            try
            {
                IntPtr original = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(typeof(DynamicBone).GetMethod(nameof(DynamicBone.Method_Private_Void_Boolean_0))).GetValue(null);
                MelonUtils.NativeHookAttach((IntPtr)(&original), typeof(ImmersiveTouch).GetMethod(nameof(ImmersiveTouch.OnUpdateParticles), BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());
                updateParticlesDelegate = Marshal.GetDelegateForFunctionPointer<UpdateParticlesDelegate>(original);
            }
            catch (Exception e) { MelonLogger.Error($"Failed to patch: OnUpdateParticles\n{e}"); }

            try
            {
                IntPtr original = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(typeof(DynamicBoneCollider).GetMethod(nameof(DynamicBoneCollider.Method_Public_Void_byref_Vector3_Single_0))).GetValue(null);
                MelonUtils.NativeHookAttach((IntPtr)(&original), typeof(ImmersiveTouch).GetMethod(nameof(ImmersiveTouch.OnCollide), BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());
                collideDelegate = Marshal.GetDelegateForFunctionPointer<CollideDelegate>(original);
            }
            catch (Exception e) { MelonLogger.Error($"Failed to patch: OnCollide\n{e}"); }
        }

        public static void ApplyHarmonyHooks()
        {
            ImmersiveTouch.harmony.Patch(typeof(VRCAvatarManager).GetMethods().First(method =>
                method.Name.StartsWith("Method_Private_Boolean_ApiAvatar_GameObject_") && !method.Name.Contains("_PDM_")), null, new HarmonyMethod(typeof(ImmersiveTouch).GetMethod("OnAvatarChanged", BindingFlags.Public | BindingFlags.Static)), null);
        }
    }
}
