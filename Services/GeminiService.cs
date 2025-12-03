using System.Text;
using System.Text.Json;
using Api.Dtos;
using Api.Models;

namespace Api.Services
{
    public interface IGeminiService
    {
        Task<StudyPlanResponse> GetStudyRecommendationAsync(User student, string specificGoal);
        Task<string> GetProgressAnalysisAsync(StudentProgressAnalysisRequest data);
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
                Contents =
                [
                    new Content
                    {
                        Parts =
                        [
                            new Part { Text = prompt }
                        ]
                    }
                ]
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
            string userLocation = "Não informada";
            if (!string.IsNullOrEmpty(student.City))
            {
                userLocation = $"{student.City} - {student.State}";
            }

            return $@"
                Atue como um mentor acadêmico especialista chamado EduMentor.
                Analise o seguinte perfil e objetivo do aluno:
                
                Nome: {student.Name}
                Localização do Perfil: {userLocation}
                Área de Interesse: {student.AreaInteresse ?? "Geral"}
                Bio: {student.Bio ?? "Não informado"}
                
                Objetivo do Aluno: ""{specificGoal}""

                REGRAS DE VALIDAÇÃO (CRÍTICO):
                1. Analise o texto em ""Objetivo do Aluno"".
                2. Se o texto for aleatório, ofensivo ou sem sentido acadêmico, RECUSE conforme regra abaixo.
                3. Se recusado: retorne JSON com ""planTitle"": ""Objetivo Inválido"" e ""activities"": [].
                4. Se válido: gere o plano.

                Responda ESTRITAMENTE com o seguinte formato JSON (sem markdown):
                {{
                    ""planTitle"": ""Um título curto"",
                    ""motivation"": ""Texto motivacional curto citando a cidade do aluno se possível."",
                    
                    ""suggestedCourse"": ""Nome do curso superior exato (Ex: 'Sistemas de Informação')."",
                    
                    ""suggestedLocation"": ""Regra de Prioridade: 1. Se o aluno escreveu uma cidade no 'Objetivo', use ela. 2. Se NÃO escreveu, use a 'Localização do Perfil' fornecida acima. 3. Se ambos vazios, retorne null."",
                    
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

        public async Task<string> GetProgressAnalysisAsync(StudentProgressAnalysisRequest data)
        {
            var tracksContext = string.Join("; ", data.TrackSummaries);

            var prompt = $@"
                Atue como um Mentor de Alta Performance e Analista de Dados Educacionais chamado EduMentor.
                
                DADOS DO ALUNO PARA ANÁLISE:
                - Nome: {data.StudentName}
                - Taxa Global de Conclusão: {data.GlobalCompletionRate:F1}%
                - Total de Trilhas: {data.TotalTracks} (Concluídas: {data.CompletedTracks})
                - Sessões de Mentoria Realizadas: {data.TotalMentoringSessions}
                - Status Detalhado das Trilhas: [{tracksContext}]
                
                MÉTRICAS DE TEMPO E CONSISTÊNCIA:
                - Média de dias entre tarefas concluídas: {data.AverageDaysBetweenTasks:F1} dias
                - Maior tempo parado (Gap): {data.MaxGapInDays} dias
                - Data da última atividade: {data.LastActivityDate?.ToString("dd/MM/yyyy") ?? "Nenhuma registrada"}

                OBJETIVO DA ANÁLISE:
                Forneça um feedback estratégico, profundo e baseado em dados. Identifique padrões de comportamento e consistência.

                DIRETRIZES DE RESPOSTA:
                1.  **Analise a Consistência:** Se a 'Média de dias' for baixa (< 2), elogie a disciplina. Se o 'Maior tempo parado' for alto (> 7 dias), pergunte amigavelmente se houve algum obstáculo e sugira estratégias para evitar pausas longas.
                2.  **Identifique o Padrão:** O aluno é ""focado"" (termina o que começa), ""disperso"" (inicia várias trilhas e deixa em 10-20%) ou ""inconstante"" (faz muito num dia e para por semanas)? Diga isso claramente.
                3.  **Valorize a Mentoria:** Se ele tem poucas sessões e progresso baixo/inconstante, sugira agendar uma mentoria urgentemente para desbloqueio.
                4.  **Tom de Voz:** Profissional, analítico, direto, mas acolhedor. Evite frases clichês vazias. Use os dados de tempo para embasar seu conselho.
                5.  **Formato:** Um parágrafo coeso de 3 a 5 frases (aprox. 80 palavras). Fale diretamente com o aluno (""Olá, [Nome]..."").

                Gere a análise agora.
            ";

            var requestBody = new GeminiRequest
            {
                Contents = [new Content { Parts = [new Part { Text = prompt }] }]
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}?key={_apiKey}", jsonContent);

            if (!response.IsSuccessStatusCode) return "Análise indisponível no momento.";

            var responseString = await response.Content.ReadAsStringAsync();
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseString);

            return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "Continue seus estudos.";
        }
    }
}