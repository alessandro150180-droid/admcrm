using System.Text.Json;
using CucineCRM.Application.DTOs;
using CucineCRM.Application.Interfaces;
using CucineCRM.Domain.Enums;

namespace CucineCRM.Application.Services;

/// <summary>
/// Assistente conversazionale sui dati del CRM: espone al modello alcuni "strumenti" che
/// interrogano i servizi applicativi già esistenti (Dashboard, Clienti, Agenti). Non duplica
/// mai la logica di visibilità per ruolo: ogni strumento passa da IDataScopingService o da
/// IDashboardService, esattamente come farebbe un controller REST.
/// Stateless: la cronologia della conversazione vive nel frontend e viaggia per intero ad ogni
/// richiesta, non c'è nessuna Entity/tabella dedicata a questo scopo.
/// </summary>
public class ChatAiService : IChatAiService
{
    private readonly IChatAiProvider _chatAiProvider;
    private readonly IDashboardService _dashboardService;
    private readonly IDataScopingService _scoping;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncQueryExecutor _queryExecutor;
    private readonly ICurrentUserService _currentUser;

    public ChatAiService(
        IChatAiProvider chatAiProvider, IDashboardService dashboardService, IDataScopingService scoping,
        IUnitOfWork unitOfWork, IAsyncQueryExecutor queryExecutor, ICurrentUserService currentUser)
    {
        _chatAiProvider = chatAiProvider;
        _dashboardService = dashboardService;
        _scoping = scoping;
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
        _currentUser = currentUser;
    }

    public async Task<ChatAiRispostaDto> ChiediAsync(ChatAiRichiestaDto richiesta, CancellationToken ct = default)
    {
        var cronologia = richiesta.Cronologia
            .Select(m => (Ruolo: m.Ruolo == "assistente" ? "assistant" : "user", m.Testo))
            .Append((Ruolo: "user", Testo: richiesta.Testo))
            .ToList();

        var promptSistema =
            $"""
            Sei l'assistente AI del CRM "Agenti GT Nasca", una rete vendita commerciale. Rispondi in
            italiano, in modo conciso e concreto, sempre basandoti sui dati reali restituiti dagli
            strumenti a tua disposizione: non inventare mai numeri. Se una domanda richiede dati che
            non hai (es. mesi/anno non specificati), assumi ragionevolmente il mese e l'anno correnti
            oppure chiedi chiarimenti solo se davvero indispensabile.

            Data odierna: {DateTime.Now:yyyy-MM-dd}.
            Ruolo dell'utente che ti sta scrivendo: {_currentUser.Ruolo}.

            Usa lo strumento cerca_cliente o lista_agenti quando l'utente nomina un cliente o un
            agente per trovarne l'id prima di interrogare i dati di fatturato/provvigioni.
            """;

        return new ChatAiRispostaDto(
            await _chatAiProvider.ChiediAsync(promptSistema, cronologia, Strumenti, EseguiStrumentoAsync, ct));
    }

    private static readonly IReadOnlyList<AnthropicStrumento> Strumenti = new List<AnthropicStrumento>
    {
        new(
            "cerca_cliente",
            "Cerca clienti per ragione sociale o codice cliente (ricerca parziale, case-insensitive). " +
            "Restituisce al massimo 10 risultati con il loro id, da usare poi negli altri strumenti.",
            Schema(("testo", Prop("string", "Testo da cercare nella ragione sociale o nel codice cliente."))),
            new[] { "testo" }),

        new(
            "lista_agenti",
            "Restituisce l'elenco degli agenti visibili all'utente corrente (id, nome, cognome, zona). " +
            "Usalo per risalire all'id di un agente nominato per nome/cognome.",
            Schema(),
            Array.Empty<string>()),

        new(
            "kpi_dashboard",
            "Restituisce i 4 KPI principali (fatturato, nuovi clienti, ordine medio, cucine vendute) " +
            "per i mesi/anno indicati, con confronto vs stesso periodo dell'anno precedente. " +
            "Filtra opzionalmente per agente e/o cliente.",
            Schema(
                ("mesi", PropArrayInt("Elenco dei mesi da 1 a 12 da sommare (es. [6,7,8]).")),
                ("anno", Prop("integer", "Anno di riferimento, es. 2026.")),
                ("agenteId", Prop("integer", "Id dell'agente su cui filtrare (opzionale).")),
                ("clienteId", Prop("integer", "Id del cliente su cui filtrare (opzionale)."))),
            new[] { "mesi", "anno" }),

        new(
            "fatturato_mensile",
            "Restituisce la serie mensile del fatturato per l'anno indicato e i due precedenti " +
            "(confronto anno su anno). Filtra opzionalmente per agente e/o cliente.",
            Schema(
                ("anno", Prop("integer", "Anno di riferimento, es. 2026.")),
                ("agenteId", Prop("integer", "Id dell'agente su cui filtrare (opzionale).")),
                ("clienteId", Prop("integer", "Id del cliente su cui filtrare (opzionale)."))),
            new[] { "anno" }),

        new(
            "provvigioni_per_cliente",
            "Restituisce fatturato e provvigione per ciascun cliente del portafoglio (o di un singolo " +
            "cliente se clienteId è indicato) nei mesi/anno indicati.",
            Schema(
                ("mesi", PropArrayInt("Elenco dei mesi da 1 a 12 da sommare (es. [6,7,8]).")),
                ("anno", Prop("integer", "Anno di riferimento, es. 2026.")),
                ("agenteId", Prop("integer", "Id dell'agente su cui filtrare (opzionale).")),
                ("clienteId", Prop("integer", "Id del cliente su cui filtrare (opzionale)."))),
            new[] { "mesi", "anno" }),
    };

