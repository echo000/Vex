namespace Vex.Library
{
    public class WeightsData
    {
        // The weight value for each bone
        public float[] WeightValues;
        // The bone ids for each value
        public uint[] BoneValues;
        // The count of weights this set contains
        public byte WeightCount;

        public WeightsData()
        {
            WeightValues = new float[8];
            BoneValues = new uint[8];
            // Defaults
            WeightCount = 1;
            // Clear memory
            WeightValues[0] = 1.0f;
            WeightValues[1] = 1.0f;
            WeightValues[2] = 1.0f;
            WeightValues[3] = 1.0f;
            WeightValues[4] = 1.0f;
            WeightValues[5] = 1.0f;
            WeightValues[6] = 1.0f;
            WeightValues[7] = 1.0f;
            BoneValues[0] = 0;
            BoneValues[1] = 0;
            BoneValues[2] = 0;
            BoneValues[3] = 0;
            BoneValues[4] = 0;
            BoneValues[5] = 0;
            BoneValues[6] = 0;
            BoneValues[7] = 0;
        }
    };

}
