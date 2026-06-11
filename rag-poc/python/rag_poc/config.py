"""Central configuration for the RAG POC.

Every value can be overridden with an environment variable so the POC can be
pointed at different models/stores without touching code.
"""

import os
from pathlib import Path

# Project layout
PROJECT_ROOT = Path(__file__).resolve().parent.parent
DOCS_DIR = Path(os.getenv("RAG_DOCS_DIR", PROJECT_ROOT / "data" / "docs"))
CHROMA_DIR = Path(os.getenv("RAG_CHROMA_DIR", PROJECT_ROOT / "chroma_db"))
COLLECTION_NAME = os.getenv("RAG_COLLECTION", "rag_poc")

# Embeddings: runs fully locally via sentence-transformers (Apache-2.0)
EMBEDDING_MODEL = os.getenv(
    "RAG_EMBEDDING_MODEL", "sentence-transformers/all-MiniLM-L6-v2"
)

# LLM: served locally by Ollama (MIT). Pull the model first:  ollama pull llama3.2
OLLAMA_BASE_URL = os.getenv("OLLAMA_BASE_URL", "http://localhost:11434")
LLM_MODEL = os.getenv("RAG_LLM_MODEL", "llama3.2")
LLM_TEMPERATURE = float(os.getenv("RAG_LLM_TEMPERATURE", "0.1"))

# Chunking / retrieval
CHUNK_SIZE = int(os.getenv("RAG_CHUNK_SIZE", "800"))
CHUNK_OVERLAP = int(os.getenv("RAG_CHUNK_OVERLAP", "120"))
TOP_K = int(os.getenv("RAG_TOP_K", "4"))
