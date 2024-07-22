using System.IO;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a pointer to a texture file.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Texture"/> class with the provided file.
    /// </remarks>
    /// <param name="name">Texture name/file path.</param>
    /// <param name="type">Texture type. For example diffuseMap, specularMap, etc.</param>
    public class Texture(string path, string type) : Graphics3DObject
    {
        /// <summary>
        /// Gets or Sets the name of the texture.
        /// </summary>
        public string Name { get; set; } = Path.GetFileNameWithoutExtension(path);

        /// <summary>
        /// Gets or Sets the file path of the texture.
        /// </summary>
        public string FilePath { get; set; } = path;

        /// <summary>
        /// Gets or Sets the texture type. For example diffuseMap, specularMap, etc.
        /// </summary>
        public string Type { get; set; } = type;
    }
}