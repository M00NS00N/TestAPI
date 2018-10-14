using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TestAPI.DAL;
using TestAPI.DAL.Models;
using Serilog;

namespace TestAPI.Controllers
{
    public class ProductsController : ApiController
    {
        private AdventureWorksContext _dbContext =
            new AdventureWorksContext(
                System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);

        private ILogger _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Minute)
            .CreateLogger();

        // GET api/products
        public string Get()
        {
            var serializedObject = JsonConvert.SerializeObject(_dbContext.Products.ToList());
            _logger.Information("Get products");
            return serializedObject;
        }

        // GET api/products/5
        public string Get(int id)
        {
            var product = _dbContext.Products.Find(id);
            var serializedObject = "null";
            _logger.Information("Get product");
            if (product != null)
            {
                serializedObject = JsonConvert.SerializeObject(product);
            }

            return serializedObject;
        }

        // POST api/values
        public int Post([FromBody]string productName)
        {
            var product = new Product
            {
                Name = productName ?? "test"
            };

            _logger.Information("Post product");
            product = _dbContext.Products.Add(product);
            _dbContext.SaveChanges();

            return product.ProductId;
        }

        // PUT api/products/5
        public string Put(int id, [FromBody]string name)
        {
            var product = _dbContext.Products.Find(id);
            if (product != null)
            {
                product.Name = name ?? "test2";
                _dbContext.SaveChanges();
            }
            _logger.Information("Put product");

            return JsonConvert.SerializeObject(product);
        }

        // DELETE api/products/5
        public HttpResponseMessage Delete(int id)
        {
            var product = _dbContext.Products.Find(id);
            if (product != null)
            {
                _dbContext.Products.Remove(product);
                _dbContext.SaveChanges();
            }
            _logger.Information("Delete product");

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            return httpResponseMessage;
        }
    }
}
