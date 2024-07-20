using System.Collections.Generic;

namespace PhilLibX.Media3D
{
    public class MorphAnimation
    {
        /// <summary>
        /// Gets or Sets the targets that contain animation frames.
        /// </summary>
        public List<MorphAnimationTarget> Targets { get; set; }

        public MorphAnimation()
        {
            Targets = [];
        }

        /// <summary>
        /// Creates a new instance of an <see cref="MorphAnimationTarget"/> within this animation, if one already exists with this name, then that target is returned.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <returns>A new target that is added to this animation if it doesn't exist, otherwise an existing target with the given name.</returns>
        public MorphAnimationTarget CreateTarget(string name)
        {
            var idx = Targets.FindIndex(x => x.MorphName == name);

            if (idx != -1)
                return Targets[idx];

            var nTarget = new MorphAnimationTarget(name);
            Targets.Add(nTarget);

            return nTarget;
        }
    }
}