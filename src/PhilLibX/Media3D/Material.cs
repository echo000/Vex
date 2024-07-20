using System.Collections.Generic;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a basic <see cref="Material"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="Material"/> class.
    /// </remarks>
    /// <param name="name">Name of the material.</param>
    public class Material(string name) : Graphics3DObject
    {
        /// <summary>
        /// Gets or Sets the name of the material.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// Gets or Sets the textures assigned to this material.
        /// </summary>
        public Dictionary<string, Texture> Textures { get; set; } = [];
    }
}