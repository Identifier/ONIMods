using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;
using UnityEngine;

namespace ContainerTooltips
{
	public class UserMod : UserMod2
	{
		public const string ModStringsPrefix = "CONTAINERTOOLTIPS";

		public const string ContentsStatusItemId = "CONTAINERTOOLTIPSTATUSITEM";
		private const string ContentsStringsPrefix = $"STRINGS.{ModStringsPrefix}.STATUSITEMS.{ContentsStatusItemId}";
		public const string ContentsNameStringKey = ContentsStringsPrefix + ".NAME";
		public const string ContentsTooltipStringKey = ContentsStringsPrefix + ".TOOLTIP";
		public const string ContentsEmptyStringKey = ContentsStringsPrefix + ".EMPTY";

		public const string FiltersStatusItemId = "CONTAINERTOOLTIPFILTERSTATUSITEM";
		private const string FiltersStringsPrefix = $"STRINGS.{ModStringsPrefix}.STATUSITEMS.{FiltersStatusItemId}";
		public const string FiltersNameStringKey = FiltersStringsPrefix + ".NAME";
		public const string FiltersTooltipStringKey = FiltersStringsPrefix + ".TOOLTIP";
		public const string FiltersEmptyStringKey = FiltersStringsPrefix + ".EMPTY";

		public static StatusItem? ContentsStatusItem;
		public static StatusItem? FiltersStatusItem;

		public override void OnLoad(Harmony harmony)
		{
			base.OnLoad(harmony);
			PUtil.InitLibrary();
			new POptions().RegisterOptions(this, typeof(Options));
		}

		public static void InitializeStatusItems()
		{
			var contentsInitialized = ContentsStatusItem != null;
			var filterInitialized = FiltersStatusItem != null;

			if (contentsInitialized && filterInitialized)
			{
				Debug.Log("[ContainerTooltips]: Status items already initialized, skipping creation");
				return;
			}

			AddStringIfMissing(ContentsNameStringKey, "Contents");
			AddStringIfMissing(ContentsTooltipStringKey, "Shows the items in internal storage.");
			AddStringIfMissing(ContentsEmptyStringKey, "None");

			AddStringIfMissing(FiltersNameStringKey, "Filters");
			AddStringIfMissing(FiltersTooltipStringKey, "Shows the current filter configuration.");
			AddStringIfMissing(FiltersEmptyStringKey, "None");

			if (!contentsInitialized)
			{
				var contentsStatusItem = new StatusItem(
					ContentsStatusItemId,
					ModStringsPrefix,
					"status_item_info",
					StatusItem.IconType.Info,
					NotificationType.Neutral,
					false,
					OverlayModes.None.ID);

				contentsStatusItem.resolveStringCallback = ResolveContentsStatusText;
				contentsStatusItem.resolveTooltipCallback = ResolveContentsTooltipText;

				ContentsStatusItem = contentsStatusItem;
				Debug.Log("[ContainerTooltips]: ContentsStatusItem created and callbacks assigned");
			}

			if (!filterInitialized)
			{
				var filtersStatusItem = new StatusItem(
					FiltersStatusItemId,
					ModStringsPrefix,
					"status_item_info",
					StatusItem.IconType.Info,
					NotificationType.Neutral,
					false,
					OverlayModes.None.ID);

				filtersStatusItem.resolveStringCallback = ResolveFilterStatusText;
				filtersStatusItem.resolveTooltipCallback = ResolveFilterTooltipText;

				FiltersStatusItem = filtersStatusItem;
				Debug.Log("[ContainerTooltips]: FiltersStatusItem created and callbacks assigned");
			}
		}

		private static string ResolveContentsStatusText(string _, object data)
		{
			return ResolveContentsText(data, Options.Instance.StatusLineLimit);
		}

		private static string ResolveContentsTooltipText(string _, object data)
		{
			return ResolveContentsText(data, Options.Instance.TooltipLineLimit);
		}

		private static string ResolveFilterStatusText(string _, object data)
		{
            return ResolveFilterText(data, Options.Instance.StatusLineLimit);
		}

		private static string ResolveFilterTooltipText(string _, object data)
        {
            return ResolveFilterText(data, Options.Instance.TooltipLineLimit);
        }

		private static string ResolveContentsText(object data, int lineLimit)
		{
			if (data is not Storage[] storages || storages.Length == 0)
			{
				Debug.LogWarning("[ContainerTooltips]: ResolveContentsText received no storage data");
				return string.Empty;
			}
			var key = storages[0].GetInstanceID() + (long)lineLimit << 32;
			return GetLatestText(key, () => StorageContentsSummarizer.Summarize(storages, lineLimit), ContentsNameStringKey, ContentsEmptyStringKey);
		}

		private static string ResolveFilterText(object data, int lineLimit)
		{
			if (data is not ValueTuple<Filterable[]?, TreeFilterable[]?, FlatTagFilterable[]?> filterData)
			{
				Debug.LogWarning("[ContainerTooltips]: ResolveFilterText received no filter data");
				return string.Empty;
			}
			var key = 0L; // Don't actually cache filter text, since the user can change filters even when the game is paused.
			return GetLatestText(key, () => FilterSettingsSummarizer.Summarize(filterData, lineLimit), FiltersNameStringKey, FiltersEmptyStringKey);
		}

		public static string GetLatestText(long key, Func<string> summaryFunc, string nameKey, string emptyKey)
		{
			// The game calls this method each frame that the status item is being displayed, so avoid doing expensive work every time
			var clock = GameClock.Instance;
			var tick = clock?.GetTime() ?? float.NaN;
			if (resultCache.TryGetValue(key, out var entry) && entry.Tick == tick)
			{
				return entry.Result;
			}

			var summary = summaryFunc();
			var resultBuilder = new StringBuilder(summary.Length + 50);
			resultBuilder.Append(Strings.Get(nameKey));

			if (summary.Contains('\n'))
			{
				resultBuilder.Append(":\n");
			}
			else
			{
				resultBuilder.Append(": ");
			}

			if (string.IsNullOrEmpty(summary))
			{
				resultBuilder.Append(Strings.Get(emptyKey));
			}
			else
			{
				resultBuilder.Append(summary);
			}

			var result = resultBuilder.ToString();
			if (key != 0)
			{
				resultCache[key] = new SummaryCacheEntry { Tick = tick, Result = result };
			}
			return result;
		}

		private static void AddStringIfMissing(string key, string value)
		{
			if (!Strings.TryGet(key, out _))
			{
				Strings.Add(key, value);
			}
		}

		private struct SummaryCacheEntry
		{
			public float Tick;
			public string Result;
		}

		private static readonly Dictionary<long, SummaryCacheEntry> resultCache = [];

		public static string GetName(UnityEngine.Object? o)
		{
			return o != null ? o.name : "<null>";
		}

		public static string GetNames(UnityEngine.Object[]? os)
		{
			return os?.Length > 0 ? string.Join(", ", Array.ConvertAll(os, s => s?.name ?? "<null>")) : "<none>";
		}
	}
}
