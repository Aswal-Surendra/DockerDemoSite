"""Retrieval-augmented generation chain: retrieve from Chroma, answer with Ollama."""

from dataclasses import dataclass, field

from langchain_chroma import Chroma
from langchain_core.documents import Document
from langchain_core.prompts import ChatPromptTemplate
from langchain_huggingface import HuggingFaceEmbeddings
from langchain_ollama import ChatOllama

from . import config

SYSTEM_PROMPT = (
    "You are a helpful assistant that answers questions strictly based on the "
    "provided context. If the context does not contain the answer, say you "
    "don't know - do not make anything up. Cite the source file names you used."
)

PROMPT = ChatPromptTemplate.from_messages(
    [
        ("system", SYSTEM_PROMPT),
        (
            "human",
            "Context:\n{context}\n\nQuestion: {question}\n\nAnswer:",
        ),
    ]
)


@dataclass
class RagAnswer:
    question: str
    answer: str
    sources: list[str] = field(default_factory=list)


def format_context(documents: list[Document]) -> str:
    parts = []
    for doc in documents:
        source = doc.metadata.get("source", "unknown")
        parts.append(f"[source: {source}]\n{doc.page_content}")
    return "\n\n---\n\n".join(parts)


class RagChain:
    """Thin wrapper bundling retriever + LLM so the CLI and API can share it."""

    def __init__(self) -> None:
        embeddings = HuggingFaceEmbeddings(model_name=config.EMBEDDING_MODEL)
        self.store = Chroma(
            collection_name=config.COLLECTION_NAME,
            embedding_function=embeddings,
            persist_directory=str(config.CHROMA_DIR),
        )
        self.retriever = self.store.as_retriever(search_kwargs={"k": config.TOP_K})
        self.llm = ChatOllama(
            model=config.LLM_MODEL,
            base_url=config.OLLAMA_BASE_URL,
            temperature=config.LLM_TEMPERATURE,
        )

    def ask(self, question: str) -> RagAnswer:
        documents = self.retriever.invoke(question)
        messages = PROMPT.format_messages(
            context=format_context(documents), question=question
        )
        response = self.llm.invoke(messages)
        sources = sorted({d.metadata.get("source", "unknown") for d in documents})
        return RagAnswer(question=question, answer=response.content, sources=sources)
