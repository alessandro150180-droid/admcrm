"use client";

import { useEffect, useState, type FormEvent } from "react";
import { api } from "@/lib/api";
import type { FornitoreDto } from "@/lib/types";
import { Badge, Button, Card, EmptyState, ErrorBlock, Field, Input, LoadingBlock, PageHeader, Table, Td, Th } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";
import { isSoloDirezione, useAuth } from "@/lib/auth-context";

export default function FornitoriPage() {
  const { utente } = useAuth();
  const [fornitori, setFornitori] = useState<FornitoreDto[]>([]);
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);
  const [nome, setNome] = useState("");
  const [creando, setCreando] = useState(false);

  function carica() {
    setCaricando(true);
    api.fornitori.lista().then(setFornitori).catch((err) => setErrore(messaggioErrore(err))).finally(() => setCaricando(false));
  }

  useEffect(carica, []);

  if (!isSoloDirezione(utente?.ruolo)) {
    return <ErrorBlock message="Non hai i permessi per gestire i fornitori." />;
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!nome.trim()) return;
    setErrore(null);
    setCreando(true);
    try {
      await api.fornitori.crea(nome.trim());
      setNome("");
      carica();
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setCreando(false);
    }
  }

  return (
    <div className="max-w-xl">
      <PageHeader
        title="Fornitori"
        subtitle="Brand/linee di prodotto di cui ADM rappresenta la vendita (Nobilia, NobiSmart, ...): usati per distinguere il fatturato in dashboard e negli ordini."
      />

      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      <Card className="mb-6">
        <form onSubmit={handleSubmit} className="flex items-end gap-3">
          <Field label="Nuovo fornitore">
            <Input placeholder="Es. NobiSmart" value={nome} onChange={(e) => setNome(e.target.value)} />
          </Field>
          <Button type="submit" disabled={creando || !nome.trim()}>{creando ? "Aggiunta…" : "Aggiungi"}</Button>
        </form>
      </Card>

      {caricando ? (
        <LoadingBlock />
      ) : fornitori.length === 0 ? (
        <EmptyState message="Nessun fornitore ancora censito." />
      ) : (
        <Table>
          <thead>
            <tr>
              <Th>Nome</Th>
              <Th>Stato</Th>
            </tr>
          </thead>
          <tbody>
            {fornitori.map((f) => (
              <tr key={f.id}>
                <Td className="font-medium text-zinc-900">{f.nome}</Td>
                <Td><Badge tone={f.attivo ? "green" : "zinc"}>{f.attivo ? "Attivo" : "Non attivo"}</Badge></Td>
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
}
