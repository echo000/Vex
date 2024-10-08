﻿using System.Numerics;

namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    public class TokenDataVector3 : TokenData
    {
        /// <summary>
        /// Gets or Sets the data
        /// </summary>
        public Vector3 Data { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenDataVector3"/> class
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="token">Token</param>
        public TokenDataVector3(Vector3 data, Token token) : base(token)
        {
            Data = data;
        }
    }
}
