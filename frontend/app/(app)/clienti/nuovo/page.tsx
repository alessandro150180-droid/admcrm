"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import type { AgenteDto } from "@/lib/types";
import { Button, ErrorBlock, Field, Input, PageHeader, Select } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";
import { isDirezioneOAreaManager, useAuth } from "@/lib/auth-context";

export default function NuovoClientePage() {
  const { utente } = useAuth();
  const router = useRouter();
  const [agenti, setAgenti] = useState<AgenteDto[]>([]);
  const [form, setForm] = useState({
    ragioneSociale: "", codiceCliente: "", partitaIVA: "", indirizzo: "", citta: "",
    provincia: "", regione: "", cap: "", telefono: "", email: "",
    agenteId: "", percentualeProvvigione: "0",
  });
  const [errore, setErrore] = useState<string | null>(null);
  const [inviando, setInviando] = useState(false);

  useEffect(() => {
    api.agenti.lista().then(setAgenti).catch((err) => setErrore(messaggioErrore(err)));
  }, []);

  if (!isDirezioneOAreaManager(utente?.ruolo)) {
    return <ErrorBlock message="Non hai i permessi per creare un nuovo cliente." />;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!form.agenteId) {
      setErrore("Seleziona un agente.");
      return;
    }
    setErrore(null);
    setInviando(true);
    try {
      const cliente = await api.clienti.crea({
        ragioneSociale: form.ragioneSociale,
        codiceCliente: form.codiceCliente,
        partitaIVA: form.partitaIVA || undefined,
        indirizzo: form.indirizzo || undefined,
        citta: form.citta || undefined,
        provincia: form.provincia || undefined,
        regione: form.regione || undefined,
        cap: form.cap || undefined,
        telefono: form.telefono || undefined,
        email: form.email || undefined,
        agenteId: Number(form.agenteId),
        percentualeProvvigione: Number(form.percentualeProvvigione) || 0,
      });
      router.push(`/clienti/${cliente.id}`);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  function set<K extends keyof typeof form>(campo: K, valore: string) {
    setForm((prev) => ({ ...prev, [campo]: valore }));
  }

  return (
    <div className="max-w-2xl">
      <PageHeader title="Nuovo cliente" />
      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      <form onSubmit={handleSubmit} className="space-y-4 rounded-lg border border-zinc-200 bg-white p-6">
        <div className="grid grid-cols-2 gap-4">
          <Field label="Ragione sociale *">
            <Input required value={form.ragioneSociale} onChange={(e) => set("ragioneSociale", e.target.value)} />
          </Field>
          <Field label="Codice cliente *">
            <Input required value={form.codiceCliente} onChange={(e) => set("codiceCliente", e.target.value)} />
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Agente *">
            <Select required value={form.agenteId} onChange={(e) => set("agenteId", e.target.value)}>
              <option value="">Seleziona…</option>
              {agenti.map((a) => (
                <option key={a.id} value={a.id}>{a.nome} {a.cognome} — {a.zona}</option>
              ))}
            </Select>
          </Field>
          <Field label="Provvigione (%)">
            <Input type="number" min="0" max="100" step="0.5" value={form.percentualeProvvigione} onChange={(e) => set("percentualeProvvigione", e.target.value)} />
          </Field>
        </div>

        <Field label="Partita IVA">
          <Input value={form.partitaIVA} onChange={(e) => set("partitaIVA", e.target.value)} />
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Indirizzo">
            <Input value={form.indirizzo} onChange={(e) => set("indirizzo", e.target.value)} />
          </Field>
          <Field label="CAP">
            <Input value={form.cap} onChange={(e) => set("cap", e.target.value)} />
          </Field>
        </div>

        <div className="grid grid-cols-3 gap-4">
          <Field label="Città">
            <Input value={form.citta} onChange={(e) => set("citta", e.target.value)} />
          </Field>
          <Field label="Provincia (sigla)">
            <Input maxLength={2} value={form.provincia} onChange={(e) => set("provincia", e.target.value.toUpperCase())} />
          </Field>
          <Field label="Regione">
            <Input value={form.regione} onChange={(e) => set("regione", e.target.value)} />
          </Field>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Telefono">
            <Input value={form.telefono} onChange={(e) => set("telefono", e.target.value)} />
          </Field>
          <Field label="Email">
            <Input type="email" value={form.email} onChange={(e) => set("email", e.target.value)} />
          </Field>
        </div>

        <Button type="submit" disabled={inviando}>{inviando ? "Salvataggio…" : "Crea cliente"}</Button>
      </form>
    </div>
  );
}
