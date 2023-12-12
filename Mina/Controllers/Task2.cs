using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mina.Contexts;
using Mina.Entities;
using NetTopologySuite.Features;
using NetTopologySuite.IO;

namespace Mina.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Task2 : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Task2(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("upload-buildings")]
        public async Task<IActionResult> UploadBuildings(IFormFile file1, IFormFile file2)
        {
            var processedGeometries = new HashSet<string>();

            await ProcessAndAddBuildings(file1, processedGeometries);
            await ProcessAndAddBuildings(file2, processedGeometries);

            await _context.SaveChangesAsync();
            return Ok("Buildings added successfully.");
        }

        private async Task ProcessAndAddBuildings(IFormFile file, HashSet<string> processedGeometries)
        {
            var buildings = await ProcessBuildingFile(file);

            foreach (var building in buildings)
            {
                var wkt = building.Geometry.ToText();

                if (!processedGeometries.Contains(wkt))
                {
                    var existingBuilding = await _context.Buildings
                        .FromSqlRaw(@"SELECT * FROM ""Buildings"" WHERE ST_AsText(""Geometry"") = {0}", wkt)
                        .FirstOrDefaultAsync();

                    if (existingBuilding == null)
                    {
                        _context.Buildings.Add(building);
                        processedGeometries.Add(wkt);
                    }
                }
            }
        }

        private async Task<List<Building>> ProcessBuildingFile(IFormFile file)
        {
            var buildings = new List<Building>();
            var reader = new GeoJsonReader();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                using (var sr = new StreamReader(stream))
                {
                    var content = sr.ReadToEnd();
                    var featureCollection = reader.Read<FeatureCollection>(content);

                    foreach (var feature in featureCollection)
                    {
                        buildings.Add(new Building { Geometry = feature.Geometry });
                    }
                }
            }

            return buildings;
        }


    }
}
