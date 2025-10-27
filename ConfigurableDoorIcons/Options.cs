using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ConfigurableDoorIcons
{
    [JsonObject(MemberSerialization.OptOut)]
    [ModInfo("https://github.com/Identifier/ONIMods")]
    public sealed class Options : SingletonOptions<Options>, IOptions
    {
        [Option("Show on Open Doors", "Whether to show icons on top of manually opened doors.")]
        public bool ShowOnOpenDoors { get; set; } = true;

        [Option("Show on Locked Doors", "Whether to show icons on top of manually locked doors.")]
        public bool ShowOnLockedDoors { get; set; } = true;

        [Option("Transparency", "Transparency of door icons (1 = no transparency).")]
        [Limit(0.1f, 1.0f)]
        public float Transparency { get; set; } = 0.5f;

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
