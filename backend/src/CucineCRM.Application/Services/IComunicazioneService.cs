using CucineCRM.Application.DTOs;

namespace CucineCRM.Application.Services;

public interface IComunicazioneService
{
    /// <summary>Elenco delle comunicazioni pubblicate, più recenti per prime.</summary>
    Task<IReadOnlyList<ComunicazioneDto>> GetListaAsync(CancellationToken ct = default);

    /// <summary>Pubblica un nuovo file (circolare/PDF/Excel). Valida estensione e dimensione.</summary>
    Task<ComunicazioneDto> CreaAsync(
        Stream fileStream, string nomeFile, string tipoContenuto, string titolo, string? descrizione,
        int utenteId, CancellationToken ct = default);

    /// <summary>Contenuto binario di una comunicazione, per il download.</summary>
    Task<(byte[] Contenuto, string TipoContenuto, string NomeFile)> ScaricaAsync(int id, CancellationToken ct = default);

    Task EliminaAsync(int id, CancellationToken ct = default);
}
