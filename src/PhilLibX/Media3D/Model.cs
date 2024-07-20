using System.Collections.Generic;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold a 3D Model.
    /// </summary>
    public class Model : Graphics3DObject
    {
        /// <summary>
        /// Gets or Sets the name of the model.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Get or Sets the skeleton this model uses.
        /// </summary>
        public Skeleton? Skeleton { get; set; }

        /// <summary>
        /// Get or Sets the morph this model uses.
        /// </summary>
        public Morph? Morph { get; set; }

        /// <summary>
        /// Gets or Sets the meshes stored within this model.
        /// </summary>
        public List<Mesh> Meshes { get; set; }

        /// <summary>
        /// Gets or Sets the materials stored within this model.
        /// </summary>
        public List<Material> Materials { get; set; }


        public Model()
        {
            Meshes = [];
            Materials = [];
        }

        public Model(Skeleton? skeleton)
        {
            Skeleton = skeleton;
            Meshes = [];
            Materials = [];
        }

        public Model(Skeleton? skeleton, Morph? morph)
        {
            Skeleton = skeleton;
            Morph = morph;
            Meshes = [];
            Materials = [];
        }

        /// <summary>
        /// Assigns the bone indices based off their index within the table. 
        /// </summary>
        public void AssignSkeletonBoneIndices()
        {
            Skeleton?.AssignBoneIndices();
        }

        public int GetVertexCount()
        {
            var result = 0;

            foreach (var mesh in Meshes)
            {
                result += mesh.Positions.Count;
            }

            return result;
        }

        public int GetFaceCount()
        {
            var result = 0;

            foreach (var mesh in Meshes)
            {
                result += mesh.Faces.Count;
            }

            return result;
        }

        public void Scale(float scale)
        {
            if (scale != 1 && scale != 0)
            {
                if (Skeleton != null)
                {
                    foreach (var bone in Skeleton.Bones)
                    {
                        bone.BaseLocalTranslation *= scale;
                        bone.BaseWorldTranslation *= scale;
                    }
                }

                foreach (var mesh in Meshes)
                {
                    for (int i = 0; i < mesh.Positions.Count; i++)
                    {
                        mesh.Positions[i] *= scale;
                    }
                }
            }
        }
    }
}
