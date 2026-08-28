"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { api, scaricaBlob } from "@/lib/api";
import type { OrdineDto } from "@/lib/types";
import {
  Button, EmptyState, ErrorBlock, LoadingBlock, PageHeader, Pagination, Select, Table, Td, Th, Tr,
} from "@/components/ui";
import { formattaData, formattaValuta, messaggioErrore, NOMI_MESI } from "@/lib/format";
import { StatoOrdineBadge } from "@/components/StatoBadge";

const ORA = new Date();
const ANNI = Array.from({ length: 5 }, (_, i) => ORA.getFullYear() - i);

export default function OrdiniPage() {
  const router = useRouter();
  const [dati, setDati] = useState<{ elementi: OrdineDto[]; pagina: number; totalePagine: number; totaleElementi: number } | null>(null);
  const [pagina, setPagina] = useState(1);
  const [anno, setAnno] = useState<number | "">("");
  const [mese, setMese] = useState<number | "">("");
  const [caricando, setCaricando] = useState(true);
  const [errore, setErrore] = useState<string | null>(null);

  useEffect(() => {
    let annullato = false;
    setCaricando(true);
    api.ordini
      .lista({ pagina, dimensione: 20, anno: anno || undefined, mese: mese || undefined })
      .then((r) => !annullato && setDati(r))
      .catch((err) => !annullato && setErrore(messaggioErrore(err)))
      .finally(() => !annullato && setCaricando(false));
    return () => {
      annullato = true;
    };
  }, [pagina, anno, mese]);

  async function handleEsportaCsv() {
    try {
      const blob = await api.ordini.esportaCsv({ anno: anno || undefined, mese: mese || undefined });
      scaricaBlob(blob, "ordini.csv");
    } catch (err) {
      setErrore(messaggioErrore(err));
    }
  }

  return (
    <div>
      <PageHeader
        title="Ordini"
        subtitle={dati ? `${dati.totaleElementi} ordini totali` : undefined}
        actions={
          <>
            <Button variant="secondary" onClick={handleEsportaCsv}>Esporta CSV</Button>
            <Link href="/ordini/nuovo"><Button>+ Nuovo ordine</Button></Link>
          </>
        }
      />

      <div className="mb-4 flex gap-3">
        <Select value={mese} onChange={(e) => { setPagina(1); setMese(e.target.value ? Number(e.target.value) : ""); }} className="w-40">
          <option value="">Tutti i mesi</option>
          {NOMI_MESI.map((nome, i) => <option key={nome} value={i + 1}>{nome}</option>)}
        </Select>
        <Select value={anno} onChange={(e) => { setPagina(1); setAnno(e.target.value ? Number(e.target.value) : ""); }} className="w-32">
          <option value="">Tutti gli anni</option>
          {ANNI.map((a) => <option key={a} value={a}>{a}</option>)}
        </Select>
      </div>

      {errore && <ErrorBlock message={errore} />}

      {caricando && !dati ? (
        <LoadingBlock />
      ) : !dati || dati.elementi.length === 0 ? (
        <EmptyState message="Nessun ordine trovato con questi filtri." />
      ) : (
        <>
          <Table>
            <thead>
              <tr>
                <Th>Data</Th>
                <Th>Cliente</Th>
                <Th>Importo</Th>
                <Th>Cucine</Th>
                <Th>Stato</Th>
                <Th>Riferimento</Th>
              </tr>
            </thead>
            <tbody>
              {dati.elementi.map((o) => (
                <Tr key={o.id} onClick={() => router.push(`/ordini/${o.id}`)}>
                  <Td>{formattaData(o.dataOrdine)}</Td>
                  <Td className="font-medium text-zinc-900">{o.clienteRagioneSociale}</Td>
                  <Td>{formattaValuta(o.importo)}</Td>
                  <Td>{o.numeroCucine}</Td>
                  <Td><StatoOrdineBadge stato={o.statoOrdine} /></Td>
                  <Td>{o.riferimentoEsterno ?? "—"}</Td>
                </Tr>
              ))}
            </tbody>
          </Table>
          <Pagination pagina={dati.pagina} totalePagine={dati.totalePagine} onCambiaPagina={setPagina} />
        </>
      )}
    </div>
  );
}
