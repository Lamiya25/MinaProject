using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mina.Contexts;
using Mina.DTOs;
using Mina.Entities;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;


namespace Mina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Task4 : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Task4(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("buildings")]
        public async Task<IActionResult> GetAllBuildings()
        {
            var buildings = await _context.Buildings.ToListAsync();

            var featureCollection = new FeatureCollection();
            foreach (var building in buildings)
            {
                var feature = new Feature
                {
                    Geometry = building.Geometry,
                    Attributes = new AttributesTable
                    {
                        { "Id", building.Id }
                    }
                };
                featureCollection.Add(feature);
            }

            var serializer = GeoJsonSerializer.Create();
            var stringWriter = new StringWriter();
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonWriter, featureCollection);
            }

            return Content(stringWriter.ToString(), "application/json");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBuildingById(int id)
        {
            var building = await _context.Buildings
                 .Where(b => b.Id == id)
                 .SingleOrDefaultAsync();
            if (building == null)
            {
                return NotFound();
            }
            var feature = new Feature
            {
                Geometry = building.Geometry,
                Attributes = new AttributesTable
                {
                    { "Id", building.Id }
                }
            };
            var serializer = GeoJsonSerializer.Create(new JsonSerializerSettings(), new GeometryFactory(new PrecisionModel(), 4326));
            var stringWriter = new StringWriter();
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonWriter, feature);
            }
            string geoJson = stringWriter.ToString();
            return Content(geoJson, "application/json");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("A file is required.");
            }
            Feature feature;
            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                var geoJsonReader = new GeoJsonReader();
                feature = geoJsonReader.Read<Feature>(streamReader.ReadToEnd());
            }

            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }

            building.Geometry = feature.Geometry;

            _context.Buildings.Update(building);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]

        public async Task<IActionResult> DeleteBuilding(int id)
        {
            var building = await _context.Buildings.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            _context.Buildings.Remove(building);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> AddBuilding(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("A file is required");
            }
            Feature feature;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var geoJsonReader = new GeoJsonReader();
                feature = geoJsonReader.Read<Feature>(reader.ReadToEnd());
            }

            var newBuilding = new Building
            {
                Geometry = feature.Geometry
            };
            _context.Buildings.Add(newBuilding);
            await _context.SaveChangesAsync();
            var newBuildingDto = new BuildingDto
            {
                Id = newBuilding.Id
            };

            return CreatedAtAction(nameof(GetBuildingById), new { id = newBuilding.Id }, newBuildingDto);
        }

        [HttpGet("getPoi/{id}")]
        public async Task<IActionResult> GetPoiForBuilding(int id)
        {
            var building = await _context.Buildings
          .Where(b => b.Id == id)
          .Select(b => b.Geometry)
          .SingleOrDefaultAsync();

            if (building == null)
            {
                return NotFound();
            }

            var points = new List<Point>();
            if (building is Polygon polygon)
            {
                points.AddRange(polygon.Coordinates.Select(c => new Point(c)));
            }
            var featureCollection = new FeatureCollection();
            foreach (var point in points)
            {
                var feature = new Feature(point, new AttributesTable());
                featureCollection.Add(feature);
            }

            var serializer = GeoJsonSerializer.Create();
            var stringWriter = new StringWriter();
            using (var jsonWriter = new JsonTextWriter(stringWriter))
            {
                serializer.Serialize(jsonWriter, featureCollection);
            }

            return Content(stringWriter.ToString(), "application/json");
        }
    }
 }