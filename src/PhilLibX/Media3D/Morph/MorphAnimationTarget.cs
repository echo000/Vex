using System.Collections.Generic;
using System.Numerics;

namespace PhilLibX.Media3D
{
    public class MorphAnimationTarget(string morphName)
    {
        /// <summary>
        /// Gets or Sets the name of the bone this channel targets
        /// </summary>
        public string MorphName { get; set; } = morphName;

        /// <summary>
        /// Gets or Sets the translation frames.
        /// </summary>
        public List<AnimationFrame<Vector3>>? TranslationFrames { get; set; }

        /// <summary>
        /// Gets the number of translations frames.
        /// </summary>
        public int TranslationFrameCount => TranslationFrames != null ? TranslationFrames.Count : 0;

        public void AddTranslationFrame(float time, Vector3 value)
        {
            TranslationFrames ??= [];
            TranslationFrames.Add(new(time, value));
        }
    }
}
