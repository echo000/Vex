using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.IO;

namespace Vex.Library.Utility
{
    internal class MaterialHelper
    {
        private static readonly HashSet<string> ValidTextureKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "diffusemap",
            "bumpmap",
            "occlusionmap",
            "emissivemap",
            "roughnessmap",
            "metallicmap",
            "packedmap",
            "glossmap"
        };

        //Honestly this is probably the most hacky thing in the world
        public static Dictionary<string, string> GetMaterialTextures(byte[] data)
        {
            var textures = new Dictionary<string, string>();

            using var memoryStream = new MemoryStream(data);
            using var reader = new StreamReader(memoryStream);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(); // Remove leading/trailing whitespace

                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                {
                    continue; // Skip empty lines and comments
                }

                var parts = line.Split([' ', '\t'], 2, StringSplitOptions.RemoveEmptyEntries);
                string key = parts[0].ToLower(); // Convert key to lowercase for case-insensitive matching

                if (parts.Length == 2)
                {
                    string value = parts[1];
                    if (ValidTextureKeys.Contains(key))
                    {
                        textures.Add(key, value.Trim());
                    }
                }
            }
            return textures;
        }

        public static Material GetMaterial(string MaterialName, VexInstance instance)
        {
            var asset = instance.VoidSupport.GetEntryFromName(MaterialName);
            if (asset != null)
            {
                return GetMaterialFromAsset(asset, instance);
            }
            return null;
        }

        public static Material GetMaterialFromAsset(Asset asset, VexInstance instance)
        {
            var Material = new Material(Path.GetFileNameWithoutExtension(asset.DisplayName));
            var bytes = instance.VoidSupport.ExtractEntry(asset, instance);
            var images = GetMaterialTextures(bytes);
            foreach (var image in images)
            {
                switch (image.Key)
                {
                    case "diffusemap":
                        Material.Textures["DiffuseMap"] = new(image.Value, image.Key);
                        break;
                    case "bumpmap":
                        Material.Textures["NormalMap"] = new(image.Value, image.Key);
                        break;
                    case "glossmap":
                        Material.Textures["GlossMap"] = new(image.Value, image.Key);
                        break;
                    case "specularmap":
                        Material.Textures["SpecularMap"] = new(image.Value, image.Key);
                        break;
                    case "occlusionmap":
                        Material.Textures["OcclusionMap"] = new(image.Value, image.Key);
                        break;
                    case "metallicmap":
                        Material.Textures["MetallicMap"] = new(image.Value, image.Key);
                        break;
                    case "roughnessmap":
                        Material.Textures["RoughnessMap"] = new(image.Value, image.Key);
                        break;
                    case "packedmap":
                        Material.Textures["PackedMap"] = new(image.Value, image.Key);
                        break;
                    case "emissivemap":
                        Material.Textures["EmissiveMap"] = new(image.Value, image.Key);
                        break;
                }
            }
            return Material;
        }
    }
}
