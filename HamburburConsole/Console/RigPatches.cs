using HarmonyLib;

namespace HamburburConsole.Console;

//RigPatches are only needed here if you don't already have this in your mod
//This prevents the user getting banned if an admin has to disable their rig for any reason

public static class RigPatches
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.OnDisable))]
    public static class RigDisablePatch
    {
        private static bool Prefix(VRRig __instance) =>
                !__instance.isLocal;
    }

    [HarmonyPatch(typeof(VRRig), nameof(VRRig.PostTick))]
    public static class RigPostTickPatch
    {
        private static bool Prefix(VRRig __instance) =>
                !__instance.isLocal || __instance.enabled;
    }
}