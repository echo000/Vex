using PhilLibX.Media3D;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Vex.Library.Utility
{
    internal class MaterialHelper
    {
        //Honestly this is probably the most hacky thing in the world
        public static Dictionary<string, string> GetMaterialTextures(byte[] data)
        {
            var textures = new Dictionary<string, string>();

            using var memoryStream = new MemoryStream(data);
            using var reader = new StreamReader(memoryStream);
            string line;
            bool isInStateSection = false; // Flag for state section

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(); // Remove leading/trailing whitespace

                if (string.IsNullOrEmpty(line) || line.StartsWith("//"))
                {
                    continue; // Skip empty lines and comments
                }

                var parts = line.Split([' ', '\t'], 2); // Split on whitespace, limit to 2 parts
                string key = parts[0].ToLower(); // Convert key to lowercase for case-insensitive matching

                if (parts.Length == 2)
                {
                    string value = parts[1];

                    // Look for specific texture maps
                    if (key.EndsWith("map") &&
                        (key == "diffusemap" || key == "bumpmap" || key == "occlusionmap" || key == "emissivemap" || 
                         key == "roughnessmap" || key == "metallicmap" || key == "packedmap" || key == "glossmap"))
                    {
                        value = Regex.Replace(value, @"\s", ""); // Remove whitespace from texture path
                        textures.Add(key, value);
                    }
                    // Handle state section (second format)
                    else if (key == "state")
                    {
                        isInStateSection = true;
                    }
                }
            }
            return textures;
        }

        public static Material GetMaterial(string MaterialName, VexInstance instance)
        {
            var asset = instance.VoidSupport.GetEntryFromName(MaterialName);
            if(asset != null)
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
