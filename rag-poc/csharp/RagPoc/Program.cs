using RagPoc;

const string SystemPrompt =
    "You are a helpful assistant that answers questions strictly based on the " +
    "provided context. If the context does not contain the answer, say you " +
    "don't know - do not make anything up. Cite the source file names you used.";

return args switch
{
    ["ingest"] => await IngestAsync(),
    ["query", .. var rest] when rest.Length > 0 => await QueryAsync(string.Join(' ', rest)),
    ["chat"] or ["query"] => await ChatLoopAsync(),
    _ => PrintUsage(),
};

static int PrintUsage()
{
    Console.WriteLine("""
        RAG POC (C#) - retrieval-augmented generation with open-source tools.

        Usage:
          dotnet run -- ingest              Build the vector store from data/docs
          dotnet run -- query <question>    Ask a one-shot question
          dotnet run -- chat                Interactive question loop
        """);
    return 1;
}

static async Task<int> IngestAsync()
{
    if (!Directory.Exists(AppConfig.DocsDir))
    {
        Console.Error.WriteLine($"Documents directory not found: {AppConfig.DocsDir}");
        return 1;
    }

    var files = Directory.EnumerateFiles(AppConfig.DocsDir, "*.*", SearchOption.AllDirectories)
        .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                 || f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
        .OrderBy(f => f)
        .ToList();

    if (files.Count == 0)
    {
        Console.Error.WriteLine($"No .md or .txt files found in {AppConfig.DocsDir}");
        return 1;
    }

    Console.WriteLine($"Loading {files.Count} document(s) from {AppConfig.DocsDir} ...");

    var sources = new List<string>();
    var texts = new List<string>();
    foreach (var file in files)
    {
        var content = await File.ReadAllTextAsync(file);
        foreach (var chunk in TextChunker.Split(content, AppConfig.ChunkSize, AppConfig.ChunkOverlap))
        {
            sources.Add(Path.GetFileName(file));
            texts.Add(chunk);
        }
    }
    Console.WriteLine($"  split into {texts.Count} chunk(s)");

    Console.WriteLine($"Embedding with {AppConfig.EmbeddingModel} via Ollama ...");
    using var ollama = new OllamaClient(AppConfig.OllamaBaseUrl);
    var vectors = await ollama.EmbedAsync(AppConfig.EmbeddingModel, texts);

    var store = new VectorStore();
    for (var i = 0; i < texts.Count; i++)
        store.Add(new DocumentChunk(sources[i], texts[i], vectors[i]));

    await store.SaveAsync(AppConfig.StorePath);
    Console.WriteLine($"Done. {store.Count} chunks persisted to {AppConfig.StorePath}");
    return 0;
}

static async Task<int> QueryAsync(string question)
{
    var (store, ollama) = await OpenStoreAsync();
    if (store is null || ollama is null) return 1;

    using (ollama)
    {
        await AnswerAsync(store, ollama, question);
    }
    return 0;
}

static async Task<int> ChatLoopAsync()
{
    var (store, ollama) = await OpenStoreAsync();
    if (store is null || ollama is null) return 1;

    using (ollama)
    {
        Console.WriteLine("RAG POC - ask a question (empty line to exit)\n");
        while (true)
        {
            Console.Write("Q> ");
            var question = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(question)) break;
            await AnswerAsync(store, ollama, question);
        }
    }
    return 0;
}

static async Task<(VectorStore?, OllamaClient?)> OpenStoreAsync()
{
    if (!File.Exists(AppConfig.StorePath))
    {
        Console.Error.WriteLine(
            $"Vector store not found at {AppConfig.StorePath}. Run 'dotnet run -- ingest' first.");
        return (null, null);
    }

    var store = await VectorStore.LoadAsync(AppConfig.StorePath);
    return (store, new OllamaClient(AppConfig.OllamaBaseUrl));
}

static async Task AnswerAsync(VectorStore store, OllamaClient ollama, string question)
{
    var queryVector = (await ollama.EmbedAsync(AppConfig.EmbeddingModel, [question]))[0];
    var hits = store.Search(queryVector, AppConfig.TopK);

    var context = string.Join(
        "\n\n---\n\n",
        hits.Select(hit => $"[source: {hit.Chunk.Source}]\n{hit.Chunk.Text}"));

    var answer = await ollama.ChatAsync(AppConfig.LlmModel,
    [
        new ChatMessage("system", SystemPrompt),
        new ChatMessage("user", $"Context:\n{context}\n\nQuestion: {question}\n\nAnswer:"),
    ]);

    Console.WriteLine($"\n{answer}\n");
    Console.WriteLine("Sources:");
    foreach (var source in hits.Select(h => h.Chunk.Source).Distinct().Order())
        Console.WriteLine($"  - {source}");
    Console.WriteLine();
}
