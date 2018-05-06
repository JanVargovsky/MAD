using System.Collections.Generic;

namespace MAD2.Project
{
    public class Node
    {
        public int Id { get; }
        public Dictionary<string, string> Attributes { get; }

        public Node(int id, Dictionary<string, string> attributes)
        {
            Id = id;
            Attributes = attributes;
        }
    }
}
