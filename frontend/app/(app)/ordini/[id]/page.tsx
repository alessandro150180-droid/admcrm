"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { api } from "@/lib/api";
import type { OrdineDto, StatoOrdine } from "@/lib/types";
import { Card, ErrorBlock, LoadingBlock, PageHeader, Select } from "@/components/ui";
import { formattaData, formattaValuta, messaggioErrore, ETICHETTE_STATO_ORDINE } from "@/lib/format";
import { StatoOrdineBadge } from "@/components/StatoBadge";
import { useAuth, puoModificare } from "@/lib/auth-context";

const STATI: StatoOrdine[] = ["InAttesa", "Confermato", "InProduzione", "Spedito", "Consegnato", "Annullato"];

export default function OrdineDettaglioPage() {
  const params = useParams<{ id: string }>();
  const ordineId = Number(params.id);
  const { utente } = useAuth();

  const [ordine, setOrdine] = useState<OrdineDto | null>(null);
  const [caricando, setCaricando] = useState(true);
  const [aggiornando, setAggiornando] = useState(false);
  const [errore, setErrore] = useState<string | null>(null);

  useEffect(() => {
    api.ordini.dettaglio(ordineId)
      .then(setOrdine)
      .catch((err) => setErrore(messaggioErrore(err)))
      .finally(() => setCaricando(false));
  }, [ordineId]);

  async function handleCambiaStato(nuovoStato: string) {
    setAggiornando(true);
    setErrore(null);
    try {
      const aggiornato = await api.ordini.aggiornaStato(ordineId, nuovoStato);
      setOrdine(aggiornato);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setAggiornando(false);
    }
  }

  if (caricando) return <LoadingBlock />;
  if (errore && !ordine) return <ErrorBlock message={errore} />;
  if (!ordine) return null;

  return (
    <div className="max-w-lg">
      <PageHeader title={`Ordine #${ordine.id}`} subtitle={ordine.clienteRagioneSociale} />
      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      <Card>
        <dl className="space-y-2 text-sm">
          <Riga etichetta="Data ordine" valore={formattaData(ordine.dataOrdine)} />
          <Riga etichetta="Importo" valore={formattaValuta(ordine.importo)} />
          <Riga etichetta="Cucine" valore={String(ordine.numeroCucine)} />
          <Riga etichetta="Elettrodomestici" valore={String(ordine.numeroElettrodomestici)} />
          <Riga etichetta="Complementi" valore={String(ordine.numeroComplementi)} />
          <Riga etichetta="Riferimento esterno" valore={ordine.riferimentoEsterno ?? "—"} />
        </dl>

        <div className="mt-5 flex items-center justify-between border-t border-zinc-100 pt-4">
          <div>
            <p className="text-xs text-zinc-500">Stato attuale</p>
            <div className="mt-1"><StatoOrdineBadge stato={ordine.statoOrdine} /></div>
          </div>
          {puoModificare(utente?.ruolo) && (
            <Select
              value={ordine.statoOrdine}
              disabled={aggiornando}
              onChange={(e) => handleCambiaStato(e.target.value)}
              className="w-48"
            >
              {STATI.map((s) => (
                <option key={s} value={s}>{ETICHETTE_STATO_ORDINE[s]}</option>
              ))}
            </Select>
          )}
        </div>
      </Card>
    </div>
  );
}

function Riga({ etichetta, valore }: { etichetta: string; valore: string }) {
  return (
    <div className="flex justify-between gap-4">
      <dt className="text-zinc-500">{etichetta}</dt>
      <dd className="text-right font-medium text-zinc-800">{valore}</dd>
    </div>
  );
}
