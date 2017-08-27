using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ZxBackend.Models;
using Newtonsoft.Json;
using GeoJSON.Net.Geometry;
using ZxBackend.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using ZxBackend.Data;

namespace ZxBackend.Controllers
{
    [Route("api/[controller]")]
    public class PdvController : Controller
    {
        const string _pdvsCacheKey = "pdvs";
        private IMemoryCache _cache;
        private AppDbContext _db;

        public PdvController(IMemoryCache memoryCache,AppDbContext db)
        {
            _cache = memoryCache;
            _db = db;
        }
        
        // GET api/pdv
        [HttpGet]
        public IEnumerable<Pdv> Get()
        {
            return CachedGetPdvs();
        }

        // GET api/pdv/5
        [HttpGet("{id}")]
        public Pdv Get(int id)
        {
            var pdvs = CachedGetPdvs();
            return pdvs.FirstOrDefault(x => x.Id == id);
        }

        // POST api/pdv
        [HttpPost]
        public CommandResponse Post(JObject item)
        {
            var errors = new List<string>();

            //Validate mandatory fields
            if ((int?)item["id"] == null || (int?)item["id"] < 1) errors.Add("Invalid Id");
            if (string.IsNullOrEmpty((string)item["tradingName"])) errors.Add("Invalid Trading Name");
            if (string.IsNullOrEmpty((string)item["ownerName"])) errors.Add("Invalid Owner Name");
            if (string.IsNullOrEmpty((string)item["document"])) errors.Add("Invalid Document");
            if ((int?)item["deliveryCapacity"] == null) errors.Add("Invalid Capacity");
            var coverageArea = JsonConvert.DeserializeObject<MultiPolygon>(item["coverageArea"]?.ToString());
            if (coverageArea == null) errors.Add("Invalid Coverage Area");
            var address = JsonConvert.DeserializeObject<Point>(item["address"]?.ToString());
            if (address == null) errors.Add("Invalid Address");

            //Validate existing CNPJ
            var pdvs = CachedGetPdvs();
            if (pdvs.Any(x => x.Document == (string)item["document"])) errors.Add("Document must be unique within database");

            //Return errors or persist
            if (errors.Count > 0)
            {
                return new CommandResponse(false, errors, item);
            }
            else
            {
                var pdv = new Pdv()
                {
                    Id = 0, // Avoid explict id inserting in identity column
                    TradingName = (string)item["tradingName"],
                    OwnerName = (string)item["ownerName"],
                    Document = (string)item["document"],
                    CoverageArea = (string)item["coverageArea"],
                    Address = (string)item["address"],
                };
                pdvs.ToList().Add(pdv);
                _cache.Set(_pdvsCacheKey,pdvs);
                _db.Pdvs.Add(pdv);
                _db.SaveChanges();
                return new CommandResponse(true, item);
            }
        }
        
        [HttpGet("closest")]
        public Pdv Closest()
        {
            Pdv result = null;

            //Read data from querystring
            string latitude = Request.Query["lat"];
            string longitude = Request.Query["lon"];
            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude)) return result; //TODO: better error handling
            double lat, lon;
            lat = double.Parse(latitude);
            lon = double.Parse(longitude);
            var testPoint = new Point(new Position(lat, lon));

            //Query PDV's to find the closest
            var pdvs = CachedGetPdvs();
            foreach (var pdv in pdvs)
            {
                var address = JsonConvert.DeserializeObject<Point>(pdv.Address);
                var distance = GeoUtils.GetDistance(address, testPoint);
                if (result == null || result.Distance > distance)
                {
                    var coverageArea = JsonConvert.DeserializeObject<MultiPolygon>(pdv.CoverageArea);
                    if (GeoUtils.IsPointInMultiPolygon(testPoint, coverageArea))
                    {
                        pdv.Distance = distance;
                        result = pdv;
                    }
                }
            }

            return result;
        }

        private IEnumerable<Pdv> CachedGetPdvs()
        {
            var cached = _cache.GetOrCreate(_pdvsCacheKey, entry =>
            {
                return _db.Set<Pdv>().ToList();
            });
            return cached;
        }

    }
}
