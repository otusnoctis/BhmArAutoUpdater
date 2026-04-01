using System.Text.Json;

namespace BhmArAutoUpdater.Services;

public sealed class QuoteStorage
{
    private const string QuoteFileName = "quote.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly AppEnvironment _appEnvironment;

    public QuoteStorage(AppEnvironment appEnvironment)
    {
        _appEnvironment = appEnvironment;
    }

    public async Task<QuoteDocument> LoadAsync(CancellationToken cancellationToken = default)
    {
        var filePath = GetQuoteFilePath();
        if (!File.Exists(filePath))
        {
            return new QuoteDocument();
        }

        await using var stream = File.OpenRead(filePath);
        var document = await JsonSerializer.DeserializeAsync<QuoteDocument>(stream, cancellationToken: cancellationToken);
        return document ?? new QuoteDocument();
    }

    public async Task SaveAsync(QuoteDocument document, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_appEnvironment.DataDirectory);

        var filePath = GetQuoteFilePath();
        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, document, JsonOptions, cancellationToken);
    }

    private string GetQuoteFilePath() => Path.Combine(_appEnvironment.DataDirectory, QuoteFileName);
}
