"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { AuditLogDto } from "@/lib/types";
import { Badge, EmptyState, ErrorBlock, Input, LoadingBlock, PageHeader, Pagination, Table, Td, Th } from "@/components/ui";
import { formattaDataOra, messaggioErrore } from "@/lib/format";
import { isSoloDirezione, useAuth } from "@/lib/auth-context";

const TONO_AZIONE: Record<string, "green" | "amber" | "red"> = {
  Creazione: "green",
  Modifica: "amber",
  Eliminazione: "red",
};

export default function AuditLogPage() {
  const { utente } = useAuth();
  const [dati, setDati] = useState<{ elementi: AuditLogDto[]; pagina: number; totalePagine: number; totaleElementi: number } | null>(null);
  const [pagina, setPagina] = useState(1);
  const [nomeEntita, setNomeEntita] = useState("");
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);

  useEffect(() => {
    if (!isSoloDirezione(utente?.ruolo)) return;
    let annullato = false;
    setCaricando(true);
    api.auditLog
      .lista({ pagina, dimensione: 50, nomeEntita: nomeEntita || undefined })
      .then((r) => !annullato && setDati(r))
      .catch((err) => !annullato && setErrore(messaggioErrore(err)))
      .finally(() => !annullato && setCaricando(false));
    return () => {
      annullato = true;
    };
  }, [pagina, nomeEntita, utente]);

  if (!isSoloDirezione(utente?.ruolo)) {
    return <ErrorBlock message="Non hai i permessi per consultare l'audit log." />;
  }

  return (
    <div>
      <PageHeader title="Audit log" subtitle={dati ? `${dati.totaleElementi} eventi registrati` : undefined} />

      <div className="mb-4">
        <Input
          placeholder="Filtra per entità (es. Cliente, Ordine, Attivita)…"
          value={nomeEntita}
          onChange={(e) => { setPagina(1); setNomeEntita(e.target.value); }}
          className="max-w-sm"
        />
      </div>

      {errore && <ErrorBlock message={errore} />}

      {caricando && !dati ? (
        <LoadingBlock />
      ) : !dati || dati.elementi.length === 0 ? (
        <EmptyState message="Nessun evento trovato." />
      ) : (
        <>
          <Table>
            <thead>
              <tr>
                <Th>Data/ora</Th>
                <Th>Utente</Th>
                <Th>Entità</Th>
                <Th>Id</Th>
                <Th>Azione</Th>
              </tr>
            </thead>
            <tbody>
              {dati.elementi.map((a) => (
                <tr key={a.id}>
                  <Td>{formattaDataOra(a.dataCreazione)}</Td>
                  <Td>{a.utenteNomeCompleto ?? "Sistema"}</Td>
                  <Td>{a.nomeEntita}</Td>
                  <Td>{a.entitaId}</Td>
                  <Td><Badge tone={TONO_AZIONE[a.azione] ?? "zinc"}>{a.azione}</Badge></Td>
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
