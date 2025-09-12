using System;
using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication application)
    {
        await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(application.Configuration.GetConnectionString("MongoDbConnection")));
        await DB.Index<Item>()
            .Key(a => a.Make, KeyType.Text)
            .Key(a => a.Model, KeyType.Text)
            .Key(a => a.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.CountAsync<Item>();

        using var scope = application.Services.CreateScope();
        var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();
        var items = await httpClient.GetItemsForSearchDb();
        Console.WriteLine(items.Count + " returned from the auction service");

        if (items.Count > 0) await DB.SaveAsync(items);
    }
}
