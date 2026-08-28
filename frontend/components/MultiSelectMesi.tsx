"use client";

import { useEffect, useRef, useState } from "react";
import { NOMI_MESI } from "@/lib/format";

const NOMI_MESI_BREVI = ["Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic"];

interface MultiSelectMesiProps {
  mesiSelezionati: number[];
  onChange: (mesi: number[]) => void;
  className?: string;
}

export function MultiSelectMesi({ mesiSelezionati, onChange, className = "" }: MultiSelectMesiProps) {
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

  function toggleMese(mese: number) {
    if (mesiSelezionati.includes(mese)) {
      onChange(mesiSelezionati.filter((m) => m !== mese));
    } else {
      onChange([...mesiSelezionati, mese].sort((a, b) => a - b));
    }
  }

  const etichetta =
    mesiSelezionati.length === 0
      ? "Nessun mese"
      : mesiSelezionati.length === 12
        ? "Tutti i mesi"
        : mesiSelezionati.length <= 2
          ? mesiSelezionati.map((m) => NOMI_MESI_BREVI[m - 1]).join(", ")
          : `${mesiSelezionati.length} mesi selezionati`;

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
        <div className="absolute z-20 mt-1 w-48 rounded-md border border-zinc-200 bg-white p-2 shadow-lg">
          <div className="mb-2 flex justify-between border-b border-zinc-100 pb-2 text-xs">
            <button
              type="button"
              className="text-teal-700 hover:underline"
              onClick={() => onChange(Array.from({ length: 12 }, (_, i) => i + 1))}
            >
              Tutti
            </button>
            <button type="button" className="text-teal-700 hover:underline" onClick={() => onChange([])}>
              Nessuno
            </button>
          </div>
          <div className="max-h-64 overflow-y-auto">
            {NOMI_MESI.map((nome, i) => (
              <label key={nome} className="flex items-center gap-2 rounded px-1 py-1 text-sm hover:bg-zinc-50">
                <input type="checkbox" checked={mesiSelezionati.includes(i + 1)} onChange={() => toggleMese(i + 1)} />
                {nome}
              </label>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
