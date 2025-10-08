using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			// Debug.Log($"[ContainerTooltips]: ResolveStatusText for {_} invoked with data type {data?.GetType().FullName ?? "<null>"}");
			return GenerateContainerTooltip(data, Options.Instance.StatusLineLimit);
		}

		private static string ResolveTooltipText(string _, object data)
		{
			// Debug.Log($"[ContainerTooltips]: ResolveTooltipText for {_} invoked with data type {data?.GetType().FullName ?? "<null>"}");
			return GenerateContainerTooltip(data, Options.Instance.TooltipLineLimit);
		}

		public static string GenerateContainerTooltip(object? data, int lineLimit)
		{
			// Debug.Log($"[ContainerTooltips]: GenerateContainerTooltip called with data type {data?.GetType().FullName ?? "<null>"} lineLimit={lineLimit}");
			if (data is not Storage[] storages || storages.Length == 0)
			{
				Debug.LogWarning("[ContainerTooltips]: GenerateContainerTooltip received no storage data");
				return string.Empty;
			}

			// The game calls this method each frame that the status item is being displayed, so avoid doing expensive work every time
			var clock = GameClock.Instance;
			var tick = clock?.GetTime() ?? float.NaN;
			var key = storages[0].GetInstanceID() + (long)lineLimit << 32;
			if (resultCache.TryGetValue(key, out var entry) && entry.Tick == tick)
			{
				return entry.Result;
			}

			var summary = StorageContentsSummarizer.SummarizeStorageContents(storages, lineLimit);
			var resultBuilder = new StringBuilder(summary.Length + 50);
			resultBuilder.Append(Strings.Get(NameStringKey));
			resultBuilder.Append(": ");
			if (string.IsNullOrEmpty(summary))
			{
				resultBuilder.Append(Strings.Get(EmptyStringKey));
			}
			else
			{
				if (summary.Contains('\n'))
				{
					resultBuilder.Append('\n');
				}
				resultBuilder.Append(summary);
			}
			var result = resultBuilder.ToString();

			// Debug.Log($"[ContainerTooltips]: GenerateContainerTooltip computed new summary at tick={tick} for storages={string.Join(", ", storages.Select(storage => storage.name))} result={result.Replace("\n", ", ")}");
			resultCache[key] = new SummaryCacheEntry { Tick = tick, Result = result };			
			return result;
		}

		private struct SummaryCacheEntry
		{
			public float Tick;
			public string Result;
		}

		private static readonly Dictionary<long, SummaryCacheEntry> resultCache = [];
	}
}