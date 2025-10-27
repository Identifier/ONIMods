using HarmonyLib;

namespace ContainerTooltips
{
    [HarmonyPatch(typeof(TreeFilterable), "OnSpawn")]
    public static class TreeFilterable_OnSpawn_Patch
    {
        private static void Postfix(TreeFilterable __instance)
        {
            // Debug.Log($"[ContainerTooltips]: TreeFilterable_OnSpawn called for {UserMod.GetName(__instance)}");
            __instance?.gameObject.AddOrGet<FilterableSettingsBehaviour>();
        }
    }
}
