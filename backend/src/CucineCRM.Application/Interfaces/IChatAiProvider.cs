using System.Text.Json;

namespace CucineCRM.Application.Interfaces;

/// <summary>Uno strumento (function calling) che il modello può richiedere di eseguire.
/// Lo schema è espresso con tipi generici (JsonElement/Dictionary), non con i tipi dell'SDK
/// Anthropic: l'Application layer non deve dipendere dal provider AI usato in Infrastructure.</summary>
public record AnthropicStrumento(
    string Nome,
    string Descrizione,
    IReadOnlyDictionary<string, JsonElement> Proprieta,
    IReadOnlyList<string> ProprietaRichieste);

/// <summary>Eseguito da Infrastructure ogni volta che il modello richiede uno strumento: riceve
/// nome strumento + input e restituisce il risultato come testo (tipicamente JSON) da rimandare
/// al modello. Le eventuali eccezioni sono gestite dal chiamante (ChatAiService) come esito d'errore.</summary>
public delegate Task<string> EsecutoreStrumentoAnthropic(string nomeStrumento, JsonElement input, CancellationToken ct);

public interface IChatAiProvider
{
    /// <summary>
    /// Invia la conversazione (system prompt + cronologia, l'ultimo messaggio è la domanda corrente)
    /// al modello, risolvendo internamente eventuali richieste di strumenti (loop agentico) tramite
    /// <paramref name="esecutoreStrumento"/>, finché il modello non produce una risposta testuale finale.
    /// </summary>
    Task<string> ChiediAsync(
        string promptSistema,
        IReadOnlyList<(string Ruolo, string Testo)> cronologia,
        IReadOnlyList<AnthropicStrumento> strumenti,
        EsecutoreStrumentoAnthropic esecutoreStrumento,
        CancellationToken ct = default);
}
