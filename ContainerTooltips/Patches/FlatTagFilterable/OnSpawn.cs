using HarmonyLib;

namespace ContainerTooltips
{
    [HarmonyPatch(typeof(FlatTagFilterable), "OnSpawn")]
    public static class FlatTagFilterable_OnSpawn_Patch
    {
        private static void Postfix(FlatTagFilterable __instance)
        {
            Debug.Log($"[ContainerTooltips]: FlatTagFilterable_OnSpawn called for {UserMod.GetName(__instance)}");
            __instance?.gameObject.AddOrGet<FilterableSettingsBehaviour>();
        }
    }
}
