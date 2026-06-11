# RAG POC - C# (.NET 8)

A retrieval-augmented generation proof of concept built entirely on
open-source tools. No API keys, no paid services - everything runs locally.
The pipeline mirrors the Python version in `../python`.

## Stack

| Component       | Tool                                                    | License |
| --------------- | ------------------------------------------------------- | ------- |
| Runtime         | [.NET 8](https://github.com/dotnet/runtime)             | MIT     |
| LLM serving     | [Ollama](https://github.com/ollama/ollama) + `llama3.2` | MIT     |
| Embeddings      | `nomic-embed-text` served by Ollama                     | Apache-2.0 |
| Vector store    | In-memory cosine-similarity store with JSON persistence (this repo) | - |

The vector store is intentionally minimal for the POC. For larger corpora,
swap it for an open-source vector database such as
[Qdrant](https://github.com/qdrant/qdrant),
[Milvus](https://github.com/milvus-io/milvus), or
[pgvector](https://github.com/pgvector/pgvector). For richer orchestration,
[Semantic Kernel](https://github.com/microsoft/semantic-kernel) (MIT) offers
an Ollama connector.

## Prerequisites

1. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
2. [Install Ollama](https://ollama.com/download), then pull both models:

   ```bash
   ollama pull llama3.2
   ollama pull nomic-embed-text
   ```

## Usage

```bash
cd rag-poc/csharp/RagPoc

# 1. Build the vector store from data/docs (three sample documents included)
dotnet run -- ingest

# 2. Ask a one-shot question
dotnet run -- query "Why use RAG instead of fine-tuning?"

# 3. Or start an interactive loop
dotnet run -- chat
```

## Configuration

Override defaults with environment variables (see `AppConfig.cs`):

| Variable              | Default                  |
| --------------------- | ------------------------ |
| `OLLAMA_BASE_URL`     | `http://localhost:11434` |
| `RAG_LLM_MODEL`       | `llama3.2`               |
| `RAG_EMBEDDING_MODEL` | `nomic-embed-text`       |
| `RAG_DOCS_DIR`        | `./data/docs`            |
| `RAG_STORE_PATH`      | `./vector_store.json`    |
| `RAG_CHUNK_SIZE`      | `800`                    |
| `RAG_CHUNK_OVERLAP`   | `120`                    |
| `RAG_TOP_K`           | `4`                      |

## How it works

```
data/docs/*  -->  TextChunker  -->  Ollama /api/embed (nomic-embed-text)  -->  vector_store.json
                                                                                    |
user question  -->  embed  -->  cosine top-k search  -->  context  -->  Ollama /api/chat (llama3.2)  -->  answer + sources
```
