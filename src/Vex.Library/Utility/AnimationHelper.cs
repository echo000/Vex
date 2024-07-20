using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Vex.Library.Utility
{
    // A class to handle reading a CoD XAnim.
    public class CoDXAnimReader
    {
        // The array of bone names.
        public List<string> BoneNames;
        // The array of notetracks by frame index.
        public List<(string, ulong)> Notetracks;
        // The data byte array.
        public byte[] DataBytes;
        // The data byte array.
        public byte[] DataShorts;
        // The data byte array.
        public byte[] DataInts;
        // The data byte array.
        public byte[] RandomDataBytes;
        // The data byte array.
        public byte[] RandomDataShorts;
        // The data byte array.
        public byte[] RandomDataInts;
        // The data byte array.
        public byte[] Indices;

        public CoDXAnimReader()
        {
            BoneNames = [];
            Notetracks = [];
        }
    };
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
