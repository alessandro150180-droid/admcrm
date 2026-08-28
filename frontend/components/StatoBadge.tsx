import { Badge } from "@/components/ui";
import { ETICHETTE_STATO_ATTIVITA, ETICHETTE_STATO_ORDINE } from "@/lib/format";

const TONO_ORDINE: Record<string, "zinc" | "green" | "amber" | "red" | "blue"> = {
  InAttesa: "amber",
  Confermato: "blue",
  InProduzione: "blue",
  Spedito: "blue",
  Consegnato: "green",
  Annullato: "red",
};

export function StatoOrdineBadge({ stato }: { stato: string }) {
  return <Badge tone={TONO_ORDINE[stato] ?? "zinc"}>{ETICHETTE_STATO_ORDINE[stato] ?? stato}</Badge>;
}

const TONO_ATTIVITA: Record<string, "zinc" | "green" | "amber" | "red" | "blue"> = {
  DaFare: "amber",
  InCorso: "blue",
  Completata: "green",
  Annullata: "red",
};

export function StatoAttivitaBadge({ stato }: { stato: string }) {
  return <Badge tone={TONO_ATTIVITA[stato] ?? "zinc"}>{ETICHETTE_STATO_ATTIVITA[stato] ?? stato}</Badge>;
}
