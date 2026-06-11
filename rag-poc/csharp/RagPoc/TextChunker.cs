namespace RagPoc;

/// <summary>
/// Splits documents into overlapping chunks, preferring paragraph boundaries
/// so chunks stay semantically coherent.
/// </summary>
public static class TextChunker
{
    public static List<string> Split(string text, int chunkSize, int overlap)
    {
        var paragraphs = text
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var chunks = new List<string>();
        var current = new List<string>();
        var currentLength = 0;

        foreach (var paragraph in paragraphs)
        {
            // A single paragraph larger than the chunk size is split by length.
            if (paragraph.Length > chunkSize)
            {
                Flush(chunks, current, ref currentLength);
                for (var start = 0; start < paragraph.Length; start += chunkSize - overlap)
                {
                    var length = Math.Min(chunkSize, paragraph.Length - start);
                    chunks.Add(paragraph.Substring(start, length));
                    if (start + length >= paragraph.Length) break;
                }
                continue;
            }

            if (currentLength + paragraph.Length > chunkSize && current.Count > 0)
            {
                Flush(chunks, current, ref currentLength);
            }

            current.Add(paragraph);
            currentLength += paragraph.Length + 2;
        }

        Flush(chunks, current, ref currentLength);
        return chunks;
    }

    private static void Flush(List<string> chunks, List<string> current, ref int currentLength)
    {
        if (current.Count > 0)
        {
            chunks.Add(string.Join("\n\n", current));
            current.Clear();
        }
        currentLength = 0;
    }
}
