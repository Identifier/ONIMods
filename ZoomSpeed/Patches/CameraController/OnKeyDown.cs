using HarmonyLib;
using UnityEngine;

namespace ZoomSpeed
{
    /// <summary>
    /// Patch to make zooming relative to the total distance scrolled instead of step-by-step zooming per action.
    /// This allows for smoother zooming especially when using a trackpad by taking the total distance into account
    /// rather than sending hundreds of full zoom commands to the game for each and every little movement.
    /// </summary>
    [HarmonyPatch(typeof(CameraController), nameof(CameraController.OnKeyDown))]
    public static class CameraController_OnKeyDown_Patch
    {
        private static float _lastZoomInputTime = float.NegativeInfinity;
        private static float _cumulativeDeltaY = 0f;

        public static bool Prefix(KButtonEvent e)
        {
            var result = true;

            if (IsZoomAction(e, out var action))
            {
                var currentTime = Time.unscaledTime;
                var timeSinceLastZoomInput = currentTime - _lastZoomInputTime;
                var deltaY = Input.mouseScrollDelta.y;
                var options = Options.Instance;

                // LogDistance("ScrollDelta", deltaY, timeSinceLastZoomInput);

                // Keep track of the input time so that we can detect separate zoom gestures.
                _lastZoomInputTime = currentTime;

                // If this is a new separate zoom gesture (not a continuous motion from the previous one),
                // reset the cumulative delta so that we zoom immediately (for responsiveness).
                if (timeSinceLastZoomInput >= options.ZoomGestureDemarcation && Mathf.Abs(deltaY) > options.InsignificantZoomDistance)
                {
                    _cumulativeDeltaY = Mathf.Sign(deltaY) * options.GameZoomStepDistance;
                    Debug.Log($"[ZoomSpeed]: New zoom gesture started (deltaY = {deltaY} {timeSinceLastZoomInput}s since last input).");
                }
                else
                {
                    // Keep track of the current fractional total zoom distance.
                    _cumulativeDeltaY += deltaY * options.ZoomSpeed;
                }

                // If we've zoomed past a step, allow the zoom action to go through.
                if (Mathf.Abs(_cumulativeDeltaY) >= options.GameZoomStepDistance)
                {
                    // LogDistance("Cumulative ", _cumulativeDeltaY);
                    _cumulativeDeltaY -= Mathf.Sign(_cumulativeDeltaY) * options.GameZoomStepDistance;
                    result = true;
                }
                else
                {
                    // Otherwise, block the zoom action.
                    e.TryConsume(action);
                    result = false;
                }
            }

            return result;
        }

        private static bool IsZoomAction(KButtonEvent e, out Action action)
        {
            if (e != null)
            {
                if (e.IsAction(Action.ZoomIn))
                {
                    action = Action.ZoomIn;
                    return true;
                }

                if (e.IsAction(Action.ZoomOut))
                {
                    action = Action.ZoomOut;
                    return true;
                }
            }

            action = Action.Invalid;
            return false;
        }

        private static void LogDistance(string label, float value, float timeSinceLast = float.NaN)
        {
            int len = Mathf.Abs((int)value);
            char ch = value < 0 ? '-' : value > 0 ? '+' : ' ';
            string bar = new(ch, len);
            const int barLength = 20;
            Debug.Log($"[ZoomSpeed]: {label} [{bar.PadLeft(barLength / 2).PadRight(barLength)}] {value}{(float.IsNaN(timeSinceLast) ? "" : $" (time since last: {timeSinceLast}s)")}");
        }
    }
}