namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataFloat"/> class
    /// </remarks>
    /// <param name="data">Data</param>
    /// <param name="token">Token</param>
    public class TokenDataFloat(float data, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public float Data { get; set; } = data;
    }
}
