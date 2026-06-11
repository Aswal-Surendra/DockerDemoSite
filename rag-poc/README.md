# RAG Proof of Concept - Python & C#

Two implementations of the same retrieval-augmented generation (RAG) pipeline,
built **entirely with open-source tools**. Nothing here needs an API key or a
paid service - the embedding model, vector store, and LLM all run locally.

```
            INGESTION                                 QUERY
 ┌──────────────────────────────┐      ┌──────────────────────────────────┐
 │ documents (md / txt / pdf)   │      │ user question                    │
 │        │                     │      │      │                          │
 │   load + chunk               │      │   embed question                 │
 │        │                     │      │      │                          │
 │   embed chunks               │      │   top-k similarity search        │
 │        │                     │      │      │                          │
 │   vector store  ─────────────┼──────┼──►  retrieved context            │
 └──────────────────────────────┘      │      │                          │
                                       │   local LLM (Ollama)            │
                                       │      │                          │
                                       │   grounded answer + sources     │
                                       └──────────────────────────────────┘
```

## The two projects

| | [`python/`](python/) | [`csharp/`](csharp/) |
| --- | --- | --- |
| Orchestration | LangChain (MIT) | Plain .NET 8 (MIT) |
| Embeddings | sentence-transformers `all-MiniLM-L6-v2` (Apache-2.0) | `nomic-embed-text` via Ollama (Apache-2.0) |
| Vector store | ChromaDB (Apache-2.0) | In-memory + JSON (this repo) |
| LLM | `llama3.2` via Ollama (MIT) | `llama3.2` via Ollama (MIT) |
| Interfaces | CLI + FastAPI REST API | CLI (one-shot + interactive chat) |

Each folder has its own README with setup and usage instructions.

## Common prerequisite: Ollama

Both projects use [Ollama](https://ollama.com/download) (MIT license) to serve
the LLM locally on `http://localhost:11434`:

```bash
ollama pull llama3.2          # used by both projects
ollama pull nomic-embed-text  # used by the C# project for embeddings
```

## Quick start

```bash
# Python
cd rag-poc/python
python -m venv .venv && source .venv/bin/activate
pip install -r requirements.txt
python -m rag_poc.ingest
python -m rag_poc.query "What is RAG?"

# C#
cd rag-poc/csharp/RagPoc
dotnet run -- ingest
dotnet run -- query "What is RAG?"
```
