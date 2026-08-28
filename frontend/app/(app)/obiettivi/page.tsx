"use client";

import { useEffect, useState, type FormEvent } from "react";
import { api } from "@/lib/api";
import type { AgenteDto, ObiettivoVenditaDto } from "@/lib/types";
import { Button, Card, EmptyState, ErrorBlock, Field, PageHeader, Select, Table, Td, Th } from "@/components/ui";
import { formattaPercentuale, formattaValuta, messaggioErrore, NOMI_MESI } from "@/lib/format";
import { isDirezioneOAreaManager, useAuth } from "@/lib/auth-context";

const ORA = new Date();
const ANNI = Array.from({ length: 5 }, (_, i) => ORA.getFullYear() - i);

export default function ObiettiviPage() {
  const { utente } = useAuth();
  const [anno, setAnno] = useState(ORA.getFullYear());
  const [obiettivi, setObiettivi] = useState<ObiettivoVenditaDto[]>([]);
  const [agenti, setAgenti] = useState<AgenteDto[]>([]);
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);

  const [form, setForm] = useState({ agenteId: "", mese: String(ORA.getMonth() + 1), obiettivoFatturato: "", obiettivoCucine: "" });
  const [inviando, setInviando] = useState(false);

  function carica() {
    setCaricando(true);
    api.obiettivi.lista(anno).then(setObiettivi).catch((err) => setErrore(messaggioErrore(err))).finally(() => setCaricando(false));
  }

  useEffect(() => {
    carica();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [anno]);

  useEffect(() => {
    if (isDirezioneOAreaManager(utente?.ruolo)) {
      api.agenti.lista().then(setAgenti).catch(() => {});
    }
  }, [utente]);

  async function handleImposta(e: FormEvent) {
    e.preventDefault();
    if (!form.agenteId || !form.obiettivoFatturato) {
      setErrore("Agente e obiettivo di fatturato sono obbligatori.");
      return;
    }
    setInviando(true);
    setErrore(null);
    try {
      await api.obiettivi.imposta({
        agenteId: Number(form.agenteId),
        mese: Number(form.mese),
        anno,
        obiettivoFatturato: Number(form.obiettivoFatturato),
        obiettivoCucine: Number(form.obiettivoCucine) || 0,
      });
      setForm((prev) => ({ ...prev, obiettivoFatturato: "", obiettivoCucine: "" }));
      carica();
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  return (
    <div>
      <PageHeader
        title="Obiettivi di vendita"
        actions={
          <Select value={anno} onChange={(e) => setAnno(Number(e.target.value))} className="w-28">
            {ANNI.map((a) => <option key={a} value={a}>{a}</option>)}
          </Select>
        }
      />

      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      {isDirezioneOAreaManager(utente?.ruolo) && (
        <Card className="mb-6">
          <p className="mb-3 text-sm font-medium text-zinc-700">Imposta obiettivo ({anno})</p>
          <form onSubmit={handleImposta} className="grid grid-cols-2 gap-4 sm:grid-cols-4">
            <Field label="Agente">
              <Select value={form.agenteId} onChange={(e) => setForm((p) => ({ ...p, agenteId: e.target.value }))}>
                <option value="">Seleziona…</option>
                {agenti.map((a) => <option key={a.id} value={a.id}>{a.nome} {a.cognome}</option>)}
              </Select>
            </Field>
            <Field label="Mese">
              <Select value={form.mese} onChange={(e) => setForm((p) => ({ ...p, mese: e.target.value }))}>
                {NOMI_MESI.map((nome, i) => <option key={nome} value={i + 1}>{nome}</option>)}
              </Select>
            </Field>
            <Field label="Obiettivo fatturato (€)">
              <input
                type="number" min="0" step="100"
                className="w-full rounded-md border border-zinc-300 px-3 py-2 text-sm focus:border-teal-600 focus:outline-none focus:ring-1 focus:ring-teal-600"
                value={form.obiettivoFatturato}
                onChange={(e) => setForm((p) => ({ ...p, obiettivoFatturato: e.target.value }))}
              />
            </Field>
            <Field label="Obiettivo cucine">
              <input
                type="number" min="0"
                className="w-full rounded-md border border-zinc-300 px-3 py-2 text-sm focus:border-teal-600 focus:outline-none focus:ring-1 focus:ring-teal-600"
                value={form.obiettivoCucine}
                onChange={(e) => setForm((p) => ({ ...p, obiettivoCucine: e.target.value }))}
              />
            </Field>
            <div className="col-span-2 sm:col-span-4">
              <Button type="submit" disabled={inviando}>{inviando ? "Salvataggio…" : "Imposta obiettivo"}</Button>
            </div>
          </form>
        </Card>
      )}

      {caricando ? null : obiettivi.length === 0 ? (
        <EmptyState message="Nessun obiettivo impostato per questo anno." />
      ) : (
        <Table>
          <thead>
            <tr>
              <Th>Agente</Th>
              <Th>Mese</Th>
              <Th>Obiettivo fatturato</Th>
              <Th>Realizzato</Th>
              <Th>Raggiungimento</Th>
              <Th>Obiettivo cucine</Th>
            </tr>
          </thead>
          <tbody>
            {obiettivi.map((o) => (
              <tr key={o.id}>
                <Td className="font-medium text-zinc-900">{o.agenteNomeCompleto}</Td>
                <Td>{NOMI_MESI[o.mese - 1]}</Td>
                <Td>{formattaValuta(o.obiettivoFatturato)}</Td>
                <Td>{formattaValuta(o.fatturatoRealizzato)}</Td>
                <Td>
                  <div className="flex items-center gap-2">
                    <div className="h-1.5 w-24 overflow-hidden rounded-full bg-zinc-100">
                      <div
                        className={`h-full rounded-full ${o.percentualeRaggiungimento >= 100 ? "bg-emerald-600" : "bg-teal-600"}`}
                        style={{ width: `${Math.min(o.percentualeRaggiungimento, 100)}%` }}
                      />
                    </div>
                    <span className="text-xs text-zinc-500">{formattaPercentuale(o.percentualeRaggiungimento)}</span>
                  </div>
                </Td>
                <Td>{o.obiettivoCucine}</Td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
}
