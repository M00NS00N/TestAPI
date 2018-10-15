using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using log4net.Core;
using TestAPI.DAL;
using TestAPI.DAL.Models;

namespace TestAPI.Controllers
{
    public class ProductsController : ApiController
    {
        private AdventureWorksContext _dbContext =
            new AdventureWorksContext(
                System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);

        private ILog _log = LogManager.GetLogger(typeof(ProductsController));

        // GET api/products
        public string Get()
        {
            var serializedObject = JsonConvert.SerializeObject(_dbContext.Products.ToList());
            _log.Debug("Get products");
            return serializedObject;
        }

        // GET api/products/5
        public string Get(int id)
        {
            var product = _dbContext.Products.Find(id);
            var serializedObject = "null";
            _log.Debug("Get product");
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

            _log.Debug("Post product");
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
            _log.Debug("Put product");

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
            _log.Debug("Delete product");

            HttpResponseMessage httpResponseMessage = Request.CreateResponse(HttpStatusCode.OK);
            return httpResponseMessage;
        }
    }
}
