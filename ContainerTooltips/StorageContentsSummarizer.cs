using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ContainerTooltips
{
    public static class StorageContentsSummarizer
    {
        public static string SummarizeStorageContents(Storage storage, int maxLines)
        {
            // Debug.Log($"[ContainerTooltips]: BuildContentsSummary called for storage={storage?.name ?? "<null>"} maxLines={maxLines}");

            if (storage == null)
            {
                Debug.LogWarning("[ContainerTooltips]: Skipping null Storage");
                return string.Empty;
            }

            var items = storage.items;
            if (items == null || items.Count == 0)
            {
                // Debug.Log("[ContainerTooltips]: BuildContentsSummary found no items");
                return string.Empty;
            }

            var contentSummaries = new ContentSummaryCollection(0);
            var recursionGuard = new HashSet<int>();
            foreach (var item in items)
            {
                ProcessStorageItem(item, contentSummaries, recursionGuard);
            }

            if (contentSummaries.Count == 0)
            {
                Debug.LogWarning("[ContainerTooltips]: BuildContentsSummary created no summaries after processing items");
                return string.Empty;
            }

            var options = Options.Instance;
            contentSummaries.Sort(options.SortMode);

            var flattened = FlattenedSummaries(contentSummaries).ToList();
            if (flattened.Count == 0)
            {
                return string.Empty;
            }

            var maxLineCount = Mathf.Max(1, maxLines);
            var sb = new StringBuilder(flattened.Count * 100);
            var lineIndex = 0;
            foreach (var summary in FlattenedSummaries(contentSummaries))
            {
                if (!(summary.Units > 0 || summary.Calories > 0 || summary.Diseases?.Count > 0 || summary.Children?.Count > 0))
                {
                    Debug.Log($"[ContainerTooltips]: Skipping content summary for {storage.name}'s {summary.Name} due to no substantial information.");
                    continue;
                }

                if (lineIndex > 0)
                {
                    sb.Append('\n');
                }

                sb.Append(FormatContentSummary(summary, options));

                lineIndex++;
                if (lineIndex >= maxLineCount)
                {
                    break;
                }
            }

            if (flattened.Count > maxLineCount)
            {
                sb.Append('\n');
                sb.Append('+');
                sb.Append(flattened.Count - maxLineCount);
                sb.Append(" more...");
            }

            return (lineIndex > 1 ? "\n" : string.Empty) + sb.ToString();
        }

        private static void ProcessStorageItem(GameObject? item, ContentSummaryCollection contentSummaries, HashSet<int> recursionGuard)
        {
            // Debug.Log($"[ContainerTooltips]: Processing storage item {item?.name} at depth {contentSummaries.Depth}");

            if (item == null)
            {
                Debug.LogWarning("[ContainerTooltips]: Skipping null GameObject");
                return;
            }

            var instanceId = item.GetInstanceID();
            if (!recursionGuard.Add(instanceId))
            {
                Debug.LogWarning($"[ContainerTooltips]: Detected recursive storage reference on {item.name}, aborting nested inspection");
                return;
            }

            try
            {
                var prefab = item.GetComponent<KPrefabID>();
                if (prefab == null)
                {
                    Debug.LogWarning($"[ContainerTooltips]: GameObject {item.name} missing KPrefabID");
                    return;
                }

                var summary = contentSummaries.GetOrAdd(prefab.PrefabTag, prefab.GetProperName());
                summary.Count++;

                if (item.TryGetComponent(out PrimaryElement element))
                {
                    summary.Mass += element.Mass;
                    summary.Units += element.Units;
                    summary.TemperatureSum += element.Temperature;
                    summary.TemperatureSamples++;

                    if (element.DiseaseCount > 0)
                    {
                        summary.AddDisease(element.DiseaseIdx, element.DiseaseCount);
                    }
                }
                else if (item.TryGetComponent(out Pickupable pickupable))
                {
                    Debug.Log($"[ContainerTooltips]: Item {item.name} isn't a PrimaryElement, but it is Pickupable ({pickupable.TotalAmount})");
                    summary.Units += pickupable.TotalAmount;
                }

                if (item.TryGetComponent(out Edible edible))
                {
                    summary.Calories += edible.Calories;
                }

                if (item.TryGetComponent(out Storage nestedStorage) && nestedStorage.items != null && nestedStorage.items.Count > 0)
                {
                    var children = summary.EnsureChildren();
                    foreach (var child in nestedStorage.items)
                    {
                        ProcessStorageItem(child, children, recursionGuard);
                    }
                }

                // Debug.Log($"[ContainerTooltips]: Processed storage item {summary}");
            }
            finally
            {
                recursionGuard.Remove(instanceId);
            }
        }

        private static string FormatContentSummary(ContentSummary summary, Options options)
        {
            var appendItemCount = summary.Count > 1;
            string amount;

            if (summary.Mass > 0f)
            {
                amount = GameUtil.GetFormattedMass(summary.Mass, massFormat: (GameUtil.MetricMassFormat)options.MassUnits);
            }
            else if (summary.Units > 0f)
            {
                amount = GameUtil.GetFormattedUnits(summary.Units);
            }
            else
            {
                Debug.Log($"[ContainerTooltips]: Item {summary} doesn't have any Mass or Units");
                amount = $"{summary.Count} {(summary.Count == 1 ? "item" : "items")}";
                appendItemCount = false;
            }

            if (summary.Calories > 0f)
            {
                amount += " (" + GameUtil.GetFormattedCalories(summary.Calories) + ")";
            }

            if (appendItemCount)
            {
                amount += $" ({summary.Count} {(summary.Count == 1 ? "item" : "items")})";
            }

            var temperatureSamples = summary.TemperatureSamples > 0 ? summary.TemperatureSamples : summary.Count;
            var temperature = GameUtil.GetFormattedTemperature(summary.TemperatureSum / Mathf.Max(temperatureSamples, 1));
            if (temperatureSamples > 1)
            {
                temperature = "~" + temperature;
            }

            var formatted = GetIndent(summary.Depth) + string.Format(options.LineFormat, summary.Name, amount, temperature);

            var germInfo = FormatDiseases(summary);
            if (!string.IsNullOrEmpty(germInfo))
            {
                formatted += "\n" + GetIndent(summary.Depth + 1) + germInfo;
            }

            return formatted;
        }

        private static string GetIndent(int depth)
        {
            return new string(' ', depth * 4);
        }

        private static string FormatDiseases(ContentSummary summary)
        {
            if (summary.TotalDiseaseCount <= 0 || summary.Diseases == null || summary.Diseases.Count == 0)
            {
                return string.Empty;
            }

            var totalText = GameUtil.GetFormattedDiseaseAmount(summary.TotalDiseaseCount);
            var diseaseNames = string.Join(" + ", summary.Diseases.Select(d => GameUtil.GetFormattedDiseaseName(d.Key)));
            return $"{totalText} [{diseaseNames}]";
        }

        private static IEnumerable<ContentSummary> FlattenedSummaries(ContentSummaryCollection entries)
        {
            foreach (var item in entries.Items)
            {
                yield return item;
                if (item.Children != null)
                {
                    foreach (var nested in FlattenedSummaries(item.Children))
                    {
                        yield return nested;
                    }
                }
            }
        }

        private sealed class ContentSummary
        {
            public ContentSummary(string name, Tag tag, int depth)
            {
                Name = name;
                Tag = tag;
                Depth = depth;
            }

            public string Name { get; }
            public Tag Tag { get; }
            public int Depth { get; }
            public int Count { get; set; }
            public float Mass { get; set; }
            public float Units { get; set; }
            public float Calories { get; set; }
            public float TemperatureSum { get; set; }
            public int TemperatureSamples { get; set; }
            public int TotalDiseaseCount { get; private set; }
            public Dictionary<byte, int>? Diseases { get; private set; }
            public ContentSummaryCollection? Children { get; private set; }

            public ContentSummaryCollection EnsureChildren()
            {
                return Children ??= new(Depth + 1);
            }

            public Dictionary<byte, int> EnsureDiseases()
            {
                return Diseases ??= [];
            }

            public void AddDisease(byte diseaseIdx, int amount)
            {
                var diseases = EnsureDiseases();
                diseases.TryGetValue(diseaseIdx, out var existing);
                diseases[diseaseIdx] = existing + amount;

                TotalDiseaseCount += amount;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                AppendTo(sb, this, 0);
                return sb.ToString();

                static void AppendTo(StringBuilder builder, ContentSummary summary, int indentLevel)
                {
                    var indent = new string(' ', indentLevel * 2);
                    builder.Append(indent);
                    builder.Append("ContentSummary {");
                    builder.Append(" Name=").Append('"').Append(summary.Name).Append('"');
                    builder.Append(", Tag=").Append(summary.Tag);
                    builder.Append(", Depth=").Append(summary.Depth);
                    builder.Append(", Count=").Append(summary.Count);
                    builder.Append(", Mass=").Append(summary.Mass);
                    builder.Append(", Units=").Append(summary.Units);
                    builder.Append(", Calories=").Append(summary.Calories);
                    builder.Append(", TemperatureSum=").Append(summary.TemperatureSum);
                    builder.Append(", TemperatureSamples=").Append(summary.TemperatureSamples);
                    builder.Append(", TotalDiseaseCount=").Append(summary.TotalDiseaseCount);
                    builder.Append(", Diseases=");

                    if (summary.Diseases == null || summary.Diseases.Count == 0)
                    {
                        builder.Append("[]");
                    }
                    else
                    {
                        builder.Append('[');
                        var first = true;
                        foreach (var kvp in summary.Diseases)
                        {
                            if (!first)
                            {
                                builder.Append(", ");
                            }

                            builder.Append(kvp.Key);
                            builder.Append(':');
                            builder.Append(kvp.Value);
                            first = false;
                        }

                        builder.Append(']');
                    }

                    builder.Append(", Children=");
                    if (summary.Children == null || summary.Children.Count == 0)
                    {
                        builder.Append("[]");
                    }
                    else
                    {
                        builder.AppendLine("[");
                        var children = summary.Children.Items;
                        for (var i = 0; i < children.Count; i++)
                        {
                            AppendTo(builder, children[i], indentLevel + 1);
                            if (i < children.Count - 1)
                            {
                                builder.AppendLine(",");
                            }
                            else
                            {
                                builder.AppendLine();
                            }
                        }

                        builder.Append(indent);
                        builder.Append(']');
                    }

                    builder.Append(" }");
                }
            }
        }

        private sealed class ContentSummaryCollection
        {
            private readonly int depth;
            private readonly Dictionary<Tag, ContentSummary> lookup = [];
            private readonly List<ContentSummary> items = [];

            public ContentSummaryCollection(int depth)
            {
                this.depth = depth;
            }

            public IReadOnlyList<ContentSummary> Items => items;
            public int Count => items.Count;
            public int Depth => depth;

            public ContentSummary GetOrAdd(Tag tag, string name)
            {
                if (!lookup.TryGetValue(tag, out var summary))
                {
                    summary = new ContentSummary(name, tag, depth);
                    lookup.Add(tag, summary);
                    items.Add(summary);
                }

                return summary;
            }

            public void Sort(ContentSortMode sortMode)
            {
                if (sortMode != ContentSortMode.Default)
                {
                    items.Sort(new ContentSummaryComparer(sortMode));
                    foreach (var item in items)
                    {
                        item.Children?.Sort(sortMode);
                    }
                }
            }
        }

        private sealed class ContentSummaryComparer : IComparer<ContentSummary>
        {
            private readonly ContentSortMode mode;

            public ContentSummaryComparer(ContentSortMode mode)
            {
                this.mode = mode;
            }

            public int Compare(ContentSummary? x, ContentSummary? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (x is null)
                {
                    return -1;
                }

                if (y is null)
                {
                    return 1;
                }

                return mode switch
                {
                    ContentSortMode.Alphabetical => CompareAlphabetical(x, y),
                    ContentSortMode.Amount => CompareAmount(x, y),
                    _ => 0
                };
            }

            private static int CompareAlphabetical(ContentSummary x, ContentSummary y)
            {
                var result = string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
                return result != 0 ? result : CompareAmount(x, y);
            }

            private static int CompareAmount(ContentSummary x, ContentSummary y)
            {
                var result = y.Mass.CompareTo(x.Mass);
                if (result != 0)
                {
                    return result;
                }

                result = y.Calories.CompareTo(x.Calories);
                if (result != 0)
                {
                    return result;
                }

                result = y.Units.CompareTo(x.Units);
                if (result != 0)
                {
                    return result;
                }

                result = y.Count.CompareTo(x.Count);
                return result != 0 ? result : string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase);
            }
        }
    }
}
