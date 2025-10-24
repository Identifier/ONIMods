using HarmonyLib;

namespace ContainerTooltips
{
    [HarmonyPatch(typeof(Filterable), "OnSpawn")]
    public static class Filterable_OnSpawn_Patch
    {
        private static void Postfix(Filterable __instance)
        {
            Debug.Log($"[ContainerTooltips]: Filterable_OnSpawn called for {UserMod.GetName(__instance)}");

            // Manually exclude GasFilterComplete since it already shows the filter on the tooltip in the base game
            if (__instance?.name == "GasFilterComplete" || __instance?.name == "LiquidFilterComplete")
            {
                Debug.Log($"[ContainerTooltips]: Filterable_OnSpawn skipping {UserMod.GetName(__instance)} since it already shows filter info in the base game");
                return;
            }
            __instance?.gameObject.AddOrGet<FilterableSettingsBehaviour>();
        }
    }
}
