using System.Linq;
using System.Text;

namespace ContainerTooltips
{
    public static class FilterSettingsSummarizer
    {
        public static string Summarize((Filterable[]? filterables, TreeFilterable[]? treeFilterables, FlatTagFilterable[]? flatTagFilterables) data, int maxLineCount)
        {
            if (data.filterables == null && data.treeFilterables == null && data.flatTagFilterables == null)
            {
                return string.Empty;
            }

            var filters =
                (data.filterables?.Select(f => f.SelectedTag) ?? []).Concat
                (data.treeFilterables?.SelectMany(tf => tf.GetTags()) ?? []).Concat
                (data.flatTagFilterables?.SelectMany(ftf => ftf.selectedTags) ?? []).ToList();
            if (filters.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(filters.Count * 100);
            var lineIndex = 0;
            foreach (var filter in filters.OrderBy(t => t.ProperNameStripLink()))
            {
                if (lineIndex > 0)
                {
                    sb.Append('\n');
                }

                sb.Append(filter.ProperName());

                lineIndex++;
                if (lineIndex >= maxLineCount)
                {
                    break;
                }
            }

            if (filters.Count > maxLineCount)
            {
                sb.Append('\n');
                sb.Append('+');
                sb.Append(filters.Count - maxLineCount);
                sb.Append(" more...");
            }

            return sb.ToString();
        }
    }
}
