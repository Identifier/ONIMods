using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ContainerTooltips
{
    public enum ContentSortMode
    {
        Default,
        Amount,
        Alphabetical
    }

    public enum MassDisplayMode
    {
        Default = GameUtil.MetricMassFormat.UseThreshold,
        Kilogram = GameUtil.MetricMassFormat.Kilogram,
        Gram = GameUtil.MetricMassFormat.Gram,
        Tonne = GameUtil.MetricMassFormat.Tonne
    }

    [JsonObject(MemberSerialization.OptOut)]
    [ModInfo("https://github.com/Identifier/ONIMods")]
    public sealed class Options : SingletonOptions<Options>, IOptions
    {
        [Option("Sort Order", "Controls how container contents are ordered in the displayed list.")]
        public ContentSortMode SortMode { get; set; } = ContentSortMode.Default;

        [Option("Mass Units", "Preferred units when displaying mass values.")]
        public MassDisplayMode MassUnits { get; set; } = MassDisplayMode.Default;

        [Option("Content List Limit", "Maximum number of contents to show in the hover card/tooltip (also appears in the information window's Status panel).")]
        [Limit(1, 100)]
        public int StatusLineLimit { get; set; } = 5;

        [Option("Detailed List Limit", "Maximum number of contents to show when hovering over the content list itself in the information window's Status panel.")]
        [Limit(1, 100)]
        public int TooltipLineLimit { get; set; } = 20;

        [Option("Format", "Format string for each line of the contents list. Use {0} for item name, {1} for item amount, and {2} for temperature.")]
        public string LineFormat { get; set; } = "{1} of {0} at {2}";

        public void OnOptionsChanged()
        {
            // Update the singleton Options.Instance when options are changed in the menu, so we don't need to restart the game.
            instance = POptions.ReadSettings<Options>() ?? new Options();
        }

        public IEnumerable<IOptionsEntry> CreateOptions()
        {
            // We don't need this method, but if you implement IOptions.OnOptionsChanged then you need to implement this too.
            yield break;
        }
    }
}
