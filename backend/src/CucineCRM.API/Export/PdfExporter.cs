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
                        anagraficaCol.Item().Text($"Codice cliente: {anagrafica.CodiceCliente}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.PartitaIVA))
                            anagraficaCol.Item().Text($"Partita IVA: {anagrafica.PartitaIVA}");
                        anagraficaCol.Item().Text($"Indirizzo: {anagrafica.Indirizzo} {anagrafica.CAP} {anagrafica.Citta} ({anagrafica.Provincia}) — {anagrafica.Regione}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Telefono))
                            anagraficaCol.Item().Text($"Telefono: {anagrafica.Telefono}");
                        if (!string.IsNullOrWhiteSpace(anagrafica.Email))
                            anagraficaCol.Item().Text($"Email: {anagrafica.Email}");
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