    private async Task<string> EseguiStrumentoAsync(string nome, JsonElement input, CancellationToken ct)
    {
        switch (nome)
        {
            case "cerca_cliente":
                return await CercaClienteAsync(input.GetProperty("testo").GetString() ?? string.Empty, ct);

            case "lista_agenti":
                return await ListaAgentiAsync(ct);

            case "kpi_dashboard":
                return JsonSerializer.Serialize(await _dashboardService.GetKpiPrincipaliAsync(
                    LeggiArrayInt(input, "mesi"), LeggiInt(input, "anno"),
                    LeggiIntOpzionale(input, "agenteId"), LeggiIntOpzionale(input, "clienteId"), ct: ct));

            case "fatturato_mensile":
                return JsonSerializer.Serialize(await _dashboardService.GetFatturatoMensileAsync(
                    LeggiInt(input, "anno"), LeggiIntOpzionale(input, "agenteId"), LeggiIntOpzionale(input, "clienteId"), ct: ct));

            case "provvigioni_per_cliente":
                return JsonSerializer.Serialize(await _dashboardService.GetProvvigioniPerClienteAsync(
                    LeggiArrayInt(input, "mesi"), LeggiInt(input, "anno"),
                    LeggiIntOpzionale(input, "agenteId"), LeggiIntOpzionale(input, "clienteId"), ct: ct));

            default:
                throw new InvalidOperationException($"Strumento sconosciuto: {nome}");
        }
    }

    private async Task<string> CercaClienteAsync(string testo, CancellationToken ct)
    {
        var agentiVisibili = await _scoping.GetAgentiVisibiliAsync(ct);
        var testoLower = testo.ToLower();

        var query = _unitOfWork.Clienti.Query();
        if (agentiVisibili is not null)
            query = query.Where(c => agentiVisibili.Contains(c.AgenteId));

        query = query.Where(c => c.RagioneSociale.ToLower().Contains(testoLower) || c.CodiceCliente.ToLower().Contains(testoLower));

        var risultati = await _queryExecutor.ToListAsync(query
            .OrderBy(c => c.RagioneSociale)
            .Take(10)
            .Select(c => new { c.Id, c.RagioneSociale, c.CodiceCliente, c.Citta, c.PercentualeProvvigione }), ct);

        return JsonSerializer.Serialize(risultati);
    }

    private async Task<string> ListaAgentiAsync(CancellationToken ct)
    {
        var agentiVisibili = await _scoping.GetAgentiVisibiliAsync(ct);

        var query = _unitOfWork.Agenti.Query();
        if (agentiVisibili is not null)
            query = query.Where(a => agentiVisibili.Contains(a.Id));

        var risultati = await _queryExecutor.ToListAsync(query
            .OrderBy(a => a.Cognome)
            .Select(a => new { a.Id, a.Nome, a.Cognome, a.Zona }), ct);

        return JsonSerializer.Serialize(risultati);
    }

    private static int LeggiInt(JsonElement input, string proprieta) => input.GetProperty(proprieta).GetInt32();

    private static int? LeggiIntOpzionale(JsonElement input, string proprieta) =>
        input.TryGetProperty(proprieta, out var valore) && valore.ValueKind is JsonValueKind.Number ? valore.GetInt32() : null;

    private static IReadOnlyList<int> LeggiArrayInt(JsonElement input, string proprieta) =>
        input.GetProperty(proprieta).EnumerateArray().Select(e => e.GetInt32()).ToList();

    private static JsonElement Prop(string tipo, string descrizione) =>
        JsonSerializer.SerializeToElement(new { type = tipo, description = descrizione });

    private static JsonElement PropArrayInt(string descrizione) =>
        JsonSerializer.SerializeToElement(new { type = "array", items = new { type = "integer" }, description = descrizione });

    private static IReadOnlyDictionary<string, JsonElement> Schema(params (string Nome, JsonElement Proprieta)[] proprieta) =>
        proprieta.ToDictionary(p => p.Nome, p => p.Proprieta);
}
