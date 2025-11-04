using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace ConfigurableDoorIcons
{
	public class UserMod : UserMod2
    {
        public static string? RootPath;

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();
			new POptions().RegisterOptions(this, typeof(Options));

			if (!DoorIcons.Pathfinder.FindModRootPath("ConfigurableDoorIcons", "3598063686", out RootPath))
			{
				Debug.LogError("[ConfigurableDoorIcons] Could not find mod root path!");
			}
			else
			{
				Debug.Log($"[ConfigurableDoorIcons] Mod loaded with root path {RootPath}");
			}
		}
	}
}
