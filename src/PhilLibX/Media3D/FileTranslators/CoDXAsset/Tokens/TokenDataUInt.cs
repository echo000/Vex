namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataUInt"/> class
    /// </remarks>
    /// <param name="data">Data</param>
    /// <param name="token">Token</param>
    public class TokenDataUInt(uint data, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public uint Data { get; set; } = data;
    }
}
