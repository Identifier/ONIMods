using System;
using System.Collections.Generic;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace ContainerTooltips
{
	public class UserMod : UserMod2
	{
		public const string ModStringsPrefix = "CONTAINERTOOLTIPS";
		public const string StatusItemId = "CONTAINERTOOLTIPSTATUSITEM";

		public const string ComposedPrefix = $"STRINGS.{ModStringsPrefix}.STATUSITEMS.{StatusItemId}";
		public const string NameStringKey = ComposedPrefix + ".NAME";
		public const string TooltipStringKey = ComposedPrefix + ".TOOLTIP";
		public const string EmptyStringKey = ComposedPrefix + ".EMPTY";

		public static StatusItem? ContentsStatusItem;

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();
			new POptions().RegisterOptions(this, typeof(Options));
		}

		public static void InitializeStatusItem()
		{
			if (ContentsStatusItem != null)
			{
				Debug.Log("[ContainerTooltips]: ContentsStatusItem already initialized, skipping creation");
				return;
			}

			// Add localization strings for our status item, based on the same logic as the internal StatusItem constructor (although I don't know where these are used)
			Strings.Add(NameStringKey, "Contents");
			Strings.Add(TooltipStringKey, "Shows the items in internal storage.");
			Strings.Add(EmptyStringKey, "None");

			// This is the object that's needed to add a status item to a KSelectable, which will appear in the object's hover card/tooltip as well as the top of the info panel when selected
			var statusItem = new StatusItem(
				StatusItemId,
				ModStringsPrefix,
				"status_item_info",
				StatusItem.IconType.Info,
				NotificationType.Neutral,
				false,
				OverlayModes.None.ID);

			// These are the callbacks that will be invoked on each frame while the status item is being displayed on screen
			statusItem.resolveStringCallback = ResolveStatusText;
			statusItem.resolveTooltipCallback = ResolveTooltipText;

			// Keep it for cleanup and/or replacing later if needed
			ContentsStatusItem = statusItem;

			Debug.Log("[ContainerTooltips]: ContentsStatusItem created and callbacks assigned");
		}

		private static string ResolveStatusText(string _, object data)
		{
			var clock = GameClock.Instance;
			var tick = clock?.GetTime() ?? float.NaN;
			// Debug.Log($"[ContainerTooltips]: ResolveStatusText invoked at {tick} with data type {data?.GetType().FullName ?? "<null>"}");

			if (data is not Storage storage)
			{
				Debug.LogWarning("[ContainerTooltips]: ResolveStatusText received non-storage data");
				return string.Empty;
			}

			var key = storage.GetInstanceID();
			if (statusTextCache.TryGetValue(key, out var entry) && entry.Tick == tick)
			{
				return entry.Result;
			}

			var summary = StorageContentsSummarizer.SummarizeStorageContents(storage, Options.Instance.StatusLineLimit);
			var result = Strings.Get(NameStringKey) + ": " + (string.IsNullOrEmpty(summary) ? Strings.Get(EmptyStringKey) : summary);
			statusTextCache[key] = new SummaryCacheEntry { Tick = tick, Result = result };
			// Debug.Log($"[ContainerTooltips]: ResolveStatusText computed new summary at tick={tick} for storage={storage.name} result={result.Replace("\n", ", ")}");
			return result;
		}

		private static string ResolveTooltipText(string _, object data)
		{
			var clock = GameClock.Instance;
			var tick = clock?.GetTime() ?? float.NaN;
			// Debug.Log($"[ContainerTooltips]: ResolveTooltipText invoked at {tick} with data type {data?.GetType().FullName ?? "<null>"}");

			if (data is not Storage storage)
			{
				Debug.LogWarning("[ContainerTooltips]: ResolveTooltipText received non-storage data");
				return string.Empty;
			}

			var key = storage.GetInstanceID();
			if (tooltipTextCache.TryGetValue(key, out var entry) && entry.Tick == tick)
			{
				return entry.Result;
			}

			var summary = StorageContentsSummarizer.SummarizeStorageContents(storage, Options.Instance.TooltipLineLimit);
			var result = Strings.Get(NameStringKey) + ": " + (string.IsNullOrEmpty(summary) ? Strings.Get(EmptyStringKey) : summary);
			tooltipTextCache[key] = new SummaryCacheEntry { Tick = tick, Result = result };
			// Debug.Log($"[ContainerTooltips]: ResolveTooltipText computed new summary at tick={tick} for storage={storage.name} result={result.Replace("\n", ", ")}");
			return result;
		}

		private struct SummaryCacheEntry
		{
			public float Tick;
			public string Result;
		}

		private static readonly Dictionary<int, SummaryCacheEntry> statusTextCache = [];
		private static readonly Dictionary<int, SummaryCacheEntry> tooltipTextCache = [];
	}
}