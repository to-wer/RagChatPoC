namespace RagApi.Utils;

public static class EmbeddingUtils
{
    /// <summary>
    /// Calculates the cosine similarity between two vectors.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The cosine similarity as a float value.</returns>
    /// <exception cref="ArgumentException">Thrown when the vectors have different lengths.</exception>
    public static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vector sizes are not equal.");

        const float epsilon = 1e-10f; // Avoids division by zero
        float dot = 0, magA = 0, magB = 0;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB) + epsilon);
    }
}