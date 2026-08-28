using System.Globalization;
using System.Text;
using CucineCRM.Application.DTOs;

namespace CucineCRM.API.Export;

/// <summary>Genera CSV (UTF-8 con BOM, separatore ';' per compatibilità con Excel in locale italiana).</summary>
public static class CsvExporter
{
    public static byte[] EsportaClienti(IEnumerable<ClienteDto> clienti)
    {
        var sb = new StringBuilder();
        ScriviRiga(sb, "RagioneSociale", "CodiceCliente", "PartitaIVA", "Citta", "Provincia", "Regione", "Telefono", "Email", "Agente", "DataInserimento", "PercentualeProvvigione");

        foreach (var c in clienti)
        {
            ScriviRiga(sb,
                c.RagioneSociale, c.CodiceCliente, c.PartitaIVA, c.Citta, c.Provincia, c.Regione,
                c.Telefono, c.Email, c.AgenteNomeCompleto, c.DataInserimento.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                c.PercentualeProvvigione.ToString(CultureInfo.InvariantCulture));
        }

        return new UTF8Encoding(true).GetBytes(sb.ToString());
    }

    public static byte[] EsportaOrdini(IEnumerable<OrdineDto> ordini)
    {
        var sb = new StringBuilder();
        ScriviRiga(sb, "DataOrdine", "Cliente", "Importo", "NumeroCucine", "NumeroElettrodomestici", "NumeroComplementi", "Stato", "RiferimentoEsterno");

        foreach (var o in ordini)
        {
            ScriviRiga(sb,
                o.DataOrdine.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), o.ClienteRagioneSociale,
                o.Importo.ToString(CultureInfo.InvariantCulture), o.NumeroCucine.ToString(CultureInfo.InvariantCulture),
                o.NumeroElettrodomestici.ToString(CultureInfo.InvariantCulture), o.NumeroComplementi.ToString(CultureInfo.InvariantCulture),
                o.StatoOrdine.ToString(), o.RiferimentoEsterno);
        }

        return new UTF8Encoding(true).GetBytes(sb.ToString());
    }

    private static void ScriviRiga(StringBuilder sb, params string?[] campi)
    {
        sb.AppendLine(string.Join(';', campi.Select(Escape)));
    }

    private static string Escape(string? campo)
    {
        if (string.IsNullOrEmpty(campo))
            return string.Empty;

        // Se il campo contiene il separatore, virgolette o newline va racchiuso tra virgolette,
        // raddoppiando eventuali virgolette interne (RFC 4180).
        if (campo.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0)
            return $"\"{campo.Replace("\"", "\"\"")}\"";

        return campo;
    }
}
