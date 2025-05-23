using RagChatPoC.Api.Utils;

namespace RagChatPoC.Api.UnitTests.Utils;

public class EmbeddingUtilsTests
{
    [Fact]
    public void CosineSimilarity_ShouldReturnOne_WhenVectorsAreIdentical()
    {
        var vectorA = new float[] { 1, 2, 3 };
        var vectorB = new float[] { 1, 2, 3 };

        var result = EmbeddingUtils.CosineSimilarity(vectorA, vectorB);

        Assert.Equal(1, result, 5);
    }

    [Fact]
    public void CosineSimilarity_ShouldReturnZero_WhenVectorsAreOrthogonal()
    {
        var vectorA = new float[] { 1, 0 };
        var vectorB = new float[] { 0, 1 };

        var result = EmbeddingUtils.CosineSimilarity(vectorA, vectorB);

        Assert.Equal(0, result, 5);
    }

    [Fact]
    public void CosineSimilarity_ShouldThrowArgumentException_WhenVectorsHaveDifferentLengths()
    {
        var vectorA = new float[] { 1, 2, 3 };
        var vectorB = new float[] { 1, 2 };

        Assert.Throws<ArgumentException>(() => EmbeddingUtils.CosineSimilarity(vectorA, vectorB));
    }

    [Fact]
    public void CosineSimilarity_ShouldHandleZeroMagnitudeVectors()
    {
        var vectorA = new float[] { 0, 0, 0 };
        var vectorB = new float[] { 0, 0, 0 };

        var result = EmbeddingUtils.CosineSimilarity(vectorA, vectorB);

        Assert.Equal(0, result, 5);
    }

    [Fact]
    public void CosineSimilarity_ShouldReturnNegativeValue_WhenVectorsAreOpposite()
    {
        var vectorA = new float[] { 1, 0 };
        var vectorB = new float[] { -1, 0 };

        var result = EmbeddingUtils.CosineSimilarity(vectorA, vectorB);

        Assert.Equal(-1, result, 5);
    }
}