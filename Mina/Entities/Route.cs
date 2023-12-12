using NetTopologySuite.Geometries;

namespace Mina.Entities
{
    public class Route
    {
        public int Id { get; set; }
        public Geometry Geometry { get; set; }
        public string? Highway { get; set; }
        public string? Name { get; set; }
        public string? NameAz { get; set; }
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public bool? Oneway { get; set; }
        public string? GeoType { get; set; }
        public int? Index { get; set; }
    }
}
