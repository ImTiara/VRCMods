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
        public delegate void AvatarChangedDelegate(IntPtr instance, IntPtr __0, IntPtr __1, IntPtr __2);
        public static AvatarChangedDelegate avatarChangedDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void UpdateParticlesDelegate(IntPtr __0, bool __1);
        public static UpdateParticlesDelegate updateParticlesDelegate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void CollideDelegate(IntPtr __0, IntPtr __1, float __2);
        public static CollideDelegate collideDelegate;

        public static unsafe void ApplyPatches()
        {
            try
            {
                // PDM's are scary:(
                MethodInfo avatarChangedMethod = typeof(VRCAvatarManager).GetMethods().FirstOrDefault(method => method.Name.StartsWith("Method_Private_Void_ApiAvatar_GameObject_Action_1_Boolean_"));

                IntPtr original = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(avatarChangedMethod).GetValue(null);
                MelonUtils.NativeHookAttach((IntPtr)(&original), typeof(ImmersiveTouch).GetMethod(nameof(ImmersiveTouch.OnAvatarChanged), BindingFlags.Static | BindingFlags.Public)!.MethodHandle.GetFunctionPointer());
                avatarChangedDelegate = Marshal.GetDelegateForFunctionPointer<AvatarChangedDelegate>(original);
            }
            catch (Exception e) { MelonLogger.Error($"Failed to patch: OnAvatarChanged\n{e}"); }

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
    }
}
