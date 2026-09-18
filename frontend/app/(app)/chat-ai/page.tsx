"use client";

import { useEffect, useRef, useState } from "react";
import { api } from "@/lib/api";
import type { ChatAiMessaggioDto } from "@/lib/types";
import { Button, ErrorBlock, PageHeader, Spinner } from "@/components/ui";
import { messaggioErrore } from "@/lib/format";

const SUGGERIMENTI = [
  "Come va il fatturato di questo mese rispetto allo scorso anno?",
  "Fammi un riepilogo delle provvigioni di questo mese",
  "Quanto ha fatturato quest'anno il cliente ...?",
];

export default function ChatAiPage() {
  const [messaggi, setMessaggi] = useState<ChatAiMessaggioDto[]>([]);
  const [testo, setTesto] = useState("");
  const [inviando, setInviando] = useState(false);
  const [errore, setErrore] = useState<string | null>(null);
  const fineListaRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    fineListaRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messaggi, inviando]);

  async function invia(testoDaInviare: string) {
    const domanda = testoDaInviare.trim();
    if (!domanda || inviando) return;

    const cronologia = messaggi;
    setMessaggi((prev) => [...prev, { ruolo: "utente", testo: domanda }]);
    setTesto("");
    setErrore(null);
    setInviando(true);

    try {
      const risposta = await api.chatAi.messaggio(domanda, cronologia);
      setMessaggi((prev) => [...prev, { ruolo: "assistente", testo: risposta.testo }]);
    } catch (err) {
      setErrore(messaggioErrore(err));
    } finally {
      setInviando(false);
    }
  }

  function handleKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      invia(testo);
    }
  }

  return (
    <div className="flex h-[calc(100vh-4rem)] max-w-3xl flex-col">
      <PageHeader title="ChatAI" subtitle="Chiedi qualsiasi cosa sui dati del CRM: fatturato, provvigioni, andamento clienti e agenti." />

      <div className="flex flex-1 flex-col overflow-y-auto rounded-lg border border-zinc-200 bg-white p-4">
        {messaggi.length === 0 ? (
          <div className="flex flex-1 flex-col items-center justify-center gap-3 text-center text-sm text-zinc-500">
            <p>Fai una domanda sui tuoi dati per iniziare.</p>
            <div className="flex flex-wrap justify-center gap-2">
              {SUGGERIMENTI.map((s) => (
                <button
                  key={s}
                  onClick={() => invia(s)}
                  className="rounded-full border border-zinc-200 px-3 py-1.5 text-xs text-zinc-600 hover:bg-zinc-50"
                >
                  {s}
                </button>
              ))}
            </div>
          </div>
        ) : (
          <div className="flex-1 space-y-3">
            {messaggi.map((m, i) => (
              <div key={i} className={`flex ${m.ruolo === "utente" ? "justify-end" : "justify-start"}`}>
                <div
                  className={`max-w-[80%] whitespace-pre-wrap rounded-lg px-3.5 py-2.5 text-sm ${
                    m.ruolo === "utente" ? "bg-teal-700 text-white" : "bg-zinc-100 text-zinc-800"
                  }`}
                >
                  {m.testo}
                </div>
              </div>
            ))}
            {inviando && (
              <div className="flex justify-start">
                <div className="flex items-center gap-2 rounded-lg bg-zinc-100 px-3.5 py-2.5 text-sm text-zinc-500">
                  <Spinner className="h-3.5 w-3.5" /> Sto controllando i dati…
                </div>
              </div>
            )}
          </div>
        )}
        <div ref={fineListaRef} />
      </div>

      {errore && <div className="mt-3"><ErrorBlock message={errore} /></div>}

      <div className="mt-3 flex gap-2">
        <textarea
          value={testo}
          onChange={(e) => setTesto(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Scrivi una domanda… (Invio per inviare, Maiusc+Invio per andare a capo)"
          rows={2}
          className="flex-1 resize-none rounded-md border border-zinc-300 px-3 py-2 text-sm focus:border-teal-600 focus:outline-none focus:ring-1 focus:ring-teal-600"
        />
        <Button onClick={() => invia(testo)} disabled={inviando || !testo.trim()}>
          Invia
        </Button>
      </div>
    </div>
  );
}
