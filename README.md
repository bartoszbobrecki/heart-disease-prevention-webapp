Web Application Supporting Cardiovascular Disease Prevention

Temat (PL): Webowa aplikacja wspierająca profilaktykę chorób układu krążenia

Topic (EN): Web Application Supporting Cardiovascular Disease Prevention

Opis ogólny:
System składający się z aplikacji webowej (Angular frontend) oraz backendu (ASP.NET Core Web API) z bazą danych (PostgreSQL lub MS SQL Server). Umożliwia rejestrację/logowanie użytkowników, wprowadzanie pomiarów (tętno, ciśnienie, aktywność), przegląd historii, prostą ocenę ryzyka i wizualizacje.

Stack:
- Frontend: Angular
- Backend: ASP.NET Core Web API (C#)
- Baza danych: PostgreSQL (zalecane) lub MS SQL Server
- Autoryzacja: JWT
- (Opcjonalnie) Docker, CI/CD

Co dodałem:
- `docs/architecture.md` - opis architektury i przepływów
- `docs/api_spec.md` - wstępny spec API
- `db/schema.sql` - sugerowany schemat bazy danych (PostgreSQL)
- `docker-compose.yml` - kompozycja: backend + postgres
- `backend/` - prosty szkielet ASP.NET Core Web API (models, DbContext, kontrolery, csproj, appsettings)

Szybki start (backend):

1) Wymagania: zainstalowane .NET 6+ SDK (lub nowsze), opcjonalnie Docker.
2) Przejdź do folderu backend: `cd backend`.
3) Odbuduj pakiety: `dotnet restore`.
4) Uruchom aplikację: `dotnet run` (domyślnie nasłuchuje na http://localhost:5000 i https://localhost:5001).

Docker (opcjonalnie):
1) Uruchom: `docker-compose up --build` w katalogu głównym projektu.

Następne kroki (opcjonalne):
- Dodać frontend Angular (szablon, routing, serwisy HTTP, wykresy z Chart.js lub ngx-charts).
- Dodać szczegółowe endpointy i implementację logiki oceny ryzyka.
- Dodać testy jednostkowe i integracyjne.

Migrations (EF Core):

1) Przykład użycia skryptu PowerShell stworzonego w `backend/scripts/migrate.ps1`:

	```powershell
	cd backend\scripts
	.\migrate.ps1 -Action add -Name Initial
	.\migrate.ps1 -Action update
	```

2) Alternatywnie użyj poleceń `dotnet ef` bezpośrednio w katalogu `backend`:

	```powershell
	cd backend
	dotnet ef migrations add Initial
	dotnet ef database update
	```

Uwaga: Aby użyć `dotnet ef` lokalnie z poziomu PowerShell, zainstaluj globalnie narzędzie lub użyj pakietu EF Tools zawartego w projekcie.
