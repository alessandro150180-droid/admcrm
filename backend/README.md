# CucineCRM — Backend (ASP.NET Core 9 / Clean Architecture)

API REST per la gestione della rete vendita di un'azienda produttrice di cucine.

## Struttura della solution

```
backend/
├── src/
│   ├── CucineCRM.Domain          → entità ed enum, zero dipendenze esterne
│   ├── CucineCRM.Application     → interfacce, DTO, servizi (logica di business)
│   ├── CucineCRM.Infrastructure  → EF Core, PostgreSQL, JWT, BCrypt, repository
│   └── CucineCRM.API             → controller, Program.cs, Swagger
├── tests/
│   ├── CucineCRM.UnitTests
│   └── CucineCRM.IntegrationTests
├── docker-compose.yml            → PostgreSQL + pgAdmin per lo sviluppo locale
├── Dockerfile                    → build/run dell'API
└── CucineCRM.sln
```

Il flusso delle dipendenze rispetta la Clean Architecture:
`API → Infrastructure → Application → Domain` (mai il contrario).

## Permessi e ruoli

| Ruolo | Visibilità dati |
|---|---|
| Amministratore | Tutto |
| Direttore Commerciale | Tutto |
| Area Manager | Solo gli agenti con `AreaManagerId` = proprio Agente |
| Agente | Solo se stesso |

La regola è centralizzata in un unico punto: `IDataScopingService`
(`src/CucineCRM.Application/Services/DataScopingService.cs`). Ogni nuovo
controller/servizio deve richiamarlo invece di reimplementare i controlli —
così i permessi restano coerenti in tutta l'applicazione.

## Prerequisiti locali

