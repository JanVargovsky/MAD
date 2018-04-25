using System.Diagnostics;

namespace MAD2.Lesson8
{
    [DebuggerDisplay("{SepalLength}, {SepalWidth}, {PetalLength}, {PetalWidth}, {Class}")]
    public class IrisData
    {
        public float SepalLength { get; set; }
        public float SepalWidth { get; set; }
        public float PetalLength { get; set; }
        public float PetalWidth { get; set; }
        public string Class { get; set; }
    }
}
