"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import type { AgenteDto } from "@/lib/types";
import { Button, EmptyState, ErrorBlock, LoadingBlock, PageHeader, Table, Td, Th } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";
import { isDirezioneOAreaManager, isSoloDirezione, useAuth } from "@/lib/auth-context";

export default function AgentiPage() {
  const { utente } = useAuth();
  const [agenti, setAgenti] = useState<AgenteDto[]>([]);
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);
  const [eliminando, setEliminando] = useState<number | null>(null);

  function carica() {
    setCaricando(true);
    api.agenti.lista()
      .then(setAgenti)
      .catch((err) => setErrore(messaggioErrore(err)))
      .finally(() => setCaricando(false));
  }

  useEffect(carica, []);

  async function handleElimina(agente: AgenteDto) {
    const conferma = window.confirm(
      `Eliminare ${agente.nome} ${agente.cognome}? Verranno eliminati anche tutti i suoi clienti e i relativi ordini, attività, note e obiettivi. L'operazione non è reversibile dall'interfaccia.`
    );
    if (!conferma) return;

    setEliminando(agente.id);
    setErrore(null);
    try {
      await api.agenti.elimina(agente.id);
      carica();
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setEliminando(null);
    }
  }

  return (
    <div>
      <PageHeader
        title="Agenti"
        subtitle={`${agenti.length} agenti`}
        actions={
          isDirezioneOAreaManager(utente?.ruolo)
            ? <Link href="/agenti/nuovo"><Button>+ Nuovo agente</Button></Link>
            : undefined
        }
      />

      {errore && <ErrorBlock message={errore} />}

      {caricando ? (
        <LoadingBlock />
      ) : agenti.length === 0 ? (
        <EmptyState message="Nessun agente registrato." />
      ) : (
        <Table>
          <thead>
            <tr>
              <Th>Nome</Th>
              <Th>Zona</Th>
              <Th>Telefono</Th>
              <Th>Email</Th>
              {isSoloDirezione(utente?.ruolo) && <Th>&nbsp;</Th>}
            </tr>
          </thead>
          <tbody>
            {agenti.map((a) => (
              <tr key={a.id}>
                <Td className="font-medium text-zinc-900">{a.nome} {a.cognome}</Td>
                <Td>{a.zona}</Td>
                <Td>{a.telefono ?? "—"}</Td>
                <Td>{a.email}</Td>
                {isSoloDirezione(utente?.ruolo) && (
                  <Td>
                    <Button variant="ghost" onClick={() => handleElimina(a)} disabled={eliminando === a.id}>
                      {eliminando === a.id ? "…" : "Elimina"}
                    </Button>
                  </Td>
                )}
              </tr>
            ))}
          </tbody>
        </Table>
      )}
    </div>
  );
}
