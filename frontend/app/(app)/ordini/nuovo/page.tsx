"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { api } from "@/lib/api";
import type { ClienteDto } from "@/lib/types";
import { Button, ErrorBlock, Field, Input, PageHeader, Select } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";
import { puoModificare, useAuth } from "@/lib/auth-context";

export default function NuovoOrdinePage() {
  const { utente } = useAuth();
  const router = useRouter();
  const searchParams = useSearchParams();
  const clienteIdPreselezionato = searchParams.get("clienteId");

  const [clienti, setClienti] = useState<ClienteDto[]>([]);
  const [clientePreselezionato, setClientePreselezionato] = useState<ClienteDto | null>(null);
  const [form, setForm] = useState({
    clienteId: clienteIdPreselezionato ?? "",
    dataOrdine: new Date().toISOString().slice(0, 10),
    importo: "", numeroCucine: "0", numeroElettrodomestici: "0", numeroComplementi: "0", riferimentoEsterno: "",
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

  if (!puoModificare(utente?.ruolo)) {
    return <ErrorBlock message="Non hai i permessi per creare un nuovo ordine." />;
  }

  function set<K extends keyof typeof form>(campo: K, valore: string) {
    setForm((prev) => ({ ...prev, [campo]: valore }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!form.clienteId || !form.importo) {
      setErrore("Cliente e importo sono obbligatori.");
      return;
    }
    setErrore(null);
    setInviando(true);
    try {
      const ordine = await api.ordini.crea({
        clienteId: Number(form.clienteId),
        dataOrdine: new Date(form.dataOrdine).toISOString(),
        importo: Number(form.importo),
        numeroCucine: Number(form.numeroCucine) || 0,
        numeroElettrodomestici: Number(form.numeroElettrodomestici) || 0,
        numeroComplementi: Number(form.numeroComplementi) || 0,
        riferimentoEsterno: form.riferimentoEsterno || undefined,
      });
      router.push(`/ordini/${ordine.id}`);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  return (
    <div className="max-w-xl">
      <PageHeader title="Nuovo ordine" />
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
          <Field label="Data ordine *">
            <Input type="date" required value={form.dataOrdine} onChange={(e) => set("dataOrdine", e.target.value)} />
          </Field>
          <Field label="Importo (€) *">
            <Input type="number" step="0.01" min="0" required value={form.importo} onChange={(e) => set("importo", e.target.value)} />
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label="N. cucine">
            <Input type="number" min="0" value={form.numeroCucine} onChange={(e) => set("numeroCucine", e.target.value)} />
          </Field>
          <Field label="N. elettrodomestici">
            <Input type="number" min="0" value={form.numeroElettrodomestici} onChange={(e) => set("numeroElettrodomestici", e.target.value)} />
          </Field>
          <Field label="N. complementi">
            <Input type="number" min="0" value={form.numeroComplementi} onChange={(e) => set("numeroComplementi", e.target.value)} />
          </Field>
        </div>

        <Field label="Riferimento esterno">
          <Input placeholder="Es. numero ordine gestionale" value={form.riferimentoEsterno} onChange={(e) => set("riferimentoEsterno", e.target.value)} />
        </Field>

        <Button type="submit" disabled={inviando}>{inviando ? "Salvataggio…" : "Crea ordine"}</Button>
      </form>
    </div>
  );
}
