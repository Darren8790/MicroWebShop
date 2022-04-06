
//using Newtonsoft.Json;
using System.Text.Json;
//using Microsoft.AspNetCore.Hosting.Server;
using StackExchange.Redis;

namespace BasketService.Repository;

public class RedisRepo : IBasketRepo
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisRepo> _logger;

    public RedisRepo(ConnectionMultiplexer redis, ILoggerFactory logger)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger.CreateLogger<RedisRepo>();
    }

    // public async Task<string> TestConnection(string key)
    // {
    //     return await _database.StringGetAsync(key);
    // }

    public async Task<bool> DeleteBasket(string id)
    {
        return await _database.KeyDeleteAsync(id);
    }

    public async Task<CustomerBasket> GetBasket(string customerId)
    {
        var data = await _database.StringGetAsync(customerId);

        if(data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<CustomerBasket>(data, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public IEnumerable<string> GetUsers()
    {
        var server = GetServer();
        var data = server.Keys();
        return data?.Select(k => k.ToString());
    }

    public async Task<CustomerBasket> UpdateBasket(CustomerBasket customerBasket)
    {
        var created = await _database.StringSetAsync(customerBasket.BuyerId, JsonSerializer.Serialize(customerBasket));

        if(!created)
        {
            _logger.LogInformation("Problem creating item!");
            return null;
        }
        _logger.LogInformation("Basket updated!");
        return await GetBasket(customerBasket.BuyerId);
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints();
        return _redis.GetServer(endpoint.First());
    }
}