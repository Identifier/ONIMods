using HarmonyLib;

namespace ContainerTooltips
{
	public class Patches
	{
		[HarmonyPatch(typeof(Db))]
		[HarmonyPatch("Initialize")]
		public class Db_Initialize_Patch
		{
			public static void Prefix()
			{
				Debug.Log("[ContainerTooltips]: I execute before Db.Initialize!");
			}

			public static void Postfix()
			{
				Debug.Log("[ContainerTooltips]: I execute after Db.Initialize!");
			}
		}
	}
}
