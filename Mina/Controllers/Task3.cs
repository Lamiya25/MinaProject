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
    public class Task3 : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Task3(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }


        [HttpPost("upload-routes")]
        public async Task<IActionResult> UploadRoutes(IFormFile file)
        {
            var routes = await ProcessRouteFile(file);

            foreach (var route in routes)
            {
                var intersects = await _context.Routes
      .FromSqlRaw(@"SELECT * FROM ""Routes"" WHERE ST_Intersects(""Geometry"", {0}) = true", route.Geometry)
      .AnyAsync();

                if (!intersects)
                {
                    _context.Routes.Add(route);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Routes uploaded successfully.");
        }


        private async Task<List<Entities.Route>> ProcessRouteFile(IFormFile file)
        {
            var routes = new List<Entities.Route>();
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
                        var route = new Entities.Route
                        {
                            Geometry = feature.Geometry,
                            Highway = GetAttributeAsString(feature, "highway"),
                            Name = GetAttributeAsString(feature, "name"),
                            NameAz = GetAttributeAsString(feature, "name:az"),
                            NameEn = GetAttributeAsString(feature, "name:en"),
                            NameRu = GetAttributeAsString(feature, "name:ru"),
                            Oneway = GetAttributeAsBoolean(feature, "oneway"),
                            GeoType = GetAttributeAsString(feature, "geotype"),
                            Index = GetAttributeAsInt(feature, "index")
                        };

                        routes.Add(route);
                    }
                }
            }

            return routes;
        }
        private string GetAttributeAsString(IFeature feature, string attributeName)
        {
            return feature.Attributes.Exists(attributeName) ? feature.Attributes[attributeName]?.ToString() : null;
        }

        private bool GetAttributeAsBoolean(IFeature feature, string attributeName)
        {
            if (feature.Attributes.Exists(attributeName) && bool.TryParse(feature.Attributes[attributeName]?.ToString(), out var result))
            {
                return result;
            }
            return false;
        }

        private int GetAttributeAsInt(IFeature feature, string attributeName)
        {
            if (feature.Attributes.Exists(attributeName) && int.TryParse(feature.Attributes[attributeName]?.ToString(), out var result))
            {
                return result;
            }
            return 0;
        }
    }
}