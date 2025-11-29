using System.Text;
using System.Text.Json;
using Api.Dtos;
using Api.Models;

namespace Api.Services
{
    public interface IGeminiService
    {
        Task<StudyPlanResponse> GetStudyRecommendationAsync(User student, string specificGoal);
    }

    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GeminiSettings:ApiKey"];
        }

        public async Task<StudyPlanResponse> GetStudyRecommendationAsync(User student, string specificGoal)
        {
            var prompt = GeneratePrompt(student, specificGoal);

            var requestBody = new GeminiRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Parts = new List<Part>
                        {
                            new Part { Text = prompt }
                        }
                    }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_baseUrl}?key={_apiKey}", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Erro na API do Gemini ({response.StatusCode}): {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseString);

            var textResult = geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            if (string.IsNullOrEmpty(textResult))
                return new StudyPlanResponse();

            var cleanedJson = textResult
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try 
            {
                return JsonSerializer.Deserialize<StudyPlanResponse>(cleanedJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new StudyPlanResponse();
            }
            catch
            {
                return new StudyPlanResponse 
                { 
                    PlanTitle = "Erro de Leitura", 
                    Motivation = "A IA respondeu, mas não no formato JSON esperado. Tente novamente.",
                    SuggestedCourse = "Administração" 
                };
            }
        }

        private string GeneratePrompt(User student, string specificGoal)
        {
            return $@"
                Atue como um mentor acadêmico especialista chamado EduMentor.
                Analise o seguinte perfil e objetivo do aluno:
                
                Nome: {student.Name}
                Área de Interesse: {student.AreaInteresse ?? "Geral"}
                Bio: {student.Bio ?? "Não informado"}
                Objetivo do Aluno: ""{specificGoal}""

                Sua tarefa é gerar um plano de estudos e EXTRAIR informações chave do texto do aluno.

                Responda ESTRITAMENTE com o seguinte formato JSON (sem markdown):
                {{
                    ""planTitle"": ""Um título curto"",
                    ""motivation"": ""Texto motivacional curto"",
                    ""suggestedCourse"": ""Um nome de curso superior exato e comum no Brasil (Ex: 'Administração', 'Direito', 'Sistemas de Informação') relacionado ao objetivo."",
                    ""suggestedLocation"": ""Analise o texto do 'Objetivo do Aluno'. Se ele mencionou alguma Cidade ou Estado onde quer estudar (Ex: 'em Palmas', 'no Tocantins', 'em SP'), extraia APENAS o nome do local e coloque aqui. Se ele não mencionou local, deixe null."",
                    ""activities"": [
                        {{
                            ""title"": ""Nome da atividade"",
                            ""description"": ""O que fazer"",
                            ""link"": ""Url ou null""
                        }}
                    ]
                }}
            ";
        }
    }
}