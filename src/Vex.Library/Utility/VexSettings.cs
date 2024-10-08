﻿using PhilLibX.IO;
using System;
using System.IO;

namespace Vex.Library
{
    [Serializable]
    public class VexSettings : Notifiable
    {
        /// <summary>
        /// Setting Values
        /// </summary>
        public string ExportDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "exported_files");
        public bool AutoUpdates { get; set; } = true;

        public byte ModelExportFormat { get; set; } = (byte)MdlExportFormat.CAST;
        public byte AnimationExportFormat { get; set; } = (byte)AnimExportFormat.CAST;
        public byte ImageExportFormat { get; set; } = (byte)ImgExportFormat.DDS;

        public bool LoadModels { get; set; } = true;
        public bool LoadAnimations { get; set; } = true;
        public bool LoadImages { get; set; } = false;
        public bool LoadRawFiles { get; set; } = false;
        public bool LoadSounds { get; set; } = false;
        public bool LoadMaterials { get; set; } = false;

        public bool LoadImagesModel { get; set; } = true;
        public bool ExportLods { get; set; } = false;
        public bool ExportVertexColor { get; set; } = false;
        public bool ExportModelImages { get; set; } = true;
        public bool ExportHitbox { get; set; } = false;
        public bool GlobalImages { get; set; } = false;
        public bool MaterialModelFolders { get; set; } = false;

        public bool PatchNormals { get; set; } = true;
        public bool OverwriteExistingFiles { get; set; } = false;
        public bool KeepSoundPath { get; set; } = false;
        public bool AudioLanguageFolders { get; set; } = false;

        public static VexSettings Load(string fileName)
        {
            try
            {
                var settings = new VexSettings();
                if (!File.Exists(fileName))
                {
                    settings.Save(fileName);
                }
                else
                {
                    using var reader = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
                    if (reader.ReadUInt32() == 0x47464356)
                    {
                        foreach (var prop in settings.GetType().GetProperties())
                        {
                            switch (Type.GetTypeCode(prop.PropertyType))
                            {
                                case TypeCode.String:
                                    prop.SetValue(settings, reader.ReadNullTerminatedString());
                                    break;
                                case TypeCode.Boolean:
                                    prop.SetValue(settings, reader.ReadBoolean());
                                    break;
                                case TypeCode.Int32:
                                    prop.SetValue(settings, reader.ReadInt32());
                                    break;
                                case TypeCode.Byte:
                                    prop.SetValue(settings, reader.ReadByte());
                                    break;
                                default:
                                    throw new InvalidOperationException($"Unsupported property type: {prop.PropertyType}");
                            }
                        }
                    }
                }
                return settings;
            }
            catch
            {
                var settings = new VexSettings();
                settings.Save(fileName);
                return settings;
            }
        }

        /// <summary>
        /// Saves all settings to a file
        /// </summary>
        /// <param name="fileName">File Name</param>
        public void Save(string fileName)
        {
            try
            {
                using var writer = new BinaryWriter(new FileStream(fileName, FileMode.Create));
                writer.Write(0x47464356);
                foreach (var prop in GetType().GetProperties())
                {
                    var value = prop.GetValue(this);
                    switch (value)
                    {
                        case string strValue:
                            writer.WriteNullTerminatedString(strValue ?? string.Empty);
                            break;
                        case bool boolValue:
                            writer.Write(boolValue);
                            break;
                        case int intValue:
                            writer.Write(intValue);
                            break;
                        case byte byteValue:
                            writer.Write(byteValue);
                            break;
                        default:
                            throw new InvalidOperationException($"Unsupported property type: {prop.PropertyType}");
                    }
                }
            }
            catch
            {
                return;
            }
        }
    }
}
