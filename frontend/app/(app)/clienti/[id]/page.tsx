"use client";

import { useEffect, useState, type FormEvent } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import { api, scaricaBlob } from "@/lib/api";
import type { ClienteDettaglioDto, ClienteDto, NotaClienteDto } from "@/lib/types";
import { Button, Card, ErrorBlock, Input, LoadingBlock, PageHeader } from "@/components/ui";
import { formattaData, formattaDataOra, formattaPercentuale, formattaValuta, messaggioErrore } from "@/lib/format";
import { isDirezioneOAreaManager, puoModificare, useAuth } from "@/lib/auth-context";

export default function ClienteDettaglioPage() {
  const params = useParams<{ id: string }>();
  const clienteId = Number(params.id);
  const { utente } = useAuth();

  const [dettaglio, setDettaglio] = useState<ClienteDettaglioDto | null>(null);
  const [note, setNote] = useState<NotaClienteDto[]>([]);
  const [nuovaNota, setNuovaNota] = useState("");
  const [inviandoNota, setInviandoNota] = useState(false);
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);

  const [provvigioneModifica, setProvvigioneModifica] = useState("");
  const [salvandoProvvigione, setSalvandoProvvigione] = useState(false);

  async function ricarica() {
    setCaricando(true);
    setErrore(null);
    try {
      const [d, n] = await Promise.all([api.clienti.dettaglio(clienteId), api.clienti.note(clienteId)]);
      setDettaglio(d);
      setNote(n);
      setProvvigioneModifica(String(d.anagrafica.percentualeProvvigione));
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setCaricando(false);
    }
  }

  useEffect(() => {
    if (Number.isFinite(clienteId)) ricarica();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [clienteId]);

  async function handleAggiungiNota(e: FormEvent) {
    e.preventDefault();
    if (!nuovaNota.trim()) return;
    setInviandoNota(true);
    try {
      const nota = await api.clienti.aggiungiNota(clienteId, nuovaNota.trim());
      setNote((prev) => [nota, ...prev]);
      setNuovaNota("");
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviandoNota(false);
    }
  }

  async function handleSalvaProvvigione() {
    const valore = Number(provvigioneModifica);
    if (Number.isNaN(valore) || valore < 0 || valore > 100) {
      setErrore("La percentuale di provvigione deve essere un numero tra 0 e 100.");
      return;
    }
    setSalvandoProvvigione(true);
    setErrore(null);
    try {
      const aggiornato = await api.clienti.impostaProvvigione(clienteId, valore);
      setDettaglio((prev) => (prev ? { ...prev, anagrafica: aggiornato } : prev));
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setSalvandoProvvigione(false);
    }
  }

  async function handleEsportaPdf() {
    try {
      const blob = await api.clienti.esportaPdf(clienteId);
      scaricaBlob(blob, `scheda-cliente-${dettaglio?.anagrafica.codiceCliente ?? clienteId}.pdf`);
    } catch (err) {
      setErrore(messaggioErrore(err));
    }
  }

  if (caricando) return <LoadingBlock />;
  if (errore && !dettaglio) return <ErrorBlock message={errore} />;
  if (!dettaglio) return null;

  const { anagrafica } = dettaglio;
  const modificabile = puoModificare(utente?.ruolo);

  return (
    <div className="max-w-4xl">
      <PageHeader
        title={anagrafica.ragioneSociale}
        subtitle={`Codice ${anagrafica.codiceCliente} — Agente: ${anagrafica.agenteNomeCompleto}`}
        actions={
          <>
            {modificabile && (
              <>
                <Link href={`/ordini/nuovo?clienteId=${clienteId}`}><Button variant="secondary">+ Ordine</Button></Link>
                <Link href={`/attivita/nuovo?clienteId=${clienteId}`}><Button variant="secondary">+ Attività</Button></Link>
              </>
            )}
            <Button onClick={handleEsportaPdf}>Esporta PDF</Button>
          </>
        }
      />

      {errore && <ErrorBlock message={errore} />}

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Card>
          <p className="mb-3 text-sm font-medium text-zinc-700">Anagrafica</p>
          <dl className="space-y-1.5 text-sm">
            <Riga etichetta="Agente" valore={anagrafica.agenteNomeCompleto} />
            <Riga etichetta="E-mail agente" valore={anagrafica.agenteEmail || "—"} />
            <Riga etichetta="Codice cliente" valore={anagrafica.codiceCliente} />
            <Riga etichetta="Ragione sociale" valore={anagrafica.ragioneSociale} />
            <Riga etichetta="Indirizzo" valore={formattaIndirizzo(anagrafica)} />
            <Riga etichetta="Città" valore={anagrafica.citta ?? "—"} />
            <Riga etichetta="Provincia" valore={anagrafica.provincia ?? "—"} />
            <Riga etichetta="Regione" valore={anagrafica.regione ?? "—"} />
            <Riga etichetta="Partita IVA" valore={anagrafica.partitaIVA ?? "—"} />
            <Riga etichetta="E-mail cliente" valore={anagrafica.email ?? "—"} />
            <Riga etichetta="Nominativo titolare" valore={anagrafica.nominativoTitolare ?? "—"} />
            <Riga etichetta="Telefono" valore={anagrafica.telefono ?? "—"} />
            <Riga etichetta="Provvigione" valore={formattaPercentuale(anagrafica.percentualeProvvigione)} />
            <Riga etichetta="Cliente dal" valore={formattaData(anagrafica.dataInserimento)} />
          </dl>

          {isDirezioneOAreaManager(utente?.ruolo) && (
            <div className="mt-3 flex items-end gap-2 border-t border-zinc-100 pt-3">
              <div className="flex-1">
                <label className="mb-1 block text-xs font-medium text-zinc-500">Modifica provvigione (%)</label>
                <Input
                  type="number" min="0" max="100" step="0.5"
                  value={provvigioneModifica}
                  onChange={(e) => setProvvigioneModifica(e.target.value)}
                />
              </div>
              <Button variant="secondary" onClick={handleSalvaProvvigione} disabled={salvandoProvvigione}>
                {salvandoProvvigione ? "Salvataggio…" : "Salva"}
              </Button>
            </div>
          )}
        </Card>

        <Card>
          <p className="mb-3 text-sm font-medium text-zinc-700">KPI commerciali</p>
          <dl className="space-y-1.5 text-sm">
            <Riga etichetta="Ordini totali" valore={String(dettaglio.numeroOrdiniTotali)} />
            <Riga etichetta="Fatturato totale" valore={formattaValuta(dettaglio.fatturatoTotale)} />
            <Riga etichetta="Ordine medio" valore={formattaValuta(dettaglio.ordineMedio)} />
            <Riga etichetta="Cucine acquistate" valore={String(dettaglio.numeroCucineAcquistate)} />
            <Riga etichetta="Elettrodomestici" valore={String(dettaglio.numeroElettrodomesticiAcquistati)} />
            <Riga etichetta="Ultimo acquisto" valore={formattaData(dettaglio.ultimoAcquisto)} />
          </dl>
        </Card>
      </div>

      <Card className="mt-4">
        <p className="mb-3 text-sm font-medium text-zinc-700">Note commerciali</p>

        {modificabile && (
          <form onSubmit={handleAggiungiNota} className="mb-4 flex gap-2">
            <input
              className="flex-1 rounded-md border border-zinc-300 px-3 py-2 text-sm focus:border-teal-600 focus:outline-none focus:ring-1 focus:ring-teal-600"
              placeholder="Aggiungi una nota…"
              value={nuovaNota}
              onChange={(e) => setNuovaNota(e.target.value)}
            />
            <Button type="submit" disabled={inviandoNota || !nuovaNota.trim()}>Aggiungi</Button>
          </form>
        )}

        {note.length === 0 ? (
          <p className="text-sm text-zinc-500">Nessuna nota registrata.</p>
        ) : (
          <ul className="space-y-3">
            {note.map((n) => (
              <li key={n.id} className="border-b border-zinc-100 pb-3 last:border-0">
                <p className="text-sm text-zinc-800">{n.testo}</p>
                <p className="mt-1 text-xs text-zinc-400">{n.utenteNomeCompleto} — {formattaDataOra(n.dataInserimento)}</p>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </div>
  );
}

// Città e provincia hanno una riga propria nella scheda: qui resta la sola via, con il CAP
// accostato perché da solo non direbbe nulla.
function formattaIndirizzo(c: ClienteDto): string {
  return [c.indirizzo, c.cap].filter(Boolean).join(" — ") || "—";
}

function Riga({ etichetta, valore }: { etichetta: string; valore: string }) {
  return (
    <div className="flex justify-between gap-4">
      <dt className="text-zinc-500">{etichetta}</dt>
      <dd className="text-right text-zinc-800">{valore}</dd>
    </div>
  );
}
