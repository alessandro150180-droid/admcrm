import type { KpiDto } from "@/lib/types";
import { Badge, Card } from "@/components/ui";
import { formattaPercentuale } from "@/lib/format";

export function KpiCard({
  titolo, kpi, formatta,
}: { titolo: string; kpi: KpiDto; formatta: (v: number) => string }) {
  return (
    <Card>
      <p className="text-sm font-medium text-zinc-500">{titolo}</p>
      <p className="mt-1 text-2xl font-semibold text-zinc-900">{formatta(kpi.valoreCorrente)}</p>
      <div className="mt-2 flex items-center gap-2">
        <Badge tone={kpi.trendPositivo ? "green" : "red"}>
          {kpi.trendPositivo ? "▲" : "▼"} {formattaPercentuale(Math.abs(kpi.differenzaPercentuale))}
        </Badge>
        <span className="text-xs text-zinc-400">vs anno precedente</span>
      </div>
    </Card>
  );
}
