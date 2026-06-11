# RAG POC - Python

A retrieval-augmented generation proof of concept built entirely on
open-source tools. No API keys, no paid services - everything runs locally.

## Stack

| Component       | Tool                                        | License    |
| --------------- | ------------------------------------------- | ---------- |
| Orchestration   | [LangChain](https://github.com/langchain-ai/langchain) | MIT |
| Embeddings      | [sentence-transformers](https://github.com/UKPLab/sentence-transformers) `all-MiniLM-L6-v2` | Apache-2.0 |
| Vector database | [ChromaDB](https://github.com/chroma-core/chroma) | Apache-2.0 |
| LLM serving     | [Ollama](https://github.com/ollama/ollama) + `llama3.2` | MIT |
| REST API        | [FastAPI](https://github.com/fastapi/fastapi) + [Uvicorn](https://github.com/encode/uvicorn) | MIT / BSD |

## Prerequisites

1. Python 3.10+
2. [Install Ollama](https://ollama.com/download), then pull the LLM:

   ```bash
   ollama pull llama3.2
   ```

   Ollama serves on `http://localhost:11434` by default.

## Setup

```bash
cd rag-poc/python
python -m venv .venv
source .venv/bin/activate        # Windows: .venv\Scripts\activate
pip install -r requirements.txt
```

## Usage

### 1. Ingest documents

Drop your `.md`, `.txt`, or `.pdf` files into `data/docs/` (three sample
documents are included), then build the vector store:

```bash
python -m rag_poc.ingest
```

### 2. Ask questions (CLI)

```bash
python -m rag_poc.query "Why use RAG instead of fine-tuning?"
# or start an interactive loop:
python -m rag_poc.query
```

### 3. Ask questions (REST API)

```bash
uvicorn rag_poc.api:app --port 8000
curl -X POST http://localhost:8000/query \
     -H "Content-Type: application/json" \
     -d '{"question": "What is a Docker image?"}'
```

## Configuration

Everything is overridable via environment variables (see `rag_poc/config.py`):

| Variable              | Default                                  |
| --------------------- | ---------------------------------------- |
| `RAG_LLM_MODEL`       | `llama3.2`                               |
| `RAG_EMBEDDING_MODEL` | `sentence-transformers/all-MiniLM-L6-v2` |
| `OLLAMA_BASE_URL`     | `http://localhost:11434`                 |
| `RAG_CHUNK_SIZE`      | `800`                                    |
| `RAG_CHUNK_OVERLAP`   | `120`                                    |
| `RAG_TOP_K`           | `4`                                      |

## How it works

```
data/docs/*  -->  loaders  -->  splitter  -->  sentence-transformers  -->  ChromaDB
                                                                              |
user question  -->  embed  -->  top-k similarity search  -->  context  -->  Ollama (llama3.2)  -->  answer + sources
```
