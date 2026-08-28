# ADMcrm — Frontend (Next.js 16)

Interfaccia web del gestionale ADMcrm: login, dashboard, clienti, ordini,
attività, obiettivi di vendita, import Excel, notifiche, audit log e
impostazioni (collegamento Google Calendar, cambio password).

## Stack

- Next.js 16 (App Router, Turbopack) + React 19 + TypeScript
- Tailwind CSS 4
- Nessuna libreria di data-fetching/state management: chiamate dirette
  all'API REST del backend (`lib/api.ts`) tramite `fetch`, con il token JWT
  salvato in `localStorage` (vedi `lib/auth-context.tsx`)

## Setup

```bash
npm install
```

Crea/verifica `.env.local` con l'URL del backend:

```
NEXT_PUBLIC_API_URL=http://localhost:5080
```

Il backend (`../backend`) deve essere avviato e raggiungibile su quell'URL —
vedi il README del backend per come avviarlo (richiede PostgreSQL).

## Sviluppo

```bash
npm run dev
```

Apri [http://localhost:3000](http://localhost:3000). Se non sei autenticato
vieni reindirizzato a `/login`.

## Build di produzione

```bash
npm run build
npm start
```

## Struttura

- `app/login/` — pagina di login (pubblica)
- `app/(app)/` — tutte le pagine autenticate, protette dal layout in
  `app/(app)/layout.tsx` (redirige a `/login` se non c'è un token valido)
- `lib/api.ts` — client HTTP tipizzato verso il backend, un metodo per ogni
  endpoint (`api.clienti.lista(...)`, `api.ordini.crea(...)`, ...)
- `lib/types.ts` — tipi TypeScript che rispecchiano i DTO del backend
- `components/ui.tsx` — componenti UI di base riutilizzati in tutte le pagine
  (Button, Table, Card, Badge, Pagination, ...)

## Nota su Next.js 16

Questo progetto usa Next.js 16, più recente di molte guide/esempi in giro:
prima di modificare pattern di routing o data-fetching, controlla
`node_modules/next/dist/docs/` per eventuali breaking change rispetto alle
versioni precedenti (vedi `AGENTS.md` nella root del progetto).
