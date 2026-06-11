using System.Text.Json;

namespace RagPoc;

public sealed record DocumentChunk(string Source, string Text, float[] Vector);

public sealed record SearchHit(DocumentChunk Chunk, double Score);

/// <summary>
/// A tiny in-memory vector store with JSON persistence and brute-force cosine
/// similarity search. Plenty for a POC; swap in Qdrant, Milvus, or pgvector
/// (all open source) for production-sized corpora.
/// </summary>
public sealed class VectorStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly List<DocumentChunk> _chunks = [];

    public int Count => _chunks.Count;

    public void Add(DocumentChunk chunk) => _chunks.Add(chunk);

    public List<SearchHit> Search(float[] queryVector, int topK) =>
        _chunks
            .Select(chunk => new SearchHit(chunk, CosineSimilarity(queryVector, chunk.Vector)))
            .OrderByDescending(hit => hit.Score)
            .Take(topK)
            .ToList();

    public async Task SaveAsync(string path)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, _chunks, JsonOptions);
    }

    public static async Task<VectorStore> LoadAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var chunks = await JsonSerializer.DeserializeAsync<List<DocumentChunk>>(stream, JsonOptions)
            ?? throw new InvalidOperationException($"Could not read vector store at {path}");

        var store = new VectorStore();
        store._chunks.AddRange(chunks);
        return store;
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException(
                $"Vector dimensions differ ({a.Length} vs {b.Length}). " +
                "Re-run ingest if you changed the embedding model.");

        double dot = 0, normA = 0, normB = 0;
        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        var denominator = Math.Sqrt(normA) * Math.Sqrt(normB);
        return denominator == 0 ? 0 : dot / denominator;
    }
}
