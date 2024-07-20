namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a morph target.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="MorphTarget"/> class.
    /// </remarks>
    /// <param name="name">The name of the morph target.</param>
    public class MorphTarget(string name)
    {
        /// <summary>
        /// Gets or Sets the name of the morph target.
        /// </summary>
        public string Name { get; set; } = name;
    }
}