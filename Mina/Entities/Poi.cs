using NetTopologySuite.Geometries;
using System.Text.Json;

namespace Mina.Entities
{
    public class Poi
    {
        public long Id { get; set; }
        public JsonDocument Properties { get; set; }
        public Point Location { get; set; }
    }
}