using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib.Options;

namespace ZoomSpeed
{
    [JsonObject(MemberSerialization.OptOut)]
    [ModInfo("https://github.com/Identifier/ONIMods")]
    public sealed class Options : SingletonOptions<Options>, IOptions
    {
        [Option("Zoom Speed", "Zoom speed during a continuous zoom gesture. Increase this to zoom faster, decrease to zoom slower.")]
        [Limit(0.0f, 10f)]
        public float ZoomSpeed { get; set; } = 1.0f;

        [Option("Zoom Gesture Demarcation", "If no device zoom input has been detected for this amount of time, the next significant zoom input will start a new zoom gesture.")]
        [Limit(0f, 1f)]
        public float ZoomGestureDemarcation { get; set; } = 0.04f; // My MacBook trackpad seems to send inputs about every 0.033s during continuous scrolling, so it needs to be more than that, but less than the speed of each 'notch' on my mousewheel, which seems to be about 0.049s bewteen notches.

        [Option("Insignificant Zoom Distance", "Increase this to filter out small accidental inputs from pushing hard on trackpads or sensitive mouse wheels.")]
        [Limit(0f, 1f)]
        public float InsignificantZoomDistance { get; set; } = 0.10f; // My MacBook trackpad seems to send many inputs of 0.05 or 0.10 when just squishing my fingers slightly in the same position.

        //[Option("Game Zoom Step Distance", "How 'far' to consider the game's zoom distance in one zoom step. This is compared to the zoom distance (deltaY) sent from the mousewheel/trackpad.")]
        public float GameZoomStepDistance { get; set; } = 2.0f; // This seems to make the game zoom around the same as Google Maps.  The user can adjust ZoomSpeed to make it faster or slower.

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
