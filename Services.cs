namespace Api;
using TanvirArjel.EFCore.GenericRepository;
using System.Security.Cryptography;

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

public class FileStorageService
{
    private readonly ILogger<FileStorageService> _logger;
    private readonly IRepository<Database> _repository;

    public FileStorageService(ILogger<FileStorageService> logger, IRepository<Database> repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<Guid> CreateFile(string name, string description, byte[] data)
    {
        if(string.IsNullOrEmpty(name))
            throw new Exception("Invalid file name. Name cannot be empty.");
        if(data == null || data.Length == 0)
            throw new Exception("Invalid file data. Data cannot be empty.");

        var fileRoot = Environment.GetEnvironmentVariable("FILE_ROOT");
        if(string.IsNullOrEmpty(fileRoot))
            throw new Exception("Invalid file root. Root cannot be empty.");
        if(!Directory.Exists(fileRoot))
            throw new Exception("Invalid file root. Root does not exist.");

        var fileDirectory = Path.Combine(fileRoot, name);

        var fileExtension = Path.GetExtension(name);
        if(string.IsNullOrEmpty(fileExtension))
            throw new Exception("Invalid file extension. Extension cannot be empty.");

        // calculate SHA512 hash of file data
        using var sha512 = SHA512.Create();
        var hash = sha512.ComputeHash(data);
        var hashString = hash?.ToString() ?? string.Empty;

        var file = new StoredFile(Guid.NewGuid(), name, fileDirectory, fileExtension, data.Length, DateTime.Now, DateTime.Now, hashString, false);
        _repository.Add(file);
        await _repository.SaveChangesAsync();
        return file.Id;
    }

    public async Task DeleteFile(Guid fileId)
    {
        var file = await _repository.GetByIdAsync<StoredFile>(fileId);
        if(file == null)
            throw new Exception("Invalid file ID. File does not exist.");
        _repository.Remove(file);
        await _repository.SaveChangesAsync();
    }

    public async Task<StoredFile> GetFile(Guid fileId)
    {
        var file = await _repository.GetByIdAsync<StoredFile>(fileId);
        if(file == null)
            throw new Exception("Invalid file ID. File does not exist.");
        return file;
    }

    public async Task<List<StoredFile>> GetFiles()
    {
        return await _repository.GetListAsync<StoredFile>();
    }

    public async Task<byte[]> ReadFile(Guid fileId)
    {
        var file = await _repository.GetByIdAsync<StoredFile>(fileId);
        if(file == null)
            throw new Exception("Invalid file ID. File does not exist.");
        return await File.ReadAllBytesAsync(file.Path);
    }
}