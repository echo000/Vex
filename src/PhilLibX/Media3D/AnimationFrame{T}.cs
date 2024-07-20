namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold an animation frame.
    /// </summary>
    /// <typeparam name="T">Data type stored within this frame.</typeparam>
    public struct AnimationFrame<T>(float time, T value)
    {
        /// <summary>
        /// Gets or Sets the time this frame occurs at.
        /// </summary>
        public float Time { get; set; } = time;

        /// <summary>
        /// Gets or Sets the 
        /// </summary>
        public T Value { get; set; } = value;
    }
}
