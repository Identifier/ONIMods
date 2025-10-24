using HarmonyLib;

namespace ContainerTooltips
{
    [HarmonyPatch(typeof(Db), nameof(Db.Initialize))]
    public static class Db_Initialize_Patch
    {
        public static void Postfix()
        {
            Debug.Log("[ContainerTooltips]: Db.Initialize postfix running. Calling UserMod.InitializeStatusItems()");
            UserMod.InitializeStatusItems();
        }
    }
}