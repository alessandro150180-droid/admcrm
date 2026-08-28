"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { api } from "@/lib/api";
import type { ClienteDto, PrioritaAttivita, TipoAttivita } from "@/lib/types";
import { Button, ErrorBlock, Field, Input, PageHeader, Select } from "@/components/ui";
import { ETICHETTE_PRIORITA, ETICHETTE_TIPO_ATTIVITA, messaggioErrore } from "@/lib/format";

const TIPI: TipoAttivita[] = ["Telefonata", "Visita", "Preventivo", "FollowUp", "Reclamo", "Campionario", "Email", "Assistenza"];
const PRIORITA: PrioritaAttivita[] = ["Bassa", "Media", "Alta", "Urgente"];

export default function NuovaAttivitaPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const clienteIdPreselezionato = searchParams.get("clienteId");

  const [clienti, setClienti] = useState<ClienteDto[]>([]);
  const [clientePreselezionato, setClientePreselezionato] = useState<ClienteDto | null>(null);
  const [form, setForm] = useState({
    clienteId: clienteIdPreselezionato ?? "",
    tipoAttivita: "Telefonata" as TipoAttivita,
    titolo: "", descrizione: "", priorita: "Media" as PrioritaAttivita,
    dataScadenza: new Date(Date.now() + 24 * 3600 * 1000).toISOString().slice(0, 16),
  });
  const [errore, setErrore] = useState<string | null>(null);
  const [inviando, setInviando] = useState(false);

  useEffect(() => {
    if (clienteIdPreselezionato) {
      api.clienti.dettaglio(Number(clienteIdPreselezionato))
        .then((d) => setClientePreselezionato(d.anagrafica))
        .catch((err) => setErrore(messaggioErrore(err)));
    } else {
      api.clienti.lista({ dimensione: 500 })
        .then((r) => setClienti(r.elementi))
        .catch((err) => setErrore(messaggioErrore(err)));
    }
  }, [clienteIdPreselezionato]);

  function set<K extends keyof typeof form>(campo: K, valore: string) {
    setForm((prev) => ({ ...prev, [campo]: valore }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!form.clienteId || !form.titolo.trim()) {
      setErrore("Cliente e titolo sono obbligatori.");
      return;
    }
    setErrore(null);
    setInviando(true);
    try {
      const attivita = await api.attivita.crea({
        clienteId: Number(form.clienteId),
        tipoAttivita: form.tipoAttivita,
        titolo: form.titolo,
        descrizione: form.descrizione || undefined,
        priorita: form.priorita,
        dataScadenza: new Date(form.dataScadenza).toISOString(),
      });
      router.push(`/attivita?clienteId=${attivita.clienteId}`);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  return (
    <div className="max-w-xl">
      <PageHeader title="Nuova attività" />
      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      <form onSubmit={handleSubmit} className="space-y-4 rounded-lg border border-zinc-200 bg-white p-6">
        <Field label="Cliente *">
          {clientePreselezionato ? (
            <p className="rounded-md bg-zinc-50 px-3 py-2 text-sm text-zinc-800">
              {clientePreselezionato.ragioneSociale} ({clientePreselezionato.codiceCliente})
            </p>
          ) : (
            <Select required value={form.clienteId} onChange={(e) => set("clienteId", e.target.value)}>
              <option value="">Seleziona…</option>
              {clienti.map((c) => (
                <option key={c.id} value={c.id}>{c.ragioneSociale} ({c.codiceCliente})</option>
              ))}
            </Select>
          )}
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Tipo attività">
            <Select value={form.tipoAttivita} onChange={(e) => set("tipoAttivita", e.target.value)}>
              {TIPI.map((t) => <option key={t} value={t}>{ETICHETTE_TIPO_ATTIVITA[t]}</option>)}
            </Select>
          </Field>
          <Field label="Priorità">
            <Select value={form.priorita} onChange={(e) => set("priorita", e.target.value)}>
              {PRIORITA.map((p) => <option key={p} value={p}>{ETICHETTE_PRIORITA[p]}</option>)}
            </Select>
          </Field>
        </div>

        <Field label="Titolo *">
          <Input required value={form.titolo} onChange={(e) => set("titolo", e.target.value)} />
        </Field>

        <Field label="Descrizione">
          <textarea
            className="w-full rounded-md border border-zinc-300 px-3 py-2 text-sm focus:border-teal-600 focus:outline-none focus:ring-1 focus:ring-teal-600"
            rows={3}
            value={form.descrizione}
            onChange={(e) => set("descrizione", e.target.value)}
          />
        </Field>

        <Field label="Scadenza *">
          <Input type="datetime-local" required value={form.dataScadenza} onChange={(e) => set("dataScadenza", e.target.value)} />
        </Field>

        <Button type="submit" disabled={inviando}>{inviando ? "Salvataggio…" : "Crea attività"}</Button>
      </form>
    </div>
  );
}
