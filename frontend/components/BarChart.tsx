import { formattaValuta } from "@/lib/format";

interface Punto {
  mese: number;
  anno: number;
  valore: number;
}

const MESI_BREVI = ["Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic"];

// Dal più recente (colore pieno) al più vecchio (via via più chiaro): al massimo 3 anni a confronto.
const COLORI_ANNO = ["bg-teal-600 group-hover:bg-teal-700", "bg-teal-300 group-hover:bg-teal-400", "bg-zinc-300 group-hover:bg-zinc-400"];

/** Grafico a colonne con confronto anno su anno: fino a 3 barre affiancate per mese, una per ogni anno in `anni` (dal più recente al più vecchio). */
export function BarChart({ dati, anni }: { dati: Punto[]; anni: number[] }) {
  const perAnnoMese = new Map(dati.map((d) => [`${d.anno}-${d.mese}`, d.valore]));
  const massimo = Math.max(...dati.map((d) => d.valore), 1);

  return (
    <div className="flex h-56 items-end gap-2">
      {Array.from({ length: 12 }, (_, i) => i + 1).map((mese) => (
        <div key={mese} className="flex flex-1 flex-col items-center gap-1.5">
          <div className="flex h-[180px] w-full items-end justify-center gap-0.5">
            {anni.map((anno, indiceAnno) => {
              const valore = perAnnoMese.get(`${anno}-${mese}`) ?? 0;
              return (
                <div key={anno} className="group relative flex-1">
                  <div
                    className={`w-full rounded-t transition-all ${COLORI_ANNO[indiceAnno] ?? "bg-zinc-200"}`}
                    style={{ height: `${Math.max((valore / massimo) * 180, valore > 0 ? 4 : 0)}px` }}
                  />
                  {valore > 0 && (
                    <span className="pointer-events-none absolute -top-6 left-1/2 -translate-x-1/2 whitespace-nowrap rounded bg-zinc-900 px-1.5 py-0.5 text-[10px] text-white opacity-0 transition-opacity group-hover:opacity-100">
                      {anno} — {formattaValuta(valore)}
                    </span>
                  )}
                </div>
              );
            })}
          </div>
          <span className="text-[11px] text-zinc-500">{MESI_BREVI[mese - 1]}</span>
        </div>
      ))}
    </div>
  );
}

export function LegendaAnni({ anni }: { anni: number[] }) {
  return (
    <div className="flex flex-wrap items-center gap-4 text-xs text-zinc-500">
      {anni.map((anno, i) => (
        <span key={anno} className="flex items-center gap-1.5">
          <span className={`inline-block h-2.5 w-2.5 rounded-sm ${(COLORI_ANNO[i] ?? "bg-zinc-200").split(" ")[0]}`} />
          {anno}
        </span>
      ))}
    </div>
  );
}
