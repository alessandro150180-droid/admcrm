export function formattaValuta(valore: number): string {
  return valore.toLocaleString("it-IT", { style: "currency", currency: "EUR" });
}

export function formattaData(iso: string | null | undefined): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("it-IT");
}

export function formattaDataOra(iso: string | null | undefined): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleString("it-IT");
}

export function formattaPercentuale(valore: number): string {
  return `${valore.toFixed(1)}%`;
}

export function formattaDimensioneFile(byte: number): string {
  if (byte < 1024) return `${byte} B`;
  if (byte < 1024 * 1024) return `${(byte / 1024).toFixed(0)} KB`;
  return `${(byte / (1024 * 1024)).toFixed(1)} MB`;
}

export const NOMI_MESI = [
  "Gennaio", "Febbraio", "Marzo", "Aprile", "Maggio", "Giugno",
  "Luglio", "Agosto", "Settembre", "Ottobre", "Novembre", "Dicembre",
];

export const ETICHETTE_STATO_ORDINE: Record<string, string> = {
  InAttesa: "In attesa",
  Confermato: "Confermato",
  InProduzione: "In produzione",
  Spedito: "Spedito",
  Consegnato: "Consegnato",
  Annullato: "Annullato",
};

export const ETICHETTE_STATO_ATTIVITA: Record<string, string> = {
  DaFare: "Da fare",
  InCorso: "In corso",
  Completata: "Completata",
  Annullata: "Annullata",
};

export const ETICHETTE_TIPO_ATTIVITA: Record<string, string> = {
  Telefonata: "Telefonata",
  Visita: "Visita",
  Preventivo: "Preventivo",
  FollowUp: "Follow-up",
  Reclamo: "Reclamo",
  Campionario: "Campionario",
  Email: "Email",
  Assistenza: "Assistenza",
};

export const ETICHETTE_PRIORITA: Record<string, string> = {
  Bassa: "Bassa",
  Media: "Media",
  Alta: "Alta",
  Urgente: "Urgente",
};

export function messaggioErrore(err: unknown): string {
  if (err instanceof Error) return err.message;
  return "Si è verificato un errore imprevisto.";
}
