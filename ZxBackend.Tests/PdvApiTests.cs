using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using ZxBackend.Controllers;

namespace ZxBackend.Tests
{
    [TestClass]
    public class PdvApiTests
    {
        [TestMethod]
        public void TestCreatePdvInvalidId()
        {
            //Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            var controller = new PdvController(cache);
            var obj = MockValidPdv();
            obj["id"] = null;

            // Act
            var response = controller.Post(obj);

            // Assert
            Assert.IsFalse(response.Success);
            Assert.IsTrue(response.Errors.Count > 0);
        }

        private JObject MockValidPdv()
        {
            return JObject.Parse(@"{
	            ""id"": 51, 
	            ""tradingName"": ""Adega da Cerveja - Teste"",
	            ""ownerName"": ""Pedro Melo"",
	            ""document"": ""1234567891011/0001"",
	            ""coverageArea"": { 
	              ""type"": ""MultiPolygon"", 
	              ""coordinates"": [
		            [[[30, 20], [45, 40], [10, 40], [30, 20]]], 
		            [[[15, 5], [40, 10], [10, 20], [5, 10], [15, 5]]]
	              ]
	            },
	            ""address"": { 
	              ""type"": ""Point"",
	              ""coordinates"": [-46.57421, -21.785741]
	            },
	            ""deliveryCapacity"": 5
            }");
        }
    }
}
