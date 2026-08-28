import { formattaValuta } from "@/lib/format";

interface Punto {
  mese: number;
  valore: number;
}

const MESI_BREVI = ["Gen", "Feb", "Mar", "Apr", "Mag", "Giu", "Lug", "Ago", "Set", "Ott", "Nov", "Dic"];

export function BarChart({ dati }: { dati: Punto[] }) {
  const perMese = new Map(dati.map((d) => [d.mese, d.valore]));
  const valori = Array.from({ length: 12 }, (_, i) => perMese.get(i + 1) ?? 0);
  const massimo = Math.max(...valori, 1);

  return (
    <div className="flex h-56 items-end gap-2">
      {valori.map((valore, i) => (
        <div key={i} className="group relative flex flex-1 flex-col items-center gap-1.5">
          <div
            className="w-full rounded-t bg-teal-600 transition-all group-hover:bg-teal-700"
            style={{ height: `${Math.max((valore / massimo) * 180, valore > 0 ? 4 : 0)}px` }}
            title={formattaValuta(valore)}
          />
          <span className="text-[11px] text-zinc-500">{MESI_BREVI[i]}</span>
          {valore > 0 && (
            <span className="pointer-events-none absolute -top-6 rounded bg-zinc-900 px-1.5 py-0.5 text-[10px] text-white opacity-0 transition-opacity group-hover:opacity-100">
              {formattaValuta(valore)}
            </span>
          )}
        </div>
      ))}
    </div>
  );
}
