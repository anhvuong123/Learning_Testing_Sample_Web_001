using System.Text.Json;
using System.Text.Json.Serialization;

namespace PersonalFinanceTracker.Api.Services;

public sealed class JsonFileStore
{
    private readonly string _dataDirectory;
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonFileStore(IWebHostEnvironment environment)
    {
        _dataDirectory = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(_dataDirectory);
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<List<T>> ReadAsync<T>(string fileName)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        await _lock.WaitAsync();
        try
        {
            if (!File.Exists(path))
                return [];

            await using var stream = File.OpenRead(path);
            return await JsonSerializer.DeserializeAsync<List<T>>(stream, _options) ?? [];
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task WriteAsync<T>(string fileName, List<T> items)
    {
        var path = Path.Combine(_dataDirectory, fileName);
        var temporaryPath = $"{path}.tmp";
        await _lock.WaitAsync();
        try
        {
            await using (var stream = File.Create(temporaryPath))
                await JsonSerializer.SerializeAsync(stream, items, _options);
            File.Move(temporaryPath, path, true);
        }
        finally
        {
            _lock.Release();
        }
    }
}
