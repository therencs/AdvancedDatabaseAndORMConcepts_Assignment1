using WebApplication2.Data;
using WebApplication2.Models;
using WebApplication2.Models.ResponseModel;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using System.IO.Pipelines;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LaptopStoresContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("LaptopStoresConnectionString"));
});

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;

    await SeedData.Init(services);
}

// DEBUG

app.MapGet("/laptops/idonly", (LaptopStoresContext db) =>
{
    HashSet<Guid> LaptopIDsQuery = db.Laptops.Select(l => l.Number).ToHashSet();

    return Results.Ok(LaptopIDsQuery);
});

app.MapGet("/stores/idonly", (LaptopStoresContext db) =>
{
    HashSet<Guid> StoreIDsQuery = db.StoreLocations.Select(s => s.StoreNumber).ToHashSet();

    return Results.Ok(StoreIDsQuery);
});

// END OF DEBUG

app.MapGet("/laptops/search", (LaptopStoresContext db, 
    decimal? priceAbove, 
    decimal? priceBelow, 
    Guid? stockInStore, 
    string? stockInProvince, 
    LaptopCondition? inCondition, 
    int? fromBrand, 
    string? searchPhrase)
=> {

    try
    {
        HashSet<Laptop> LaptopsQuery = db.Laptops
        .Include(l => l.Brand).ToHashSet();
        //.Include(la => la.StoreLaptops).ThenInclude(sl => sl.StoreLocation).ToHashSet();

        if (priceAbove != null)
        {
            LaptopsQuery = LaptopsQuery.Where(laptop => laptop.Price > priceAbove).ToHashSet();
        }

        if (priceBelow != null)
        {
            LaptopsQuery = LaptopsQuery.Where(laptop => laptop.Price < priceBelow).ToHashSet();
        }

        if ((stockInStore != null && stockInProvince == null) || (stockInProvince != null && stockInStore == null))
        {
            if (stockInStore != null)
            {
                LaptopsQuery = LaptopsQuery.Where(l => l.StoreLaptops.Any(sl => sl.StoreId == stockInStore && sl.StoreLocation.Quantity > 0)).ToHashSet();

            }
            else if (stockInProvince != null){

                LaptopsQuery = LaptopsQuery.Where(l => l.StoreLaptops.Any(sl => sl.StoreLocation.Province == stockInProvince && sl.StoreLocation.Quantity > 0)).ToHashSet();

            }

        }
        else if (stockInStore != null && stockInProvince != null)
        {
            throw new InvalidOperationException();

        }

        if (inCondition != null)
        {
            LaptopsQuery = LaptopsQuery.Where(laptop => laptop.Condition == inCondition).ToHashSet();
        }

        if (fromBrand != null)
        {
            LaptopsQuery = LaptopsQuery.Where(laptop => laptop.Brand.Id == fromBrand).ToHashSet();
        }

        if (searchPhrase != null)
        {
            LaptopsQuery = LaptopsQuery.Where(laptop => laptop.Model.Contains(searchPhrase)).ToHashSet();
        }

        if (LaptopsQuery.Count > 0)
        {
            return Results.Ok(LaptopsQuery);
        } else
        {
            throw new Exception();
        }

    } catch (InvalidOperationException ex)
    {
        return Results.Problem(ex.Message);
    } catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }

});

app.MapGet("/stores/{storeNumber}/inventory", (LaptopStoresContext db, Guid storeNumber) => 
{
    try
    {
        HashSet<Laptop> LaptopsQuery = db.Laptops
        .Include(l => l.StoreLaptops).ThenInclude(sl => sl.StoreLocation).ToHashSet();

        if (!LaptopsQuery.Any(l => l.StoreLaptops.Any(sl => sl.StoreLocation.Quantity > 0)))
        {
            throw new Exception();
        }

        LaptopsQuery = LaptopsQuery.Where(l => l.StoreLaptops.Any(sl => sl.StoreLocation.Quantity > 0 && sl.StoreLocation.StoreNumber == storeNumber)).ToHashSet();

        return Results.Ok(LaptopsQuery);

    } catch (Exception ex)
    {

        return Results.BadRequest(ex.Message);
    }
});

app.MapPost("/stores/{storeNumber}/{laptopNumber}/changeQuantity", async (LaptopStoresContext db, int amount, Guid storeNumber, Guid laptopNumber) =>
{
    try
    {
        // I made this async so that the request successfully gathers all the information it needs before moving on the error handling.

        StoreLocation? store = await db.StoreLocations.FindAsync(storeNumber);

        if (store == null)
        {
            throw new Exception();
        }

        if (amount == 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        StoreLaptop? storeLaptop = await db.StoreLaptops.FirstOrDefaultAsync(sl => sl.StoreId == storeNumber && sl.LaptopId == laptopNumber);

        if (storeLaptop == null)
        {
            throw new Exception();
        }
        else
        {
            storeLaptop.StoreLocation.Quantity = amount;
            await db.SaveChangesAsync();
            return Results.Created($"/stores/{storeNumber}/", storeLaptop);
        }
    } catch (ArgumentOutOfRangeException ex)
    {
        return Results.NotFound(ex.Message);
    } catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    } 
    
    

});

app.MapGet("/laptops/avgOfBrand/{brandName}", (LaptopStoresContext db, string brandName) =>
{
    try
    {
        HashSet<Laptop> LaptopsQuery = db.Laptops
        .Include(l => l.Brand).ToHashSet();

        int laptopCount;
        decimal laptopPrice;


        if (LaptopsQuery.Any(l => l.Brand.Name == brandName))
        {
            laptopPrice = LaptopsQuery.Where(l => l.Brand.Name == brandName).Select(bl => bl.Price).Average();
            laptopCount = LaptopsQuery.Where(l => l.Brand.Name == brandName).Count();

            AvgWithCountOfBrand SumAndAvgResults = new AvgWithCountOfBrand { LaptopCount = laptopCount, AveragePrice = laptopPrice };

            return Results.Ok(SumAndAvgResults);
            // return ResponseModel
        } else
        {
            throw new ArgumentException();
        }
       

    } catch (ArgumentException ex)
    {
        return Results.NotFound(ex.Message);
    } catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapGet("/stores/storesInProvinces/", (LaptopStoresContext db) =>
{   
    try
    {
        IEnumerable<IGrouping<string, StoreLocation>> storesByProvince = db.StoreLocations.GroupBy(sl => sl.Province);


        if (storesByProvince.Any())
        {
            return Results.Ok(storesByProvince);
        } else
        {
            throw new Exception();
        }
        
    } catch (Exception ex)
    {
        return Results.BadRequest(ex.Message);
    }
    
});


app.Run();
