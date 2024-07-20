namespace PhilLibX.Media3D
{
    /// <summary>
    /// An enum to define Animation Transform Type
    /// </summary>
    public enum TransformType
    {
        /// <summary>
        /// Type is unknown.
        /// </summary>
        Unknown,

        /// <summary>
        /// Animation Data is same as it parent bones/animation
        /// </summary>
        Parent,

        /// <summary>
        /// Animation Data is relative to parent bind pose
        /// </summary>
        Relative,

        /// <summary>
        /// Animation Data is relative to zero
        /// </summary>
        Absolute,

        /// <summary>
        /// Animation Data is applied to existing animation data in the scene
        /// </summary>
        Additive,

        /// <summary>
        /// Animation Data is relative and contains delta data (Whole model movement) Delta tag name must be set!
        /// </summary>
        Delta,
    }
}
