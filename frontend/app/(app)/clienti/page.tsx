"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import type { ClienteDto } from "@/lib/types";
import {
  Button, EmptyState, ErrorBlock, Input, LoadingBlock, PageHeader, Pagination, Table, Td, Th, Tr,
} from "@/components/ui";
import { formattaData, formattaPercentuale, messaggioErrore } from "@/lib/format";
import { useAuth, isDirezioneOAreaManager } from "@/lib/auth-context";
import { scaricaBlob } from "@/lib/api";

export default function ClientiPage() {
  const { utente } = useAuth();
  const [dati, setDati] = useState<{ elementi: ClienteDto[]; pagina: number; totalePagine: number; totaleElementi: number } | null>(null);
  const [pagina, setPagina] = useState(1);
  const [regione, setRegione] = useState("");
  const [provincia, setProvincia] = useState("");
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);

  useEffect(() => {
    let annullato = false;
    setCaricando(true);
    api.clienti
      .lista({ pagina, dimensione: 20, regione, provincia })
      .then((r) => !annullato && setDati(r))
      .catch((err) => !annullato && setErrore(messaggioErrore(err)))
      .finally(() => !annullato && setCaricando(false));
    return () => {
      annullato = true;
    };
  }, [pagina, regione, provincia]);

  async function handleEsportaCsv() {
    try {
      const blob = await api.clienti.esportaCsv({ regione, provincia });
      scaricaBlob(blob, "clienti.csv");
    } catch (err) {
      setErrore(messaggioErrore(err));
    }
  }

  return (
    <div>
      <PageHeader
        title="Clienti"
        subtitle={dati ? `${dati.totaleElementi} clienti totali` : undefined}
        actions={
          <>
            <Button variant="secondary" onClick={handleEsportaCsv}>Esporta CSV</Button>
            {isDirezioneOAreaManager(utente?.ruolo) && (
              <Link href="/clienti/nuovo"><Button>+ Nuovo cliente</Button></Link>
            )}
          </>
        }
      />

      <div className="mb-4 flex gap-3">
        <Input placeholder="Filtra per regione…" value={regione} onChange={(e) => { setPagina(1); setRegione(e.target.value); }} className="max-w-xs" />
        <Input placeholder="Filtra per provincia (sigla)…" value={provincia} onChange={(e) => { setPagina(1); setProvincia(e.target.value); }} className="max-w-xs" />
      </div>

      {errore && <ErrorBlock message={errore} />}

      {caricando && !dati ? (
        <LoadingBlock />
      ) : !dati || dati.elementi.length === 0 ? (
        <EmptyState message="Nessun cliente trovato con questi filtri." />
      ) : (
        <>
          <Table>
            <thead>
              <tr>
                <Th>Ragione sociale</Th>
                <Th>Codice</Th>
                <Th>Agente</Th>
                <Th>E-mail agente</Th>
                <Th>Indirizzo</Th>
                <Th>Città</Th>
                <Th>Provincia</Th>
                <Th>Regione</Th>
                <Th>Partita IVA</Th>
                <Th>E-mail cliente</Th>
                <Th>Telefono</Th>
                <Th>Provvigione</Th>
                <Th>Cliente dal</Th>
              </tr>
            </thead>
            <tbody>
              {dati.elementi.map((c) => (
                <Row key={c.id} cliente={c} />
              ))}
            </tbody>
          </Table>
          <Pagination pagina={dati.pagina} totalePagine={dati.totalePagine} onCambiaPagina={setPagina} />
        </>
      )}
    </div>
  );
}

function Row({ cliente }: { cliente: ClienteDto }) {
  const router = useRouter();
  return (
    <Tr onClick={() => router.push(`/clienti/${cliente.id}`)}>
      <Td className="font-medium text-zinc-900">{cliente.ragioneSociale}</Td>
      <Td>{cliente.codiceCliente}</Td>
      <Td>{cliente.agenteNomeCompleto}</Td>
      <Td>{cliente.agenteEmail || "—"}</Td>
      <Td>{cliente.indirizzo ?? "—"}</Td>
      <Td>{cliente.citta ?? "—"}</Td>
      <Td>{cliente.provincia ?? "—"}</Td>
      <Td>{cliente.regione ?? "—"}</Td>
      <Td>{cliente.partitaIVA ?? "—"}</Td>
      <Td>{cliente.email ?? "—"}</Td>
      <Td>{cliente.telefono ?? "—"}</Td>
      <Td>{formattaPercentuale(cliente.percentualeProvvigione)}</Td>
      <Td>{formattaData(cliente.dataInserimento)}</Td>
    </Tr>
  );
}
