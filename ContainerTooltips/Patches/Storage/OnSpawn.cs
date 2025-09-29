using HarmonyLib;

namespace ContainerTooltips
{
    [HarmonyPatch(typeof(Storage), "OnSpawn")]
    public static class Storage_OnSpawn_Patch
    {
        private static void Postfix(Storage __instance)
        {
            // Debug.Log($"[ContainerTooltips]: Storage_OnSpawn called for {__instance?.name ?? "<null>"}");
            if (__instance == null)
            {
                Debug.LogWarning($"[ContainerTooltips]: Storage_OnSpawn instance invalid");
                return;
            }

            // Add our custom behavior to the storage game object, which has its own OnSpawn, OnCleanUp, etc that it will use to manage the status item
            __instance.gameObject.AddOrGet<StorageContentsBehaviour>();

            // Debug.Log($"[ContainerTooltips]: StorageContentsBehaviour ensured on {__instance.name}");
        }
    }
}