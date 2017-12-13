using System.Collections.Generic;
using System.Linq;

namespace MAD.Project
{
    public class DataMatrixRow
    {
        public int Id { get; set; }
        public List<string> Attributes { get; }

        public string this[int i]
        {
            get => Attributes[i];
            set => Attributes[i] = value;
        }

        public DataMatrixRow(int id, string[] attributes)
        {
            Id = id;
            Attributes = attributes.ToList();
        }
    }
}
