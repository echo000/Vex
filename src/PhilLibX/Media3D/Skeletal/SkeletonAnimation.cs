﻿using System.Collections.Generic;
using System.Numerics;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class that holds animation data for a <see cref="Skeleton"/>.
    /// </summary>
    public class SkeletonAnimation
    {
        /// <summary>
        /// Gets or Sets the skeleton tied to this animation, if any.
        /// </summary>
        public Skeleton? Skeleton { get; set; }

        /// <summary>
        /// Gets or Sets the targets that contain animation frames.
        /// </summary>
        public List<SkeletonAnimationTarget> Targets { get; set; }

        /// <summary>
        /// Gets or Sets the transform type.
        /// </summary>
        public TransformType TransformType { get; set; }

        /// <summary>
        /// Gets or Sets the transform space.
        /// </summary>
        public TransformSpace TransformSpace { get; set; }

        public SkeletonAnimation()
        {
            Targets = [];
            TransformType = TransformType.Unknown;
        }

        public SkeletonAnimation(Skeleton? skeleton)
        {
            Skeleton = skeleton;
            Targets = [];
        }

        public SkeletonAnimation(Skeleton? skeleton, int targetCount, TransformType type)
        {
            Skeleton = skeleton;
            Targets = new(targetCount);
            TransformType = type;
        }


        public void ScaleAnimation(float factor)
        {
            foreach (var target in Targets)
            {
                if (target.TranslationFrames != null)
                {
                    for (int i = 0; i < target.TranslationFrames.Count; i++)
                    {
                        var frame = target.TranslationFrames[i];
                        var scaledValue = new Vector3(frame.Value.X * factor, frame.Value.Y * factor, frame.Value.Z * factor);
                        target.TranslationFrames[i] = new(frame.Time, scaledValue);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new instance of an <see cref="SkeletonAnimationTarget"/> within this animation, if one already exists with this name, then that target is returned.
        /// </summary>
        /// <param name="name">Name of the target.</param>
        /// <returns>A new target that is added to this animation if it doesn't exist, otherwise an existing target with the given name.</returns>
        public SkeletonAnimationTarget CreateTarget(string name)
        {
            var idx = Targets.FindIndex(x => x.BoneName == name);

            if (idx != -1)
                return Targets[idx];

            var nTarget = new SkeletonAnimationTarget(name);
            Targets.Add(nTarget);

            return nTarget;
        }
    }
}
