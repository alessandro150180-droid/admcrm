using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;
using CucineCRM.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace CucineCRM.Infrastructure.ChatAi;

/// <summary>
/// Unico punto di contatto con l'API Anthropic: traduce tra i tipi generici usati
/// dall'Application layer (AnthropicStrumento, tuple (Ruolo, Testo)) e i tipi dell'SDK ufficiale,
/// e porta avanti da solo il loop agentico (richiesta -> eventuale tool_use -> esecuzione -> nuova
/// richiesta) finché il modello non produce una risposta testuale finale.
/// </summary>
public class AnthropicChatClient : IChatAiProvider
{
    // Limite di sicurezza contro un modello che continuasse a richiedere strumenti all'infinito:
    // le domande gestite da questo assistente si risolvono tipicamente in 1-3 chiamate.
    private const int MassimeIterazioni = 6;
    private const string Modello = "claude-opus-5";

    private readonly AnthropicClient _client;

    public AnthropicChatClient(IOptions<AnthropicOptions> options)
    {
        _client = new AnthropicClient { ApiKey = options.Value.ApiKey };
    }

    public async Task<string> ChiediAsync(
        string promptSistema,
        IReadOnlyList<(string Ruolo, string Testo)> cronologia,
        IReadOnlyList<AnthropicStrumento> strumenti,
        EsecutoreStrumentoAnthropic esecutoreStrumento,
        CancellationToken ct = default)
    {
        var messaggi = cronologia
            .Select(m => new MessageParam { Role = m.Ruolo == "assistant" ? Role.Assistant : Role.User, Content = m.Testo })
            .ToList();

        var tools = strumenti
            .Select(s => new ToolUnion(new Tool
            {
                Name = s.Nome,
                Description = s.Descrizione,
                InputSchema = new InputSchema { Properties = s.Proprieta.ToDictionary(p => p.Key, p => p.Value), Required = s.ProprietaRichieste.ToList() },
            }))
            .ToList();

        for (var iterazione = 0; iterazione < MassimeIterazioni; iterazione++)
        {
            var response = await _client.Messages.Create(new MessageCreateParams
            {
                Model = Modello,
                MaxTokens = 2048,
                System = promptSistema,
                OutputConfig = new OutputConfig { Effort = Effort.Medium },
                Tools = tools,
                Messages = messaggi,
            }, ct);

            if (response.StopReason != "tool_use")
            {
                return string.Join("\n", response.Content
                    .Select(b => b.Value)
                    .OfType<TextBlock>()
                    .Select(t => t.Text));
            }

            var contenutoAssistente = new List<ContentBlockParam>();
            var risultatiStrumenti = new List<ContentBlockParam>();

            foreach (var block in response.Content)
            {
                if (block.TryPickText(out TextBlock? testo))
                {
                    contenutoAssistente.Add(new TextBlockParam { Text = testo.Text });
                }
                else if (block.TryPickThinking(out ThinkingBlock? thinking))
                {
                    contenutoAssistente.Add(new ThinkingBlockParam { Thinking = thinking.Thinking, Signature = thinking.Signature });
                }
                else if (block.TryPickToolUse(out ToolUseBlock? toolUse))
                {
                    contenutoAssistente.Add(new ToolUseBlockParam { ID = toolUse.ID, Name = toolUse.Name, Input = toolUse.Input });

                    var inputJson = JsonSerializer.SerializeToElement(toolUse.Input);
                    string esito;
                    var isErrore = false;
                    try
                    {
                        esito = await esecutoreStrumento(toolUse.Name, inputJson, ct);
                    }
                    catch (Exception ex)
                    {
                        esito = $"Errore nell'esecuzione dello strumento: {ex.Message}";
                        isErrore = true;
                    }

                    risultatiStrumenti.Add(new ToolResultBlockParam { ToolUseID = toolUse.ID, Content = esito, IsError = isErrore });
                }
            }

            messaggi.Add(new MessageParam { Role = Role.Assistant, Content = contenutoAssistente });
            messaggi.Add(new MessageParam { Role = Role.User, Content = risultatiStrumenti });
        }

        return "Non sono riuscito a completare la richiesta in un numero ragionevole di passaggi: prova a riformularla in modo più specifico.";
    }
}
