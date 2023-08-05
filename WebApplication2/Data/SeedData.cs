using WebApplication2.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Data
{
    public class SeedData
    {
        public async static Task Init(IServiceProvider serviceProvider)
        {
            LaptopStoresContext db = new LaptopStoresContext(serviceProvider.GetRequiredService<DbContextOptions<LaptopStoresContext>>());

            db.Database.EnsureDeleted();
            db.Database.Migrate();

            // Init brands

            Brand firstBrand = new Brand { Name = "ASUS"};
            Brand secondBrand = new Brand { Name = "Dell"};
            Brand thirdBrand = new Brand { Name = "Apple" };

            if (!db.Brands.Any()) // If there arent any current brands in the database, create em
            {
                db.Add(firstBrand);
                db.Add(secondBrand);
                db.Add(thirdBrand);
                db.SaveChanges();
            }

            // Init Laptops 

            Laptop laptop1 = new Laptop("Latitude 9510", secondBrand, 1200, LaptopCondition.Refurbished);
            Laptop laptop2 = new Laptop("Inspiron 15", secondBrand, 800, LaptopCondition.New);
            Laptop laptop3 = new Laptop("Macbook Pro", thirdBrand, 1200, LaptopCondition.Refurbished);
            Laptop laptop4 = new Laptop("iMac", thirdBrand, 400, LaptopCondition.Rental);
            Laptop laptop5 = new Laptop("ROG Strix G17", firstBrand, 1500, LaptopCondition.New);
            Laptop laptop6 = new Laptop("TUF Gaming A15", firstBrand, 1200, LaptopCondition.Rental);

            if (!db.Laptops.Any())
            {
                db.AddRange(
                    laptop1, 
                    laptop2, 
                    laptop3, 
                    laptop4, 
                    laptop5, 
                    laptop6
                );
                db.SaveChanges();
            }

            // Init StoreLocations

            StoreLocation firstStore = new StoreLocation("123 Main St", "British Columbia", 10);

            StoreLocation secondStore = new StoreLocation ("456 Elm St", "Ontario", 5);

            StoreLocation thirdStore = new StoreLocation("777 Valentin Ave", "Quebec", 3);

            if (!db.StoreLocations.Any())
            {
                db.Add(firstStore);
                db.Add(secondStore);
                db.Add(thirdStore);

                db.SaveChanges();
            }

            // Init StoreLaptops

            if (!db.StoreLaptops.Any())
            {
                StoreLaptop storeLaptop1 = new StoreLaptop { LaptopId = laptop1.Number, StoreId = firstStore.StoreNumber };
                StoreLaptop storeLaptop2 = new StoreLaptop { LaptopId = laptop2.Number, StoreId = secondStore.StoreNumber };
                StoreLaptop storeLaptop3 = new StoreLaptop { LaptopId = laptop3.Number, StoreId = firstStore.StoreNumber };
                StoreLaptop storeLaptop4 = new StoreLaptop { LaptopId = laptop4.Number, StoreId = thirdStore.StoreNumber };
                StoreLaptop storeLaptop5 = new StoreLaptop { LaptopId = laptop5.Number, StoreId = secondStore.StoreNumber }; 
                StoreLaptop storeLaptop6 = new StoreLaptop { LaptopId = laptop6.Number, StoreId = secondStore.StoreNumber };

                db.AddRange(
                    storeLaptop1,
                    storeLaptop2,
                    storeLaptop3,
                    storeLaptop4,
                    storeLaptop5,
                    storeLaptop6
                );
                db.SaveChanges();
            }
           

            

        }
    }
}
