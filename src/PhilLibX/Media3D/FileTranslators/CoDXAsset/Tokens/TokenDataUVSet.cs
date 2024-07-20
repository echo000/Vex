using System.Collections.Generic;
using System.Numerics;

namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    public class TokenDataUVSet : TokenData
    {
        /// <summary>
        /// Gets or Sets the uvs
        /// </summary>
        public List<Vector2> UVs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataUVSet"/> class
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="token">Token</param>
        public TokenDataUVSet(Token token) : base(token)
        {
            UVs = new();
        }
    }
}
