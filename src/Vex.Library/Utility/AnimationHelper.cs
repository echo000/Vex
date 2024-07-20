using PhilLibX.Media3D;

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
