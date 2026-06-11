"""Interactive CLI for asking questions against the ingested documents.

Run from the python/ directory:
    python -m rag_poc.query                 # interactive loop
    python -m rag_poc.query "What is RAG?"  # one-shot question
"""

import sys

from .rag_chain import RagChain


def print_answer(chain: RagChain, question: str) -> None:
    result = chain.ask(question)
    print(f"\n{result.answer}\n")
    print("Sources:")
    for source in result.sources:
        print(f"  - {source}")
    print()


def main() -> None:
    chain = RagChain()

    if len(sys.argv) > 1:
        print_answer(chain, " ".join(sys.argv[1:]))
        return

    print("RAG POC - ask a question (empty line or Ctrl-C to exit)\n")
    while True:
        try:
            question = input("Q> ").strip()
        except (EOFError, KeyboardInterrupt):
            print()
            break
        if not question:
            break
        print_answer(chain, question)


if __name__ == "__main__":
    main()
