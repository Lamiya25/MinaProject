using Microsoft.AspNetCore.Mvc;
using Mina.Contexts;
using Mina.Entities;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Task1 : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Task1(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> PostPOI(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Upload a valid GeoJSON file.");
            }

            FeatureCollection featureCollection;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(stream))
                {
                    string content = streamReader.ReadToEnd();
                    var geoJsonReader = new GeoJsonReader();
                    featureCollection = geoJsonReader.Read<FeatureCollection>(content);
                }
            }

            foreach (var feature in featureCollection)
            {
                if (!feature.Attributes.Exists("id") ||
             !long.TryParse(feature.Attributes["id"]?.ToString() ?? string.Empty, out long id))
                {
                    continue;
                }
                var poi = new Poi
                {
                    Id = id,
                    Properties = JsonDocument.Parse(JsonConvert.SerializeObject(feature.Attributes)),
                    Location = (Point)feature.Geometry
                };


                _context.POIs.Add(poi);
            }

            await _context.SaveChangesAsync();
            return Ok("Data imported successfully.");
        }
    }
} 
 