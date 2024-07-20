namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataInt"/> class
    /// </remarks>
    /// <param name="data">Data</param>
    /// <param name="token">Token</param>
    public class TokenDataInt(int data, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public int Data { get; set; } = data;
    }
}
