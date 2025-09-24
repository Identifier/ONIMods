using HarmonyLib;

namespace ZoomSpeed
{
	public class Patches
	{
		[HarmonyPatch(typeof(Db))]
		[HarmonyPatch("Initialize")]
		public class Db_Initialize_Patch
		{
			public static void Prefix()
			{
				Debug.Log("[ZoomSpeed]: I execute before Db.Initialize!");
			}

			public static void Postfix()
			{
				Debug.Log("[ZoomSpeed]: I execute after Db.Initialize!");
			}
		}
	}
}
