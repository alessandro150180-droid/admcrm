using System.Globalization;
using ClosedXML.Excel;
using CucineCRM.Application.Interfaces;

namespace CucineCRM.Infrastructure.Import;

public class ClosedXmlSpreadsheetReader : ISpreadsheetReader
{
    public IReadOnlyList<IReadOnlyDictionary<string, string>> LeggiRighe(Stream file)
    {
        using var workbook = new XLWorkbook(file);
        var foglio = workbook.Worksheets.First();
        var rangeUsato = foglio.RangeUsed();
        if (rangeUsato is null)
            return Array.Empty<IReadOnlyDictionary<string, string>>();

        var righe = rangeUsato.RowsUsed().ToList();
        if (righe.Count < 2) // solo intestazione o foglio vuoto: nessuna riga dati
            return Array.Empty<IReadOnlyDictionary<string, string>>();

        var intestazioni = righe[0].Cells().Select(c => c.GetString().Trim()).ToList();

        var risultato = new List<IReadOnlyDictionary<string, string>>();
        foreach (var riga in righe.Skip(1))
        {
            var valori = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < intestazioni.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(intestazioni[i]))
                    continue;

                valori[intestazioni[i]] = CellaATesto(riga.Cell(i + 1));
            }
            risultato.Add(valori);
        }
        return risultato;
    }

    private static string CellaATesto(IXLCell cella)
    {
        if (cella.IsEmpty())
            return string.Empty;

        // Le date Excel sono numeri seriali: senza normalizzarle qui in ISO, il parsing a valle
        // (DateTime.Parse sul testo) dipenderebbe dal formato di visualizzazione della cella.
        if (cella.DataType == XLDataType.DateTime)
            return cella.GetDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        if (cella.DataType == XLDataType.Number)
            return cella.GetDouble().ToString(CultureInfo.InvariantCulture);

        return cella.GetString().Trim();
    }
}
