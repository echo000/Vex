namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenData"/> class
    /// </remarks>
    /// <param name="token">Token</param>
    public class TokenData(Token token)
    {
        /// <summary>
        /// Gets the token
        /// </summary>
        public Token Token { get; private set; } = token;

        public override string ToString()
        {
            return Token.Name;
        }
    }
}
