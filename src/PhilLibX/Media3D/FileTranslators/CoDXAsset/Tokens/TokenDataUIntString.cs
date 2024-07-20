namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataUIntString"/> class
    /// </remarks>
    /// <param name="intVal">Integer Value</param>
    /// <param name="strVal">String value</param>
    /// <param name="token">Token</param>
    public class TokenDataUIntString(uint intVal, string strVal, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public uint IntegerValue { get; set; } = intVal;

        /// <summary>
        /// Gets or Sets the value
        /// </summary>
        public string StringValue { get; set; } = strVal;
    }
}