- [.NET SDK 9](https://dotnet.microsoft.com/download/dotnet/9.0)
- Docker Desktop (per PostgreSQL) oppure un'istanza PostgreSQL 16 già disponibile
- (Opzionale) `dotnet-ef` come tool globale: `dotnet tool install --global dotnet-ef`

## Setup — primo avvio

### 1. Avvia PostgreSQL

```bash
cd backend
docker compose up -d postgres
```

Questo crea un database `cucinecrm_dev` su `localhost:5432` con utente/password
`postgres`/`postgres` (vedi `appsettings.Development.json` — **da cambiare
prima di andare in produzione**, insieme al `Jwt:SecretKey`).

### 2. Ripristina i pacchetti e compila

```bash
dotnet restore
dotnet build
```

### 3. Genera ed applica la migration iniziale

Le entità e le configurazioni EF Core sono già scritte; la migration iniziale
va generata in locale (richiede l'SDK .NET, non incluso nell'ambiente in cui
è stato scritto questo codice):

```bash
dotnet ef migrations add InitialCreate \
  --project src/CucineCRM.Infrastructure \
  --startup-project src/CucineCRM.API

dotnet ef database update \
  --project src/CucineCRM.Infrastructure \
  --startup-project src/CucineCRM.API
```

In alternativa, in ambiente di **sviluppo** l'API applica automaticamente le
migration pendenti all'avvio (vedi `Program.cs`), quindi il secondo comando
è opzionale se lanci subito `dotnet run`.

### 4. Avvia l'API

```bash
dotnet run --project src/CucineCRM.API
```

Swagger UI: `https://localhost:<porta>/swagger`

### 5. Crea il primo utente Amministratore

Non essendoci ancora un endpoint di self-registration (scelta voluta, per
sicurezza), il primo utente va creato con un piccolo script SQL oppure
temporaneamente aprendo l'endpoint `POST /api/auth/utenti` con la policy
`[AllowAnonymous]` finché non esiste il primo Amministratore. Consigliato:
inserimento diretto via SQL con password già hashata con BCrypt (work factor 12).

## Test

```bash
dotnet test
```

- `CucineCRM.UnitTests`: logica di business isolata (AuthService, DataScopingService...)
  con Moq + FluentAssertions.
- `CucineCRM.IntegrationTests`: chiamate HTTP end-to-end sull'API reale con
  `WebApplicationFactory`, database sostituito con EF Core InMemory.

## Docker (solo API)

```bash
docker build -t cucinecrm-api -f Dockerfile .
docker run -p 8080:8080 --env ConnectionStrings__DefaultConnection="Host=host.docker.internal;..." cucinecrm-api
```

Il `docker-compose.yml` verrà esteso per includere anche l'API e il frontend
Next.js in un'unica orchestrazione quando affronteremo quella fase.

## Cosa c'è già

- [x] Domain: 10 entità, 5 enum, soft-delete + audit automatico
- [x] JWT + BCrypt + 4 ruoli con policy di autorizzazione
- [x] Repository Pattern + Unit of Work
- [x] Scoping dati centralizzato per ruolo (`IDataScopingService`)
- [x] Controller: Auth, Agenti, Clienti (+ Note), Ordini, Attività/CRM, Obiettivi di Vendita, Dashboard/KPI, Importazioni (Excel), Notifiche, Audit Log, Google Calendar
- [x] Import Excel ordini: parsing (.xlsx), validazione, deduplica per `RiferimentoEsterno`, log dettagliato per riga
- [x] Audit log automatico (Creazione/Modifica/Eliminazione) via interceptor su `ApplicationDbContext.SaveChangesAsync`
- [x] Notifiche in-app (generazione da attività scadute, letto/non letto)
- [x] Export CSV (Clienti, Ordini) ed export PDF (scheda cliente, via QuestPDF)
- [x] Integrazione Google Calendar: flusso OAuth 2.0 completo (connetti/callback, refresh token) e sincronizzazione Attività → evento calendario — **codice pronto ma richiede credenziali reali**, vedi sezione dedicata sotto
- [x] Swagger con supporto Bearer JWT
- [x] CORS pronto per il frontend Next.js (`http://localhost:3000`)
- [x] Test di esempio (unit + integration)
- [x] Docker/Dockerfile per l'API
- [x] Migration iniziale generata e verificata (`InitialCreate` + `AuditNotificheGoogleCalendar`), applicata a Postgres locale
- [x] Frontend Next.js 16 (cartella `../frontend`): login, dashboard, clienti, ordini, attività, obiettivi, import, notifiche, audit log, impostazioni

## Configurare Google Calendar

L'integrazione è scritta e funzionante lato codice, ma richiede credenziali OAuth reali che
solo tu puoi generare (non possono essere create automaticamente):

1. Vai su [Google Cloud Console](https://console.cloud.google.com/), crea (o riusa) un progetto.
2. Abilita la **Google Calendar API** (menu "API e servizi" → "Libreria").
3. In "API e servizi" → "Credenziali", crea un **OAuth 2.0 Client ID** di tipo "Applicazione web".
4. Aggiungi come **URI di reindirizzamento autorizzato** esattamente l'URL configurato in
   `GoogleOAuth:RedirectUri` (default: `http://localhost:5080/api/google-calendar/callback`
   — aggiornalo se l'API gira su una porta/host diversi).
5. Copia Client ID e Client Secret in `appsettings.Development.json` (o in User Secrets /
   variabili d'ambiente per produzione), sostituendo i placeholder `CHANGE_ME_...` nella
   sezione `GoogleOAuth` di `appsettings.json`.

Da quel momento, ogni utente collega il proprio account da **Impostazioni** nel frontend.

## Cosa manca (prossimi passi)

- [ ] StoricoKPI: job/endpoint per popolare la tabella di aggregazione mensile (oggi la dashboard calcola i KPI on-the-fly dagli Ordini)
- [ ] Export in formato Excel (oggi solo CSV/PDF)
- [ ] Business Intelligence avanzata (clienti inattivi, top 20, previsioni)
- [ ] Caching (es. output caching sulle dashboard) e indicizzazione avanzata
- [ ] Assegnazione di un'attività a un utente diverso da chi la crea (oggi sempre auto-assegnata)
