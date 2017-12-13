using System;

namespace MAD.Project
{
    public class UFORecord
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string City { get; set; }
        public string StateOrProvince { get; set; }
        public string Country { get; set; }
        public string Shape { get; set; }
        public Shape? ShapeEnum { get; set; }
        public float Length { get; set; } // in sec
        public string DescribedLength { get; set; }
        public string Description { get; set; }
        public DateTime DocumentedAt { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
