namespace MAD2.Project
{
    public class Edge
    {
        public int NodeFrom { get; }
        public int NodeTo { get; }
        public int Weight { get; }

        public Edge(int nodeFrom, int nodeTo, int weight)
        {
            NodeFrom = nodeFrom;
            NodeTo = nodeTo;
            Weight = weight;
        }
    }
}
