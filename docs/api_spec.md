## API - wstępny szkic

Auth
- POST /api/auth/register
  - body: { email, password, name }
  - returns: 201 Created

- POST /api/auth/login
  - body: { email, password }
  - returns: { accessToken (JWT), refreshToken }

- POST /api/auth/refresh
  - body: { refreshToken }
  - returns: { accessToken, refreshToken }

- POST /api/auth/revoke
  - body: { refreshToken }
  - auth: Bearer (or allow anonymous to revoke by token)
  - returns: 200 OK

Users
- GET /api/users/me
  - auth: Bearer
  - returns: user profile

Measurements
- POST /api/measurements
  - auth: Bearer
  - body: { timestamp, heartRate, systolic, diastolic, activityMinutes, notes }
  - returns: created measurement

- GET /api/measurements?from=&to=&page=&pageSize=
  - auth: Bearer
  - returns: paged list

Stats
- GET /api/stats/summary?from=&to=
  - auth: Bearer
  - returns: aggregated stats (avg heart rate, avg BP, total activity, simple riskScore and recommendations)

Example response:
{
  "count": 12,
  "averageHeartRate": 72.5,
  "averageSystolic": 125.3,
  "averageDiastolic": 78.1,
  "totalActivityMinutes": 320,
  "averageWeeklyActivity": 120.0,
  "riskScore": 25,
  "riskCategory": "Low",
  "recommendations": ["..."]
}

Admin
- (Opcjonalnie) zarządzanie użytkownikami i treściami edukacyjnymi

Bezpieczeństwo
- Wszystkie wrażliwe endpointy chronione JWT
- Hasła przechowywane w postaci bezpiecznego hasha (np. PBKDF2/BCrypt)
