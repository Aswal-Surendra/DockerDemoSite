"""Ingestion pipeline: load documents -> chunk -> embed -> store in ChromaDB.

Run from the python/ directory:
    python -m rag_poc.ingest
"""

from pathlib import Path

from langchain_chroma import Chroma
from langchain_community.document_loaders import PyPDFLoader, TextLoader
from langchain_core.documents import Document
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_text_splitters import RecursiveCharacterTextSplitter

from . import config

SUPPORTED_TEXT_SUFFIXES = {".txt", ".md", ".markdown", ".rst", ".csv", ".log"}


def load_documents(docs_dir: Path) -> list[Document]:
    """Load every supported file under docs_dir into LangChain Documents."""
    documents: list[Document] = []
    for path in sorted(docs_dir.rglob("*")):
        if not path.is_file():
            continue
        if path.suffix.lower() == ".pdf":
            documents.extend(PyPDFLoader(str(path)).load())
        elif path.suffix.lower() in SUPPORTED_TEXT_SUFFIXES:
            documents.extend(TextLoader(str(path), encoding="utf-8").load())
        else:
            print(f"  skipping unsupported file: {path.name}")
    return documents


def chunk_documents(documents: list[Document]) -> list[Document]:
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=config.CHUNK_SIZE,
        chunk_overlap=config.CHUNK_OVERLAP,
        separators=["\n\n", "\n", ". ", " ", ""],
    )
    return splitter.split_documents(documents)


def build_vector_store(chunks: list[Document]) -> Chroma:
    embeddings = HuggingFaceEmbeddings(model_name=config.EMBEDDING_MODEL)
    return Chroma.from_documents(
        documents=chunks,
        embedding=embeddings,
        collection_name=config.COLLECTION_NAME,
        persist_directory=str(config.CHROMA_DIR),
    )


def main() -> None:
    if not config.DOCS_DIR.exists():
        raise SystemExit(f"Documents directory not found: {config.DOCS_DIR}")

    print(f"Loading documents from {config.DOCS_DIR} ...")
    documents = load_documents(config.DOCS_DIR)
    if not documents:
        raise SystemExit("No documents found - add files to data/docs first.")
    print(f"  loaded {len(documents)} document(s)")

    chunks = chunk_documents(documents)
    print(f"  split into {len(chunks)} chunk(s)")

    print(f"Embedding with {config.EMBEDDING_MODEL} and writing to ChromaDB ...")
    build_vector_store(chunks)
    print(f"Done. Vector store persisted at {config.CHROMA_DIR}")


if __name__ == "__main__":
    main()
