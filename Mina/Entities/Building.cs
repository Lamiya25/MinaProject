using NetTopologySuite.Geometries;

namespace Mina.Entities
{
    public class Building
    {
        public int Id { get; set; }
        public Geometry Geometry { get; set; }
    }
}
