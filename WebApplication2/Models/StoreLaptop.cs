using System.Text.Json.Serialization;

namespace WebApplication2.Models
{
    public class StoreLaptop
    {
        public Guid LaptopId { get; set; }
        public Guid StoreId { get; set; }

        public StoreLocation StoreLocation { get; set; }
        [JsonIgnore] public Laptop Laptop { get; set; }

    }
}
