# Open-Source Stack Used in This POC

Every tool in this proof of concept is open source and runs locally.
No API keys or paid services are required.

## Ollama (MIT license)

Ollama serves open-weight large language models on your own machine through a
simple HTTP API on port 11434. This POC uses the `llama3.2` model for answer
generation. Other models such as `mistral`, `phi3`, or `qwen2.5` can be used
by changing one configuration value.

## Sentence-Transformers (Apache-2.0 license)

The `all-MiniLM-L6-v2` model converts text into 384-dimensional embedding
vectors. It is small (about 80 MB), fast on CPU, and works completely offline
after the first download.

## ChromaDB (Apache-2.0 license)

Chroma is an embedded vector database. It persists embeddings to a local
folder and performs fast approximate nearest-neighbour search using cosine
similarity, so no external database server is needed.

## LangChain (MIT license)

LangChain provides the orchestration layer: document loaders, text splitters,
retriever abstractions, and prompt templates that connect the embedding
model, the vector store, and the LLM into a single pipeline.

## nomic-embed-text (Apache-2.0 license)

The C# version of this POC uses the `nomic-embed-text` embedding model served
by Ollama, which produces 768-dimensional vectors and also runs fully locally.
