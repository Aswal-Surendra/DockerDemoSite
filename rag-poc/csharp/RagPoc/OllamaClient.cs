using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RagPoc;

/// <summary>
/// Minimal client for the Ollama REST API (https://github.com/ollama/ollama).
/// Ollama is open source (MIT) and serves both the embedding model and the LLM
/// locally, so no API keys or external services are needed.
/// </summary>
public sealed class OllamaClient : IDisposable
{
    private readonly HttpClient _http;

    public OllamaClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromMinutes(5),
        };
    }

    /// <summary>Embed a batch of texts using POST /api/embed.</summary>
    public async Task<List<float[]>> EmbedAsync(string model, IReadOnlyList<string> inputs)
    {
        var response = await _http.PostAsJsonAsync(
            "/api/embed", new EmbedRequest(model, inputs));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<EmbedResponse>()
            ?? throw new InvalidOperationException("Empty response from /api/embed");
        return body.Embeddings;
    }

    /// <summary>Run a chat completion using POST /api/chat (non-streaming).</summary>
    public async Task<string> ChatAsync(string model, IReadOnlyList<ChatMessage> messages)
    {
        var response = await _http.PostAsJsonAsync(
            "/api/chat", new ChatRequest(model, messages, Stream: false));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>()
            ?? throw new InvalidOperationException("Empty response from /api/chat");
        return body.Message.Content;
    }

    public void Dispose() => _http.Dispose();

    private sealed record EmbedRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("input")] IReadOnlyList<string> Input);

    private sealed record EmbedResponse(
        [property: JsonPropertyName("embeddings")] List<float[]> Embeddings);

    private sealed record ChatRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IReadOnlyList<ChatMessage> Messages,
        [property: JsonPropertyName("stream")] bool Stream);

    private sealed record ChatResponse(
        [property: JsonPropertyName("message")] ChatMessage Message);
}

public sealed record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content);
