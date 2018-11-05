using Newtonsoft.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using log4net;
using log4net.Core;
using TestAPI.DAL;
using TestAPI.DAL.Models;
using log4net.Config;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using TestAPI.Common;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Reflection;
using System;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace TestAPI.Controllers
{
    public class ProductsController : ApiController
    {
        private AdventureWorksContext _dbContext =
            new AdventureWorksContext(
                System.Configuration.ConfigurationManager.ConnectionStrings["Default"].ConnectionString);

        private ILog _log = LogManager.GetLogger(typeof(ProductsController));

        private CloudStorageAccount _storageAccount = new CloudStorageAccount(
            new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(
                "khtestapistorage", "is6gRWqtwF7US3ZgzwA7T6ObWn8T7z8KqkaQteAtJMauCPQjFFvqlrBWJKGQ34HvKKIEigHu+OIIzydLaHHq5Q=="), true);

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
            foreach (IAppender appender in (_log.Logger as Logger).Appenders)
            {
                var buffered = appender as BufferingAppenderSkeleton;
                if (buffered != null)
                {
                    buffered.Flush();
                }
            }

            if (product != null)
            {
                serializedObject = JsonConvert.SerializeObject(product);
            }

            return serializedObject;
        }

        // POST api/values
        //public int Post([FromBody]string productName)
        //{
        //    var product = new Product
        //    {
        //        Name = productName ?? "test"
        //    };

        //    _log.Debug("Post product");
        //    product = _dbContext.Products.Add(product);
        //    _dbContext.SaveChanges();

        //    return product.ProductId;
        //}

        [System.Web.Http.HttpPost, System.Web.Http.Route("api/upload")]
        public ActionResult PostFile(HttpPostedFile upload)
        {
            if (upload == null)
            {
                upload = ConstructHttpPostedFile(new byte[] { }, "test.txt", "text");
            }

            if (upload != null)
            {
                string fileName = System.IO.Path.GetFileName(upload.FileName);

                byte[] buffer;
                using (BinaryReader br = new BinaryReader(upload.InputStream))
                {
                    buffer = br.ReadBytes((int)upload.InputStream.Length);
                }

                var filename = upload.FileName;
                var cloudBlobClient = _storageAccount.CreateCloudBlobClient();
                var cloudBlobContainer = cloudBlobClient.GetContainerReference("documents");
                var blobBlock = cloudBlobContainer.GetBlockBlobReference(filename);
                blobBlock.UploadFromByteArray(buffer, 0, buffer.Length);

                var cloudQueueClient = _storageAccount.CreateCloudQueueClient();
                var cloudQueue = cloudQueueClient.GetQueueReference("blobnotificationqueue");
                cloudQueue.AddMessage(new CloudQueueMessage($"{filename} {blobBlock.Name} {upload.ContentType}"));
            }

            // Return status code  
            return null;
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

        public HttpPostedFile ConstructHttpPostedFile(byte[] data, string filename, string contentType)
        {
            // Get the System.Web assembly reference
            Assembly systemWebAssembly = typeof(HttpPostedFileBase).Assembly;
            // Get the types of the two internal types we need
            Type typeHttpRawUploadedContent = systemWebAssembly.GetType("System.Web.HttpRawUploadedContent");
            Type typeHttpInputStream = systemWebAssembly.GetType("System.Web.HttpInputStream");

            // Prepare the signatures of the constructors we want.
            Type[] uploadedParams = { typeof(int), typeof(int) };
            Type[] streamParams = { typeHttpRawUploadedContent, typeof(int), typeof(int) };
            Type[] parameters = { typeof(string), typeof(string), typeHttpInputStream };

            // Create an HttpRawUploadedContent instance
            object uploadedContent = typeHttpRawUploadedContent
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, uploadedParams, null)
              .Invoke(new object[] { data.Length, data.Length });

            // Call the AddBytes method
            typeHttpRawUploadedContent
              .GetMethod("AddBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, new object[] { data, 0, data.Length });

            // This is necessary if you will be using the returned content (ie to Save)
            typeHttpRawUploadedContent
              .GetMethod("DoneAddingBytes", BindingFlags.NonPublic | BindingFlags.Instance)
              .Invoke(uploadedContent, null);

            // Create an HttpInputStream instance
            object stream = (Stream)typeHttpInputStream
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, streamParams, null)
              .Invoke(new object[] { uploadedContent, 0, data.Length });

            // Create an HttpPostedFile instance
            HttpPostedFile postedFile = (HttpPostedFile)typeof(HttpPostedFile)
              .GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, parameters, null)
              .Invoke(new object[] { filename, contentType, stream });

            return postedFile;
        }
    }
}
