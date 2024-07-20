namespace Vex.Library.Utility
{
    /// <summary>
    /// A container for a color (RGBA)
    /// </summary>
    public class Color
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public Color() { R = 255; G = 255; B = 255; A = 255; }
        public Color(byte Red, byte Green, byte Blue, byte Alpha) { R = Red; G = Green; B = Blue; A = Alpha; }

        public override bool Equals(object obj)
        {
            Color vec = (Color)obj;
            return (vec.R == R && vec.G == G && vec.B == B && vec.A == A);
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
        }

        public static readonly Color White = new(255, 255, 255, 255);
    }
}
