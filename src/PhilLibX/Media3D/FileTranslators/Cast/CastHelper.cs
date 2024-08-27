using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PhilLibX.Media3D.FileTranslators.Cast
{
    public enum CastNodeID : uint
    {
        Root = 0x746F6F72,
        Model = 0x6C646F6D,
        Mesh = 0x6873656D,
        BlendShape = 0x68736C62,
        Skeleton = 0x6C656B73,
        Bone = 0x656E6F62,
        IKHandle = 0x64686B69,
        Constraint = 0x74736E63,
        Animation = 0x6D696E61,
        Curve = 0x76727563,
        CurveModeOverride = 0x564F4D43,
        NotificationTrack = 0x6669746E,
        Material = 0x6C74616D,
        File = 0x656C6966,
        Instance = 0x74736E69,
        Metadata = 0x6174656D,
    }

    public enum CastPropertyId : ushort
    {
        Byte = 'b',
        Short = 'h',
        Integer32 = 'i',
        Integer64 = 'l',
        Float = 'f',
        Double = 'd',
        String = 's',
        Vector2 = 'v' << 8 | '2',
        Vector3 = 'v' << 8 | '3',
        Vector4 = 'v' << 8 | '4'
    };

    public class CastProperty
    {
        public CastPropertyId Identifier;
        public ulong Elements;
        public List<byte> Buffer;

        public CastProperty()
        {
            Identifier = CastPropertyId.Byte;
            Elements = 0;
            Buffer = [];
        }

        public void Write(string data)
        {
            Elements++;
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            Buffer.AddRange(bytes);
            Buffer.Add(0);
        }

        public void Write<T>(T data)
        {
            if (data != null)
            {
                Elements++;

                // Convert the generic data to bytes
                int size = Marshal.SizeOf(data);
                byte[] bytes = new byte[size];

                IntPtr ptr = Marshal.AllocHGlobal(size);
                try
                {
                    Marshal.StructureToPtr(data, ptr, false);
                    Marshal.Copy(ptr, bytes, 0, size);
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }

                Buffer.AddRange(bytes);
            }
        }

        public void Write(byte[] data)
        {
            Elements++;
            Buffer.AddRange(data);
        }
    }

    public class CastNode
    {
        public CastNodeID Identifier;
        public ulong Hash;
        public SortedDictionary<string, CastProperty> Properties;
        public List<CastNode> Children;

        public CastNode()
        {
            Identifier = CastNodeID.Root;
            Hash = 0;
            Children = [];
            Properties = [];
        }

        public CastNode(CastNodeID id)
        {
            Identifier = id;
            Hash = 0;
            Children = [];
            Properties = [];
        }

        public CastNode(CastNodeID id, ulong hash)
        {
            Identifier = id;
            Hash = hash;
            Children = [];
            Properties = [];
        }

        public CastProperty AddProperty(string propName, CastPropertyId id)
        {
            var prop = new CastProperty
            {
                Identifier = id
            };
            Properties.Add(propName, prop);
            return prop;
        }

        public CastProperty AddProperty(string propName, CastPropertyId id, int capacity)
        {
            var prop = new CastProperty
            {
                Identifier = id,
                Buffer = new List<byte>(capacity)
            };
            Properties.Add(propName, prop);
            return prop;
        }

        public void SetProperty(string propName, string value)
        {
            var prop = new CastProperty
            {
                Identifier = CastPropertyId.String
            };
            prop.Write(value);
            Properties[propName] = prop;
        }

        public void SetProperty<T>(string propName, CastPropertyId id, T value)
        {
            var prop = new CastProperty
            {
                Identifier = id
            };
            prop.Write(value);
            Properties[propName] = prop;
        }

        public int Size()
        {
            var result = 24;

            foreach (var prop in Properties)
            {
                result += 8;
                result += prop.Key.Length;
                result += prop.Value.Buffer.Count;
            }

            foreach (var child in Children)
            {
                result += child.Size();
            }

            return result;
        }

        public CastNode AddNode(CastNodeID id)
        {
            Children.Add(new CastNode(id));
            return Children.Last();
        }

        public CastNode AddNode(CastNodeID id, ulong hash)
        {
            Children.Add(new CastNode(id, hash));
            return Children.Last();
        }
    }
}
