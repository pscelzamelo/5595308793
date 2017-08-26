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
        private IMemoryCache _cache;
        const string _pdvCacheKey = "pdvs";

        public PdvController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        const string _pdvsCacheKey = "pdvs";

        private JArray GetPdvs()
        {
            var cached = _cache.GetOrCreate(_pdvCacheKey, entry =>
            {
                var rawObj = JObject.Parse(System.IO.File.ReadAllText(@"pdvs.json"));
                return (JArray)rawObj["pdvs"];
            });
            return cached;
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
        
    }
}
