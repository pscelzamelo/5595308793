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

namespace ZxBackend.Controllers
{
    [Route("api/[controller]")]
    public class PdvController : Controller
    {
        const string _pdvsCacheKey = "pdvs";
        private IMemoryCache _cache;

        public PdvController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        
        // GET api/pdv
        [HttpGet]
        public JArray Get()
        {
            return GetPdvs();
        }

        // GET api/pdv/5
        [HttpGet("{id}")]
        public JObject Get(int id)
        {
            var pdvs = GetPdvs();
            return (JObject)pdvs.FirstOrDefault(x => (int)x["id"] == id);
        }

        // POST api/pdv
        [HttpPost]
        public CommandResponse Post(JObject pdv)
        {
            var errors = new List<string>();

            //Validate mandatory fields
            if ((int?)pdv["id"] == null || (int?)pdv["id"] < 1) errors.Add("Invalid Id");
            if (string.IsNullOrEmpty((string)pdv["tradingName"])) errors.Add("Invalid Trading Name");
            if (string.IsNullOrEmpty((string)pdv["ownerName"])) errors.Add("Invalid Owner Name");
            if (string.IsNullOrEmpty((string)pdv["document"])) errors.Add("Invalid Document");
            if ((int?)pdv["deliveryCapacity"] == null) errors.Add("Invalid Capacity");
            var coverageArea = JsonConvert.DeserializeObject<MultiPolygon>(pdv["coverageArea"]?.ToString());
            if (coverageArea == null) errors.Add("Invalid Coverage Area");
            var address = JsonConvert.DeserializeObject<Point>(pdv["address"]?.ToString());
            if (address == null) errors.Add("Invalid Address");

            //Validate existing CNPJ
            var pdvs = GetPdvs();
            if (pdvs.Any(x => (string)x["document"] == (string)pdv["document"])) errors.Add("Document must be unique within database");

            //Return errors or persist
            if (errors.Count > 0)
            {
                return new CommandResponse(false, errors, pdv);
            }
            else
            {
                pdvs.Add(pdv);
                _cache.Set(_pdvsCacheKey,pdvs);
                return new CommandResponse(true, pdv);
            }
        }
        
        [HttpGet("closest")]
        public JObject Closest()
        {
            JObject result = null;

            //Read data from querystring
            string latitude = Request.Query["lat"];
            string longitude = Request.Query["lon"];
            if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude)) return result; //TODO: better error handling
            double lat, lon;
            lat = double.Parse(latitude);
            lon = double.Parse(longitude);
            var testPoint = new Point(new Position(lat, lon));

            //Query PDV's to find the closest
            var pdvs = GetPdvs();
            foreach (JObject pdv in pdvs)
            {
                var address = JsonConvert.DeserializeObject<Point>(pdv["address"]?.ToString());
                var distance = GeoUtils.GetDistance(address, testPoint);
                if (result == null || (double)result["distance"] > distance)
                {
                    var coverageArea = JsonConvert.DeserializeObject<MultiPolygon>(pdv["coverageArea"]?.ToString());
                    if (GeoUtils.IsPointInMultiPolygon(testPoint, coverageArea))
                    {
                        pdv["distance"] = distance;
                        result = pdv;
                    }
                }
            }

            return result;
        }

        private JArray GetPdvs()
        {
            var cached = _cache.GetOrCreate(_pdvsCacheKey, entry =>
            {
                var rawObj = JObject.Parse(System.IO.File.ReadAllText(@"pdvs.json"));
                return (JArray)rawObj["pdvs"];
            });
            return cached;
        }

    }
}
