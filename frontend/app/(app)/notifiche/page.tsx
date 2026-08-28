"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { NotificaDto } from "@/lib/types";
import { Badge, Button, Card, EmptyState, ErrorBlock, LoadingBlock, PageHeader } from "@/components/ui";
import { formattaDataOra, messaggioErrore } from "@/lib/format";
import { isSoloDirezione, useAuth } from "@/lib/auth-context";

export default function NotifichePage() {
  const { utente } = useAuth();
  const [notifiche, setNotifiche] = useState<NotificaDto[]>([]);
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);
  const [generando, setGenerando] = useState(false);

  function carica() {
    setCaricando(true);
    api.notifiche.lista().then(setNotifiche).catch((err) => setErrore(messaggioErrore(err))).finally(() => setCaricando(false));
  }

  useEffect(carica, []);

  async function handleSegnaComeLetta(id: number) {
    try {
      await api.notifiche.segnaComeLetta(id);
      setNotifiche((prev) => prev.map((n) => (n.id === id ? { ...n, letta: true } : n)));
    } catch (err) {
      setErrore(messaggioErrore(err));
    }
  }

  async function handleGeneraScadute() {
    setGenerando(true);
    setErrore(null);
    try {
      const { notificheGenerate } = await api.notifiche.generaScadute();
      if (notificheGenerate > 0) carica();
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setGenerando(false);
    }
  }

  return (
    <div className="max-w-2xl">
      <PageHeader
        title="Notifiche"
        actions={
          isSoloDirezione(utente?.ruolo) ? (
            <Button variant="secondary" onClick={handleGeneraScadute} disabled={generando}>
              {generando ? "Scansione…" : "Scansiona attività scadute"}
            </Button>
          ) : undefined
        }
      />

      {errore && <div className="mb-4"><ErrorBlock message={errore} /></div>}

      {caricando ? (
        <LoadingBlock />
      ) : notifiche.length === 0 ? (
        <EmptyState message="Nessuna notifica." />
      ) : (
        <div className="space-y-2">
          {notifiche.map((n) => (
            <Card key={n.id} className={n.letta ? "opacity-60" : ""}>
              <div className="flex items-start justify-between gap-4">
                <div>
                  <div className="flex items-center gap-2">
                    <p className="text-sm font-medium text-zinc-900">{n.titolo}</p>
                    {!n.letta && <Badge tone="blue">Nuova</Badge>}
                  </div>
                  {n.messaggio && <p className="mt-1 text-sm text-zinc-600">{n.messaggio}</p>}
                  <p className="mt-1 text-xs text-zinc-400">{formattaDataOra(n.dataCreazione)}</p>
                </div>
                {!n.letta && (
                  <Button variant="ghost" onClick={() => handleSegnaComeLetta(n.id)}>Segna come letta</Button>
                )}
              </div>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
