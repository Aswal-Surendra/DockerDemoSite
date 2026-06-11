# Retrieval-Augmented Generation (RAG) Overview

Retrieval-Augmented Generation (RAG) is an architecture that improves the
answers of a large language model (LLM) by giving it relevant, up-to-date
context retrieved from a knowledge base at question time.

## How RAG works

1. **Ingestion**: Documents are loaded, split into smaller chunks, converted
   into numeric vectors (embeddings), and stored in a vector database.
2. **Retrieval**: When a user asks a question, the question is embedded with
   the same model and the vector database returns the most similar chunks.
3. **Generation**: The retrieved chunks are placed into the LLM prompt as
   context, and the model generates an answer grounded in that context.

## Why use RAG?

- Reduces hallucinations because the model answers from real documents.
- Keeps answers current without retraining or fine-tuning the model.
- Provides citations: each answer can reference the source documents.
- Keeps private data private when run with local, open-source models.

## Key parameters

- **Chunk size**: how large each text chunk is (this POC uses 800 characters).
- **Chunk overlap**: how much adjacent chunks overlap (120 characters here).
- **Top-k**: how many chunks are retrieved per question (4 in this POC).
