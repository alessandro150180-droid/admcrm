using CucineCRM.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CucineCRM.API.Export;

public static class PdfExporter
{
    public static byte[] EsportaSchedaCliente(ClienteDettaglioDto dettaglio, IReadOnlyList<NotaClienteDto> note)
    {
        var anagrafica = dettaglio.Anagrafica;

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text($"Scheda Cliente — {anagrafica.RagioneSociale}").FontSize(18).Bold();
                    col.Item().Text($"Generata il {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Spacing(12);

                    col.Item().Text("Anagrafica").FontSize(14).Bold();
                    col.Item().Column(anagraficaCol =>
                    {
                        // Stesso ordine di campi della scheda a video, così il PDF è confrontabile
                        // riga per riga con quello che l'agente vede nel CRM.
                        anagraficaCol.Item().Text($"Agente: {anagrafica.AgenteNomeCompleto}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.AgenteEmail))
                            anagraficaCol.Item().Text($"E-mail agente: {anagrafica.AgenteEmail}");
                        anagraficaCol.Item().Text($"Codice cliente: {anagrafica.CodiceCliente}");
                        anagraficaCol.Item().Text($"Ragione sociale: {anagrafica.RagioneSociale}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Indirizzo))
                            anagraficaCol.Item().Text($"Indirizzo: {anagrafica.Indirizzo}{(string.IsNullOrWhiteSpace(anagrafica.CAP) ? "" : $" — {anagrafica.CAP}")}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Citta))
                            anagraficaCol.Item().Text($"Città: {anagrafica.Citta}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Provincia))
                            anagraficaCol.Item().Text($"Provincia: {anagrafica.Provincia}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Regione))
                            anagraficaCol.Item().Text($"Regione: {anagrafica.Regione}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.PartitaIVA))
                            anagraficaCol.Item().Text($"Partita IVA: {anagrafica.PartitaIVA}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Email))
                            anagraficaCol.Item().Text($"E-mail cliente: {anagrafica.Email}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.NominativoTitolare))
                            anagraficaCol.Item().Text($"Nominativo titolare: {anagrafica.NominativoTitolare}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Telefono))
                            anagraficaCol.Item().Text($"Telefono: {anagrafica.Telefono}");
                        anagraficaCol.Item().Text($"Provvigione: {anagrafica.PercentualeProvvigione:0.##}%");
                        anagraficaCol.Item().Text($"Cliente dal: {anagrafica.DataInserimento:dd/MM/yyyy}");
                    });

                    col.Item().Text("KPI commerciali").FontSize(14).Bold();
                    col.Item().Column(kpiCol =>
                    {
                        kpiCol.Item().Text($"Ordini totali: {dettaglio.NumeroOrdiniTotali}");
                        kpiCol.Item().Text($"Fatturato totale: {dettaglio.FatturatoTotale:N2} €");
                        kpiCol.Item().Text($"Ordine medio: {dettaglio.OrdineMedio:N2} €");
                        kpiCol.Item().Text($"Cucine acquistate: {dettaglio.NumeroCucineAcquistate}");
                        kpiCol.Item().Text($"Elettrodomestici acquistati: {dettaglio.NumeroElettrodomesticiAcquistati}");
                        kpiCol.Item().Text(dettaglio.UltimoAcquisto is null
                            ? "Ultimo acquisto: nessuno"
                            : $"Ultimo acquisto: {dettaglio.UltimoAcquisto:dd/MM/yyyy}");
                    });

                    col.Item().Text("Note").FontSize(14).Bold();
                    if (note.Count == 0)
                    {
                        col.Item().Text("Nessuna nota registrata.").FontColor(Colors.Grey.Darken1);
                    }
                    else
                    {
                        foreach (var nota in note)
                        {
                            col.Item().Text(text =>
                            {
                                text.Span($"{nota.DataInserimento:dd/MM/yyyy} — {nota.UtenteNomeCompleto}: ").Bold();
                                text.Span(nota.Testo);
                            });
                        }
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
        });

        return documento.GeneratePdf();
    }
}
