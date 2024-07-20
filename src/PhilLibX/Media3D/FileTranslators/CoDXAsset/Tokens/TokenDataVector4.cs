using System.Numerics;

namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    public class TokenDataVector4 : TokenData
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public Vector4 Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataVector4"/> class
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="token">Token</param>
        public TokenDataVector4(Vector4 data, Token token) : base(token)
        {
            Data = data;
        }
    }
}
