using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace SimpleRagTool;

class Program
{
    private static readonly HttpClient httpClient = new();
    private const string OpenAiApiKey = ""; // Falls OpenAI verwendet werden soll
    private const string EmbeddingModel = "text-embedding-ada-002";

    public static async Task Main(string[] args)
    {
        const string rootDirectory = "C:\\Temp\\SimpleRagTool";
        const string dbPath = "sragt_embeddings.db";

        InitDatabase(dbPath);

        foreach (var file in Directory.GetFiles(rootDirectory, "*.md", SearchOption.AllDirectories))
        {
            Console.WriteLine($"Verarbeite: {file}");
            string content = File.ReadAllText(file);
            var chunks = ChunkText(content, 1000);

            foreach (var chunk in chunks)
            {
                var embedding = await GetEmbedding(chunk);
                SaveEmbeddingToDb(dbPath, file, chunk, embedding);
            }
        }

        Console.WriteLine("Gib eine Frage ein:");
        var question = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(question))
        {
            await AskAsync(dbPath, question);
        }
    }

    static List<string> ChunkText(string text, int maxLength)
    {
        var chunks = new List<string>();
        var paragraphs = text.Split(new[] { "\n\n" }, StringSplitOptions.None);
        var current = new StringBuilder();

        foreach (var para in paragraphs)
        {
            if ((current.Length + para.Length) > maxLength)
            {
                var chunk = current.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(chunk))
                    chunks.Add(chunk);
                current.Clear();
            }
            current.AppendLine(para);
        }

        var lastChunk = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(lastChunk))
            chunks.Add(lastChunk);

        return chunks;
    }

    static async Task<string> GetEmbedding(string input)
    {
        return !string.IsNullOrWhiteSpace(OpenAiApiKey)
            ? await GetEmbeddingFromOpenAi(input)
            : await GetEmbeddingFromOllama(input);
    }

    static async Task<string> GetEmbeddingFromOllama(string input)
    {
        var requestBody = new
        {
            model = "nomic-embed-text",
            prompt = input
        };

        var response = await httpClient.PostAsync(
            "http://localhost:11434/api/embeddings",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        var responseString = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseString);
        return json.RootElement.GetProperty("embedding").ToString();
    }

    static async Task<string> GetEmbeddingFromOpenAi(string input)
    {
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", OpenAiApiKey);
        var requestBody = new { input, model = EmbeddingModel };

        var response = await httpClient.PostAsync(
            "https://api.openai.com/v1/embeddings",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        var responseString = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseString);
        return json.RootElement.GetProperty("data")[0].GetProperty("embedding").ToString();
    }

    static void InitDatabase(string dbPath)
    {
        if (File.Exists(dbPath))
        {
            File.Delete(dbPath);
        }
        
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Embeddings (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SourceFile TEXT,
                Chunk TEXT,
                Embedding TEXT
            );";
        cmd.ExecuteNonQuery();
    }

    static void SaveEmbeddingToDb(string dbPath, string sourceFile, string chunk, string embedding)
    {
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();

        var insertCmd = connection.CreateCommand();
        insertCmd.CommandText = "INSERT INTO Embeddings (SourceFile, Chunk, Embedding) VALUES ($file, $chunk, $embedding)";
        insertCmd.Parameters.AddWithValue("$file", sourceFile);
        insertCmd.Parameters.AddWithValue("$chunk", chunk);
        insertCmd.Parameters.AddWithValue("$embedding", embedding);
        insertCmd.ExecuteNonQuery();
    }

    static async Task AskAsync(string dbPath, string question)
    {
        Console.WriteLine($"Frage: {question}");

        // 1. Embedding für Frage erzeugen
        var questionEmbeddingJson = await GetEmbedding(question);
        var questionEmbedding = JsonSerializer.Deserialize<List<float>>(questionEmbeddingJson);

        // 2. Alle Embeddings aus der DB laden
        var chunks = new List<(string Chunk, List<float> Embedding)>();
        using var connection = new SqliteConnection($"Data Source={dbPath}");
        connection.Open();
        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT Chunk, Embedding FROM Embeddings";
        using var reader = selectCmd.ExecuteReader();

        while (reader.Read())
        {
            var chunk = reader.GetString(0);
            var embeddingJson = reader.GetString(1);
            var embedding = JsonSerializer.Deserialize<List<float>>(embeddingJson);
            chunks.Add((chunk, embedding!));
        }

        // 3. Ähnlichste Chunks finden
        var mostRelevant = chunks
            .Select(c => new
            {
                Chunk = c.Chunk,
                Similarity = CosineSimilarity(questionEmbedding!, c.Embedding)
            })
            .OrderByDescending(c => c.Similarity)
            .Take(5) // du kannst hier die Anzahl verändern
            .ToList();

        var context = string.Join("\n---\n", mostRelevant.Select(c => c.Chunk));

        // 4. Prompt zusammenbauen
        var fullPrompt = $"""
                          Beantworte die folgende Frage basierend auf dem gegebenen Kontext.

                          Kontext:
                          {context}

                          Frage:
                          {question}

                          Antworte so präzise wie möglich.
                          """;

        // 5. Antwort generieren
        var answer = await GetAnswerFromOllama(fullPrompt);

        // 6. Ausgabe
        Console.WriteLine("\nAntwort von Ollama:");
        Console.WriteLine(answer);
    }


    static float CosineSimilarity(List<float> v1, List<float> v2)
    {
        float dot = 0f, mag1 = 0f, mag2 = 0f;
        for (int i = 0; i < v1.Count; i++)
        {
            dot += v1[i] * v2[i];
            mag1 += v1[i] * v1[i];
            mag2 += v2[i] * v2[i];
        }
        return dot / ((float)Math.Sqrt(mag1) * (float)Math.Sqrt(mag2) + 1e-5f);
    }

    static async Task<string> GetAnswerFromOllama(string prompt)
    {
        var requestBody = new
        {
            model = "llama3.2:latest",
            prompt = prompt,
            stream = false
        };

        var response = await httpClient.PostAsync(
            "http://localhost:11434/api/generate",
            new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        );

        var responseString = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(responseString);
        return json.RootElement.GetProperty("response").GetString()!;
    }
}
