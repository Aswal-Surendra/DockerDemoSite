# Docker Basics

Docker is an open platform for developing, shipping, and running applications
inside lightweight, isolated environments called containers.

## Images and containers

A Docker **image** is a read-only template containing the application code,
runtime, libraries, and settings. A **container** is a running instance of an
image. Many containers can run from the same image at once.

## Dockerfile

A Dockerfile is a text file with instructions to build an image. Common
instructions include `FROM` (base image), `COPY` (add files), `RUN` (execute
build commands), `EXPOSE` (document ports), and `ENTRYPOINT`/`CMD` (the
process to start).

## Useful commands

- `docker build -t myapp .` - build an image from a Dockerfile.
- `docker run -p 8080:80 myapp` - run a container and map port 8080 to 80.
- `docker ps` - list running containers.
- `docker compose up` - start a multi-container application defined in a
  `compose.yaml` file.

## Why containers help RAG deployments

Containers package the vector database, the embedding model runtime, and the
LLM server (such as Ollama) with pinned versions, so the whole RAG pipeline
runs identically on any machine.
