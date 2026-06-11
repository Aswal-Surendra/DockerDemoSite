"""Optional REST API exposing the RAG chain (FastAPI + Uvicorn, both MIT).

Run from the python/ directory:
    uvicorn rag_poc.api:app --reload --port 8000

Then:
    curl -X POST http://localhost:8000/query \
         -H "Content-Type: application/json" \
         -d '{"question": "What is RAG?"}'
"""

from functools import lru_cache

from fastapi import FastAPI
from pydantic import BaseModel

from .rag_chain import RagChain

app = FastAPI(title="RAG POC API", version="0.1.0")


class QueryRequest(BaseModel):
    question: str


class QueryResponse(BaseModel):
    question: str
    answer: str
    sources: list[str]


@lru_cache(maxsize=1)
def get_chain() -> RagChain:
    return RagChain()


@app.get("/health")
def health() -> dict:
    return {"status": "ok"}


@app.post("/query", response_model=QueryResponse)
def query(request: QueryRequest) -> QueryResponse:
    result = get_chain().ask(request.question)
    return QueryResponse(
        question=result.question, answer=result.answer, sources=result.sources
    )
