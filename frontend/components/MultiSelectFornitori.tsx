"use client";

import { useEffect, useRef, useState } from "react";
import type { FornitoreDto } from "@/lib/types";

interface MultiSelectFornitoriProps {
  fornitori: FornitoreDto[];
  selezionati: number[];
  onChange: (fornitoreIds: number[]) => void;
  className?: string;
}

/** Selettore fornitori (Nobilia, NobiSmart, ...): selezione singola o multipla, elenco estendibile
 * lato server — non una lista fissa come i mesi, quindi riceve i fornitori come prop. */
export function MultiSelectFornitori({ fornitori, selezionati, onChange, className = "" }: MultiSelectFornitoriProps) {
  const [aperto, setAperto] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClickFuori(e: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setAperto(false);
      }
    }
    document.addEventListener("mousedown", handleClickFuori);
    return () => document.removeEventListener("mousedown", handleClickFuori);
  }, []);

  function toggleFornitore(id: number) {
    if (selezionati.includes(id)) {
      onChange(selezionati.filter((f) => f !== id));
    } else {
      onChange([...selezionati, id].sort((a, b) => a - b));
    }
  }

  const nomiSelezionati = fornitori.filter((f) => selezionati.includes(f.id)).map((f) => f.nome);
  const etichetta =
    selezionati.length === 0
      ? "Tutti i fornitori"
      : selezionati.length <= 2
        ? nomiSelezionati.join(", ")
        : `${selezionati.length} fornitori selezionati`;

  return (
    <div ref={containerRef} className={`relative ${className}`}>
      <button
        type="button"
        onClick={() => setAperto((a) => !a)}
        className="w-full truncate rounded-md border border-zinc-300 bg-white px-3 py-2 text-left text-sm hover:bg-zinc-50"
      >
        {etichetta}
      </button>

      {aperto && (
        <div className="absolute z-20 mt-1 w-52 rounded-md border border-zinc-200 bg-white p-2 shadow-lg">
          <div className="mb-2 flex justify-between border-b border-zinc-100 pb-2 text-xs">
            <button
              type="button"
              className="text-teal-700 hover:underline"
              onClick={() => onChange(fornitori.map((f) => f.id))}
            >
              Tutti
            </button>
            <button type="button" className="text-teal-700 hover:underline" onClick={() => onChange([])}>
              Nessuno
            </button>
          </div>
          <div className="max-h-64 overflow-y-auto">
            {fornitori.length === 0 ? (
              <p className="px-1 py-1 text-xs text-zinc-400">Nessun fornitore.</p>
            ) : (
              fornitori.map((f) => (
                <label key={f.id} className="flex items-center gap-2 rounded px-1 py-1 text-sm hover:bg-zinc-50">
                  <input type="checkbox" checked={selezionati.includes(f.id)} onChange={() => toggleFornitore(f.id)} />
                  {f.nome}
                </label>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
