namespace Api;
using TanvirArjel.EFCore.GenericRepository;

public class AuthorizerService
{
    private readonly ILogger<AuthorizerService> _logger;
    private readonly IRepository<Database> _repository;

    public AuthorizerService(ILogger<AuthorizerService> logger, IRepository<Database> repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Authorize(string apiKey)
    {
        if(string.IsNullOrEmpty(apiKey))
            throw new Exception("Invalid API key. Key cannot be empty.");

        if(!Guid.TryParse(apiKey, out var key))
            throw new Exception("Invalid API key. Key must be a valid GUID.");
        await Authorize(key);
    }

    public async Task Authorize(Guid apiKey)
    {
        if(apiKey == Guid.Empty)
            throw new Exception("Invalid API key. Key cannot be empty.");

        var key = await _repository.GetByIdAsync<ApiKey>(apiKey);
        if(key == null)
            throw new Exception("Invalid API key. Key does not exist.");
        _logger.LogDebug($"API key authorized: { apiKey }");
    }

    public Guid CreateApiKey(string name)
    {
        var apiKey = Guid.NewGuid();
        _repository.Add(new ApiKey(apiKey, name));
        _repository.SaveChangesAsync();
        return apiKey;
    }

    public async Task DeleteApiKey(string apiKey)
    {
        if(!Guid.TryParse(apiKey, out var key))
            throw new Exception("Invalid API key. Key must be a valid GUID.");
        await DeleteApiKey(key);
    }

    public async Task DeleteApiKey(Guid apiKey)
    {
        var key = await _repository.GetByIdAsync<ApiKey>(apiKey);
        if(key == null)
            throw new Exception("Invalid API key. Key does not exist.");
        _repository.Remove(key);
        await _repository.SaveChangesAsync();
    }
}