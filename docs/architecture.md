## Architektura systemu

1) Frontend (Angular)
   - Aplikacja jednostronicowa (SPA) zarządzająca interakcją z użytkownikiem.
   - Komponenty: logowanie/rejestracja, dashboard, formularz pomiarów, historia, widok edukacyjny.
   - Korzysta z API backendu (HTTPS + JWT bearer).

2) Backend (ASP.NET Core Web API)
   - Autoryzacja: JWT
   - Modele: User, Measurement (pomiary), EducationalContent (opcjonalnie)
   - Endpoints: /api/auth, /api/users, /api/measurements, /api/stats, /api/admin
   - Warstwa dostępu do danych: Entity Framework Core (Postgres/SQL Server)

3) Baza danych
   - Przechowuje użytkowników, hasła (hash + salt), tokeny odświeżania (opcjonalnie), historię pomiarów.

4) Integracje i wdrożenie
   - Docker Compose z backendem i Postgres
   - CI/CD (GitHub Actions / Azure DevOps) - budowanie obrazu, migrowanie bazy, wdrożenie
