namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataTri"/> class
    /// </remarks>
    /// <param name="index">Index</param>
    /// <param name="weight">Weight</param>
    public class TokenDataTri(int a, int b, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the index
        /// </summary>
        public int A { get; set; } = a;

        /// <summary>
        /// Gets or Sets the weight
        /// </summary>
        public int B { get; set; } = b;
    }
}
