namespace RagPoc;

/// <summary>
/// Central configuration. Every value can be overridden with an environment
/// variable so the POC can be pointed at different models without recompiling.
/// </summary>
public static class AppConfig
{
    public static string OllamaBaseUrl =>
        Environment.GetEnvironmentVariable("OLLAMA_BASE_URL") ?? "http://localhost:11434";

    /// <summary>LLM served by Ollama. Pull it first: ollama pull llama3.2</summary>
    public static string LlmModel =>
        Environment.GetEnvironmentVariable("RAG_LLM_MODEL") ?? "llama3.2";

    /// <summary>Embedding model served by Ollama: ollama pull nomic-embed-text</summary>
    public static string EmbeddingModel =>
        Environment.GetEnvironmentVariable("RAG_EMBEDDING_MODEL") ?? "nomic-embed-text";

    public static string DocsDir =>
        Environment.GetEnvironmentVariable("RAG_DOCS_DIR")
        ?? Path.Combine(Directory.GetCurrentDirectory(), "data", "docs");

    public static string StorePath =>
        Environment.GetEnvironmentVariable("RAG_STORE_PATH")
        ?? Path.Combine(Directory.GetCurrentDirectory(), "vector_store.json");

    public static int ChunkSize = ReadInt("RAG_CHUNK_SIZE", 800);
    public static int ChunkOverlap = ReadInt("RAG_CHUNK_OVERLAP", 120);
    public static int TopK = ReadInt("RAG_TOP_K", 4);

    private static int ReadInt(string name, int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : fallback;
}
