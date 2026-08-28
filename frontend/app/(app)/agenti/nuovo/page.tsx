"use client";

import { useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import { Button, ErrorBlock, Field, Input, PageHeader } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";
import { isDirezioneOAreaManager, useAuth } from "@/lib/auth-context";

export default function NuovoAgentePage() {
  const { utente } = useAuth();
  const router = useRouter();
  const [form, setForm] = useState({ nome: "", cognome: "", zona: "", telefono: "", email: "" });
  const [errore, setErrore] = useState<string | null>(null);
  const [inviando, setInviando] = useState(false);

  if (!isDirezioneOAreaManager(utente?.ruolo)) {
    return <ErrorBlock message="Non hai i permessi per creare un nuovo agente." />;
  }

  function set<K extends keyof typeof form>(campo: K, valore: string) {
    setForm((prev) => ({ ...prev, [campo]: valore }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrore(null);
    setInviando(true);
    try {
      const agente = await api.agenti.crea({
        nome: form.nome,
        cognome: form.cognome,
        zona: form.zona,
        telefono: form.telefono || undefined,
        email: form.email,
      });
      router.push(`/agenti?creato=${agente.id}`);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  return (
    <div className="max-w-lg">
      <PageHeader title="Nuovo agente" />
      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      <form onSubmit={handleSubmit} className="space-y-4 rounded-lg border border-zinc-200 bg-white p-6">
        <div className="grid grid-cols-2 gap-4">
          <Field label="Nome *">
            <Input required value={form.nome} onChange={(e) => set("nome", e.target.value)} />
          </Field>
          <Field label="Cognome *">
            <Input required value={form.cognome} onChange={(e) => set("cognome", e.target.value)} />
          </Field>
        </div>

        <Field label="Zona *">
          <Input required placeholder="Es. Puglia, Lombardia…" value={form.zona} onChange={(e) => set("zona", e.target.value)} />
        </Field>

        <div className="grid grid-cols-2 gap-4">
          <Field label="Telefono">
            <Input value={form.telefono} onChange={(e) => set("telefono", e.target.value)} />
          </Field>
          <Field label="Email *">
            <Input type="email" required value={form.email} onChange={(e) => set("email", e.target.value)} />
          </Field>
        </div>

        <Button type="submit" disabled={inviando}>{inviando ? "Salvataggio…" : "Crea agente"}</Button>
      </form>
    </div>
  );
}
