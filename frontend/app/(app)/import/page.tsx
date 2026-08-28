"use client";

import { useState, type FormEvent } from "react";
import { api } from "@/lib/api";
import type { ImportazioneRisultatoDto, RigaImportLogDto } from "@/lib/types";
import { Button, Card, ErrorBlock, Field, Input, PageHeader, Table, Td, Th } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";
import { isSoloDirezione, useAuth } from "@/lib/auth-context";

type TipoImport = "ordini" | "clienti" | "fatturato-mensile";

const COLONNE: Record<TipoImport, string> = {
  ordini: "CodiceCliente, DataOrdine, Importo, NumeroCucine, NumeroElettrodomestici, NumeroComplementi, RiferimentoEsterno (opzionale)",
  clienti: "RagioneSociale, CodiceCliente, PartitaIVA, Indirizzo, Citta, Provincia, Regione, CAP, Telefono, Email, EmailAgente",
  "fatturato-mensile": "CodiceCliente, Provvigione (opzionale), + una colonna per ogni mese con fatturato (es. \"Aprile 2026\", \"Maggio 2026\"…)",
};

const ETICHETTE_TIPO: Record<TipoImport, string> = {
  ordini: "Ordini / fatturato",
  clienti: "Anagrafiche clienti",
  "fatturato-mensile": "Fatturato mensile (pivot)",
};

export default function ImportPage() {
  const { utente } = useAuth();
  const [tipo, setTipo] = useState<TipoImport>("ordini");
  const [file, setFile] = useState<File | null>(null);
  const [periodo, setPeriodo] = useState(new Date().toISOString().slice(0, 7));
  const [risultato, setRisultato] = useState<ImportazioneRisultatoDto | null>(null);
  const [errore, setErrore] = useState<string | null>(null);
  const [inviando, setInviando] = useState(false);

  if (!isSoloDirezione(utente?.ruolo)) {
    return <ErrorBlock message="Non hai i permessi per importare dati da Excel." />;
  }

  function cambiaTipo(nuovo: TipoImport) {
    setTipo(nuovo);
    setFile(null);
    setRisultato(null);
    setErrore(null);
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!file) {
      setErrore("Seleziona un file .xlsx.");
      return;
    }
    setErrore(null);
    setInviando(true);
    setRisultato(null);
    try {
      const esito = tipo === "ordini"
        ? await api.importazioni.importaOrdini(file, periodo)
        : tipo === "clienti"
          ? await api.importazioni.importaClienti(file, periodo)
          : await api.importazioni.importaFatturatoMensile(file);
      setRisultato(esito);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  const log: RigaImportLogDto[] = risultato?.logEsito ? JSON.parse(risultato.logEsito) : [];

  return (
    <div className="max-w-3xl">
      <PageHeader title="Import Excel" subtitle="Importazione massiva di anagrafiche clienti e ordini/fatturato da file .xlsx" />

      <div className="mb-4 flex gap-2">
        {(Object.keys(ETICHETTE_TIPO) as TipoImport[]).map((t) => (
          <Button key={t} variant={tipo === t ? "primary" : "secondary"} onClick={() => cambiaTipo(t)}>
            {ETICHETTE_TIPO[t]}
          </Button>
        ))}
      </div>

      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      <Card className="mb-6">
        <p className="mb-3 text-xs text-zinc-500">Colonne attese (prima riga): {COLONNE[tipo]}</p>
        <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-4">
          <Field label="File Excel (.xlsx)">
            <input
              type="file" accept=".xlsx"
              onChange={(e) => setFile(e.target.files?.[0] ?? null)}
              className="text-sm"
            />
          </Field>
          {tipo !== "fatturato-mensile" && (
            <Field label="Periodo di competenza">
              <Input type="month" value={periodo} onChange={(e) => setPeriodo(e.target.value)} className="w-40" />
            </Field>
          )}
          <Button type="submit" disabled={inviando}>{inviando ? "Importazione in corso…" : "Importa"}</Button>
        </form>
      </Card>

      {risultato && (
        <Card>
          <p className="mb-3 text-sm font-medium text-zinc-700">Esito importazione — {risultato.nomeFile}</p>
          <div className="mb-4 grid grid-cols-4 gap-4 text-center text-sm">
            <Riepilogo etichetta="Righe totali" valore={risultato.righePlesse} />
            <Riepilogo etichetta="Importate" valore={risultato.righeImportate} tono="text-emerald-700" />
            <Riepilogo
              etichetta={tipo === "clienti" ? "Aggiornate" : tipo === "fatturato-mensile" ? "Mesi già presenti" : "Duplicate"}
              valore={risultato.righeDuplicate}
              tono="text-amber-700"
            />
            <Riepilogo etichetta="Scartate" valore={risultato.righeScartate} tono="text-red-700" />
          </div>

          {log.length > 0 && (
            <Table>
              <thead>
                <tr>
                  <Th>Riga</Th>
                  <Th>Esito</Th>
                  <Th>Dettaglio</Th>
                </tr>
              </thead>
              <tbody>
                {log.map((r) => (
                  <tr key={r.numeroRiga}>
                    <Td>{r.numeroRiga}</Td>
                    <Td>{r.esito}</Td>
                    <Td>{r.messaggio ?? "—"}</Td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </Card>
      )}
    </div>
  );
}

function Riepilogo({ etichetta, valore, tono = "text-zinc-800" }: { etichetta: string; valore: number; tono?: string }) {
  return (
    <div className="rounded-md bg-zinc-50 py-3">
      <p className={`text-xl font-semibold ${tono}`}>{valore}</p>
      <p className="text-xs text-zinc-500">{etichetta}</p>
    </div>
  );
}
