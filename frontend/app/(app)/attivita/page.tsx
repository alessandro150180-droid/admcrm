"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import type { AttivitaDto, StatoAttivita } from "@/lib/types";
import {
  Button, EmptyState, ErrorBlock, LoadingBlock, PageHeader, Pagination, Select, Table, Td, Th, Tr,
} from "@/components/ui";
import { formattaData, messaggioErrore, ETICHETTE_STATO_ATTIVITA, ETICHETTE_TIPO_ATTIVITA } from "@/lib/format";
import { useAuth, puoModificare } from "@/lib/auth-context";

const STATI: StatoAttivita[] = ["DaFare", "InCorso", "Completata", "Annullata"];

export default function AttivitaPage() {
  const { utente } = useAuth();
  const modificabile = puoModificare(utente?.ruolo);
  const [dati, setDati] = useState<{ elementi: AttivitaDto[]; pagina: number; totalePagine: number; totaleElementi: number } | null>(null);
  const [pagina, setPagina] = useState(1);
  const [stato, setStato] = useState("");
  const [soloScadute, setSoloScadute] = useState(false);
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);
  const [sincronizzando, setSincronizzando] = useState<number | null>(null);

  function carica() {
    setCaricando(true);
    api.attivita
      .lista({ pagina, dimensione: 20, stato: stato || undefined, soloScadute: soloScadute || undefined })
      .then(setDati)
      .catch((err) => setErrore(messaggioErrore(err)))
      .finally(() => setCaricando(false));
  }

  useEffect(() => {
    carica();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pagina, stato, soloScadute]);

  async function handleCambiaStato(id: number, nuovoStato: string) {
    try {
      await api.attivita.aggiornaStato(id, nuovoStato);
      carica();
    } catch (err) {
      setErrore(messaggioErrore(err));
    }
  }

  async function handleSincronizza(id: number) {
    setSincronizzando(id);
    setErrore(null);
    try {
      await api.attivita.sincronizzaCalendario(id);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setSincronizzando(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Attività"
        subtitle={dati ? `${dati.totaleElementi} attività totali` : undefined}
        actions={modificabile ? <Link href="/attivita/nuovo"><Button>+ Nuova attività</Button></Link> : undefined}
      />

      <div className="mb-4 flex items-center gap-3">
        <Select value={stato} onChange={(e) => { setPagina(1); setStato(e.target.value); }} className="w-44">
          <option value="">Tutti gli stati</option>
          {STATI.map((s) => <option key={s} value={s}>{ETICHETTE_STATO_ATTIVITA[s]}</option>)}
        </Select>
        <label className="flex items-center gap-2 text-sm text-zinc-600">
          <input type="checkbox" checked={soloScadute} onChange={(e) => { setPagina(1); setSoloScadute(e.target.checked); }} />
          Solo scadute
        </label>
      </div>

      {errore && <ErrorBlock message={errore} />}

      {caricando && !dati ? (
        <LoadingBlock />
      ) : !dati || dati.elementi.length === 0 ? (
        <EmptyState message="Nessuna attività trovata con questi filtri." />
      ) : (
        <>
          <Table>
            <thead>
              <tr>
                <Th>Scadenza</Th>
                <Th>Cliente</Th>
                <Th>Tipo</Th>
                <Th>Titolo</Th>
                <Th>Responsabile</Th>
                <Th>Stato</Th>
                <Th>Calendario</Th>
              </tr>
            </thead>
            <tbody>
              {dati.elementi.map((a) => (
                <tr key={a.id}>
                  <Td>{formattaData(a.dataScadenza)}</Td>
                  <Td className="font-medium text-zinc-900">{a.clienteRagioneSociale}</Td>
                  <Td>{ETICHETTE_TIPO_ATTIVITA[a.tipoAttivita] ?? a.tipoAttivita}</Td>
                  <Td>{a.titolo}</Td>
                  <Td>{a.utenteNomeCompleto}</Td>
                  <Td>
                    {modificabile ? (
                      <Select value={a.stato} onChange={(e) => handleCambiaStato(a.id, e.target.value)} className="w-36 py-1">
                        {STATI.map((s) => <option key={s} value={s}>{ETICHETTE_STATO_ATTIVITA[s]}</option>)}
                      </Select>
                    ) : (
                      ETICHETTE_STATO_ATTIVITA[a.stato]
                    )}
                  </Td>
                  <Td>
                    {modificabile && (
                      <Button variant="ghost" onClick={() => handleSincronizza(a.id)} disabled={sincronizzando === a.id}>
                        {sincronizzando === a.id ? "…" : "📅 Sincronizza"}
                      </Button>
                    )}
                  </Td>
                </tr>
              ))}
            </tbody>
          </Table>
          <Pagination pagina={dati.pagina} totalePagine={dati.totalePagine} onCambiaPagina={setPagina} />
        </>
      )}
    </div>
  );
}
