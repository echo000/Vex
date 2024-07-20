namespace PhilLibX.Media3D.CoDXAsset.Tokens
{
    /// <summary>
    /// A class to hold a token of the specified type
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="TokenDataBoneInfo"/> class
    /// </remarks>
    /// <param name="index">Index</param>
    /// <param name="parentIndex">Parent index</param>
    /// <param name="name">Bone name</param>
    /// <param name="token">Token</param>
    public class TokenDataBoneInfo(int index, int parentIndex, string name, Token token) : TokenData(token)
    {
        /// <summary>
        /// Gets or Sets the name of the bone
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// Gets or Sets the index
        /// </summary>
        public int BoneIndex { get; set; } = index;

        /// <summary>
        /// Gets or Sets the parent index
        /// </summary>
        public int BoneParentIndex { get; set; } = parentIndex;
    }
}
