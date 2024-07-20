using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vex.Library.Utility
{
    class AnimationHelper
    {
        private static void AddBonesAndTargets(SkeletonAnimation skelAnim, string[] boneNames, TransformType transformType)
        {
            foreach (var boneName in boneNames)
            {
                skelAnim.Skeleton.Bones.Add(new SkeletonBone(boneName));
                skelAnim.CreateTarget(boneName).TransformType = transformType;
            }
        }
    }
}
