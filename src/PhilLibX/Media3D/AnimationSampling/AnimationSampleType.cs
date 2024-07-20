namespace PhilLibX.Media3D.AnimationSampling
{
    /// <summary>
    /// A class to handle sampling an <see cref="Animation"/> at arbitrary frames or in a linear fashion.
    /// </summary>
    public enum AnimationSampleType
    {
        Percentage,
        AbsoluteFrameTime,
        AbsoluteTime,
        DeltaTime,
    }
}
