namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataBoneWeight"/> class
    /// </remarks>
    /// <param name="index">Index</param>
    /// <param name="weight">Weight</param>
    public class TokenDataBoneWeight(int index, float weight, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the index
        /// </summary>
        public int BoneIndex { get; set; } = index;

        /// <summary>
        /// Gets or Sets the weight
        /// </summary>
        public float BoneWeight { get; set; } = weight;
    }
}
