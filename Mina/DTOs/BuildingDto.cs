using NetTopologySuite.Geometries;

namespace Mina.DTOs
{
    public class BuildingDto
    {
        public int Id { get; set; }
        public Geometry Geometry { get; set; }
    }
}
