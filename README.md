# Hospital Information System

Tento projekt predstavuje jednoduchý nemocničný informačný systém, ktorého cieľom je demonštrovať modernú webovú architektúru postavenú na platforme .NET 8 s využitím Blazor WebAssembly, ASP.NET Core Web API a reverse proxy.

Aplikácia umožňuje základné operácie ako registráciu a prihlásenie pacienta, pričom backend je oddelený od frontendu a komunikuje s databázou pomocou Entity Framework Core.

## Použité technológie

.NET 8

ASP.NET Core

Blazor WebAssembly

Entity Framework Core

PostgreSQL

YARP Reverse Proxy

Docker (pre databázu)

HTML, CSS

## Spustenie projektu

  - Spusti databázu (pomocou Docker)

  - Spusti projekt Hospital.Api

  - Spusti projekt Hospital.Web

  - Otvor prehliadač na adrese: https://localhost:7014

## Architektúra projektu

### Hospital.Web

- ASP.NET Core Blazor Web App

- Slúži ako hlavný vstupný bod aplikácie

- Hostuje Blazor WebAssembly klienta

- Obsahuje reverse proxy (YARP), ktorá presmerúva API požiadavky na backend

- Beží na https://localhost:7014

### Hospital.Web.Client

- Blazor WebAssembly (frontend)

- Beží v prehliadači používateľa

- Obsahuje UI komponenty, formuláre a klientsku logiku

- Komunikuje s backendom výhradne cez HTTP API (/api/...)

- Nepozná priamu adresu backendu

### Hospital.Api

- ASP.NET Core Web API

- Obsahuje aplikačnú logiku

- Komunikuje s databázou

- Implementuje autentifikáciu, registráciu a správu používateľov

- Beží na http://localhost:5103 (HTTP) a https://localhost:7183 (HTTPS)

### Databáza

- PostgreSQL

- Prístupná výhradne cez backend (Hospital.Api)

- Používa sa Entity Framework Core (Code First)



prihlasovanie:
  - lekar -> lekar@gmail.com
  - admin -> admin@gmail.com
  - pacient -> pacient@gmail.com
