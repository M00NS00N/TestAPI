using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestAPI.DAL.Models
{
    [Table("Product", Schema = "Production")]
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        public string Name { get; set; }
        public string ProductNumber { get; set; } = string.Empty;
        public bool MakeFlag { get; set; }
        public bool FinishedGoodsFlag { get; set; }
        public short SafetyStockLevel { get; set; } = 1;
        public short ReorderPoint { get; set; } = 1;
        public decimal StandardCost { get; set; }
        public decimal ListPrice { get; set; }
        public int DaysToManufacture { get; set; } = 1;
        public DateTime SellStartDate { get; set; } = DateTime.Now;
    }
}
