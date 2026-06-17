# Heart Disease Prevention Web App — Full Analysis & Implementation Prompts

## Project Overview

**Title (PL):** Webowa aplikacja wspierająca profilaktykę chorób układu krążenia  
**Title (EN):** Web Application Supporting Cardiovascular Disease Prevention

**Goal:** A system consisting of a web application and database supporting cardiovascular disease prevention. Users can log in, input health parameters (heart rate, blood pressure, physical activity), view/analyze them over time via charts, and see simple risk indicators for educational purposes.

---

## Current Repository State

| Component | Status | Notes |
|-----------|--------|-------|
| Frontend (Angular 21.2) | Scaffolded | Empty shell — no features, no routes, no components |
| Backend (ASP.NET Core) | **Empty folder** | Nothing created yet |
| Database | **Not started** | No schema, no migrations |
| Authentication | **Not started** | No JWT setup |
| Docker/CI/CD | **Not started** | Optional |

**Frontend has:**
- Angular CLI 21.2.7, TypeScript 5.9.2, Vitest for testing
- SCSS support, Prettier configured
- Standalone component architecture (modern Angular)
- Empty routes, empty styles, only root `App` component with `<router-outlet />`

---

## Functional Requirements (from temat.txt)

1. User registration and login
2. Health parameter input (heart rate, blood pressure, physical activity)
3. Data visualization (charts, time-based trends)
4. Simple risk indicators (informational/educational character)
5. Measurement history and user statistics
6. (Optional) Admin panel for managing users and educational content

## Technical Requirements

- **Frontend:** Angular
- **Backend:** ASP.NET Core Web API
- **Database:** PostgreSQL or MS SQL Server
- **Security:** JWT authorization, basic user data protection
- **Optional:** Docker containerization, CI/CD pipeline

---

## Implementation Prompts (Step-by-Step)

---

### PHASE 1: Backend Foundation (ASP.NET Core + Database)

---

#### Step 1.1 — Create ASP.NET Core Web API Project

```
In the backend/ folder, create a new ASP.NET Core 8 Web API project called "HeartDiseasePreventionApi". 
Set up the following structure:
- Controllers/ folder
- Models/ folder  
- DTOs/ folder
- Services/ folder
- Data/ folder (for DbContext)
- Migrations/ folder

Add NuGet packages:
- Microsoft.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL (for PostgreSQL)
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Swashbuckle.AspNetCore (Swagger)

Configure Program.cs with:
- CORS policy allowing Angular dev server (http://localhost:4200)
- Swagger/OpenAPI
- JWT authentication placeholder
- Entity Framework with PostgreSQL connection string from appsettings.json
```

---

#### Step 1.2 — Define Database Models (Entity Framework)

```
In the backend project, create Entity Framework models in Models/ folder:

1. User (extends IdentityUser):
   - FirstName (string)
   - LastName (string)
   - DateOfBirth (DateTime)
   - Gender (enum: Male, Female, Other)
   - CreatedAt (DateTime)

2. HealthMeasurement:
   - Id (Guid, PK)
   - UserId (FK to User)
   - MeasurementDate (DateTime)
   - SystolicPressure (int?, mmHg)
   - DiastolicPressure (int?, mmHg)
   - HeartRate (int?, bpm)
   - PhysicalActivityMinutes (int?, minutes per day)
   - Weight (decimal?, kg)
   - Notes (string?, optional)
   - CreatedAt (DateTime)

3. RiskAssessment:
   - Id (Guid, PK)
   - UserId (FK to User)
   - AssessmentDate (DateTime)
   - RiskLevel (enum: Low, Medium, High)
   - RiskScore (decimal)
   - Recommendations (string)

Create ApplicationDbContext in Data/ folder with DbSets for all entities.
Configure relationships with Fluent API. Add indexes on UserId + MeasurementDate.
```

---

#### Step 1.3 — Implement JWT Authentication

```
Implement JWT authentication in the backend:

1. Create AuthController with endpoints:
   - POST /api/auth/register (email, password, firstName, lastName, dateOfBirth, gender)
   - POST /api/auth/login (email, password) → returns JWT token + refresh token
   - POST /api/auth/refresh-token

2. Create AuthService with methods:
   - RegisterAsync(RegisterDto) → returns success/error
   - LoginAsync(LoginDto) → returns TokenDto (accessToken, refreshToken, expiresAt)
   - RefreshTokenAsync(string refreshToken) → returns new TokenDto

3. Create DTOs:
   - RegisterDto, LoginDto, TokenDto, AuthResponseDto

4. Configure JWT in Program.cs:
   - Read secret key, issuer, audience from appsettings.json
   - Token expiration: 60 minutes
   - Refresh token expiration: 7 days

5. Add [Authorize] attribute to protected endpoints.
Use ASP.NET Core Identity for user management. Hash passwords with Identity's built-in hasher.
```

---

#### Step 1.4 — Create Health Measurements API

```
Create the Health Measurements CRUD API:

1. MeasurementsController (authorized):
   - GET /api/measurements — get all measurements for current user (paginated, sorted by date desc)
   - GET /api/measurements/{id} — get single measurement
   - GET /api/measurements/range?from={date}&to={date} — get measurements in date range
   - POST /api/measurements — create new measurement
   - PUT /api/measurements/{id} — update measurement
   - DELETE /api/measurements/{id} — delete measurement
   - GET /api/measurements/statistics — get aggregated stats (avg, min, max for each parameter over last 30/90/365 days)

2. Create DTOs:
   - CreateMeasurementDto, UpdateMeasurementDto, MeasurementResponseDto, MeasurementStatisticsDto

3. Create IMeasurementService and MeasurementService with business logic.

4. Add input validation:
   - Systolic pressure: 60-250 mmHg
   - Diastolic pressure: 40-150 mmHg
   - Heart rate: 30-220 bpm
   - Physical activity: 0-1440 minutes
   - Weight: 20-300 kg

5. Ensure users can only access their own data (filter by UserId from JWT claims).
```

---

#### Step 1.5 — Create Risk Assessment Endpoint

```
Create a simple cardiovascular risk assessment feature:

1. RiskController (authorized):
   - GET /api/risk/current — calculate current risk based on latest measurements
   - GET /api/risk/history — get risk assessment history

2. Create RiskService with a simple scoring algorithm:
   - Based on: blood pressure ranges, resting heart rate, physical activity level, age, gender
   - Use WHO/ESC simplified categories:
     * Blood pressure: Normal (<120/80), Elevated (120-129/<80), High Stage 1 (130-139/80-89), High Stage 2 (>=140/>=90)
     * Heart rate: Good (<70), Normal (70-80), Elevated (>80)
     * Activity: Active (>=150 min/week), Moderate (75-149), Sedentary (<75)
   - Combine into overall risk: Low, Medium, High
   - Generate text recommendations based on risk factors

3. Create RiskAssessmentDto with:
   - overallRisk (Low/Medium/High)
   - riskScore (0-100)
   - bloodPressureStatus, heartRateStatus, activityStatus
   - recommendations (string[])

Add disclaimer: "This is for informational purposes only. Not a medical diagnosis."
```

---

#### Step 1.6 — Database Migration & Seed Data

```
Set up Entity Framework migrations:

1. Create initial migration for all models
2. Add a seed data method in ApplicationDbContext.OnModelCreating or a separate DataSeeder class:
   - Create a demo admin user (admin@example.com)
   - Add sample educational content
3. Update appsettings.json and appsettings.Development.json with:
   - PostgreSQL connection string (localhost:5432, database: heart_disease_db)
   - JWT settings (secret key, issuer, audience)
4. Add a docker-compose.yml at the project root with PostgreSQL service for local development
```

---

### PHASE 2: Frontend Core (Angular)

---

#### Step 2.1 — Angular Project Setup (Routing, HTTP, Guards)

```
Set up the Angular frontend infrastructure:

1. Install packages:
   - @angular/material (UI components)
   - chart.js + ng2-charts (data visualization)
   - @angular/cdk

2. Update app.config.ts:
   - Add provideHttpClient(withInterceptors([authInterceptor]))
   - Add provideAnimations()

3. Create folder structure under src/app/:
   - core/ (guards, interceptors, services)
   - features/ (login, register, dashboard, measurements, risk)
   - shared/ (components, pipes, models)

4. Create auth.interceptor.ts:
   - Attach JWT token from localStorage to Authorization header
   - Handle 401 responses (redirect to login)

5. Create auth.guard.ts:
   - CanActivate guard checking if token exists and is not expired

6. Set up routes in app.routes.ts:
   - '' → redirect to '/dashboard'
   - 'login' → LoginComponent
   - 'register' → RegisterComponent
   - 'dashboard' → DashboardComponent (guarded)
   - 'measurements' → MeasurementsComponent (guarded)
   - 'measurements/add' → AddMeasurementComponent (guarded)
   - 'risk' → RiskAssessmentComponent (guarded)
   - 'history' → HistoryComponent (guarded)

7. Create core/services/auth.service.ts:
   - login(email, password): Observable<TokenResponse>
   - register(dto): Observable<any>
   - logout(): void
   - isAuthenticated(): boolean
   - getToken(): string

8. Create core/services/api.service.ts:
   - Base URL from environment
   - Generic CRUD methods
```

---

#### Step 2.2 — Authentication Pages (Login + Register)

```
Create Angular authentication pages:

1. LoginComponent (features/auth/login/):
   - Angular Material card layout, centered on page
   - Reactive form with email + password fields
   - "Remember me" checkbox
   - "Don't have an account? Register" link
   - Form validation (required, email format, min password length 6)
   - On submit: call AuthService.login(), store token, navigate to /dashboard
   - Show error message on invalid credentials (mat-snackbar)

2. RegisterComponent (features/auth/register/):
   - Angular Material card layout
   - Reactive form: email, password, confirmPassword, firstName, lastName, dateOfBirth, gender
   - Validation: passwords match, email format, all required fields
   - On submit: call AuthService.register(), show success, navigate to /login
   - "Already have an account? Login" link

3. Style both with a clean medical/health theme:
   - Soft blue/green color palette
   - Heart/health icon in header
   - Responsive design (works on mobile)
```

---

#### Step 2.3 — Dashboard Page

```
Create the main Dashboard component (features/dashboard/):

1. Layout with Angular Material:
   - Top navigation bar with: app title, user name, navigation links (Dashboard, Measurements, Risk, History), logout button
   - Create a shared LayoutComponent with sidenav for navigation

2. Dashboard content:
   - Welcome message with user's first name
   - Quick summary cards (mat-card):
     * Latest blood pressure reading
     * Latest heart rate
     * Activity this week (total minutes)
     * Current risk level (color-coded: green/yellow/red)
   - Line chart showing last 7 days of heart rate (using ng2-charts)
   - "Add Measurement" quick action button (FAB)
   - Last 5 measurements in a compact list

3. Create a MeasurementService (core/services/measurement.service.ts):
   - getMeasurements(page, pageSize): Observable<PaginatedResult<Measurement>>
   - getMeasurement(id): Observable<Measurement>
   - createMeasurement(dto): Observable<Measurement>
   - updateMeasurement(id, dto): Observable<Measurement>
   - deleteMeasurement(id): Observable<void>
   - getStatistics(): Observable<MeasurementStatistics>
   - getMeasurementsInRange(from, to): Observable<Measurement[]>
```

---

#### Step 2.4 — Measurement Input Form

```
Create the Add/Edit Measurement component (features/measurements/):

1. AddMeasurementComponent:
   - Angular Material form with sections:
     * Blood Pressure: systolic (number input, placeholder "120") + diastolic (number input, placeholder "80") with "mmHg" suffix
     * Heart Rate: number input with "bpm" suffix
     * Physical Activity: number input with "minutes today" suffix
     * Weight: number input with "kg" suffix
     * Date: mat-datepicker (default: today)
     * Notes: textarea (optional)
   - All health fields are optional (user can fill only what they measured)
   - Client-side validation matching backend rules
   - On submit: call MeasurementService.create(), show success snackbar, navigate to /measurements
   - Cancel button returns to measurements list

2. MeasurementsListComponent:
   - Material table with columns: Date, Systolic/Diastolic, Heart Rate, Activity, Weight
   - Sorting by date
   - Pagination (10 per page)
   - Edit and Delete action buttons per row
   - Delete confirmation dialog
   - "Add New" button in toolbar
   - Date range filter at the top
```

---

#### Step 2.5 — Data Visualization (Charts)

```
Create data visualization components using ng2-charts (Chart.js):

1. Create shared/components/health-chart/:
   - A reusable chart component that accepts:
     * data: Measurement[]
     * metric: 'bloodPressure' | 'heartRate' | 'activity' | 'weight'
     * timeRange: '7days' | '30days' | '90days' | '1year'
   - Renders a line chart with:
     * X-axis: dates
     * Y-axis: metric values
     * For blood pressure: two lines (systolic + diastolic)
     * Color-coded zones (normal range highlighted in green background)

2. Create features/history/ page:
   - Tab group with tabs: Blood Pressure, Heart Rate, Activity, Weight
   - Each tab shows the chart for that metric
   - Time range selector (buttons: 7D, 30D, 90D, 1Y)
   - Statistics below chart: average, min, max, trend (up/down/stable)
   - Export to CSV button (optional)

3. Style charts:
   - Responsive (fill container width)
   - Tooltip showing exact values on hover
   - Reference lines for normal ranges (e.g., 120/80 for BP)
```

---

#### Step 2.6 — Risk Assessment Page

```
Create the Risk Assessment component (features/risk/):

1. RiskAssessmentComponent:
   - Call GET /api/risk/current on init
   - Display overall risk level as a large colored badge:
     * Low → green circle with checkmark
     * Medium → yellow/orange circle with warning icon
     * High → red circle with alert icon
   - Risk score gauge/meter visualization (0-100 scale)
   - Breakdown section showing individual factors:
     * Blood Pressure status (with icon + color)
     * Heart Rate status
     * Physical Activity status
   - Recommendations section:
     * List of actionable recommendations as mat-list items with icons
   - Prominent disclaimer banner:
     "⚠️ This assessment is for informational and educational purposes only. 
      It does not constitute medical advice. Please consult a healthcare professional."
   - "History" section showing risk assessments over time (small line chart of risk score)

2. Create RiskService (core/services/risk.service.ts):
   - getCurrentRisk(): Observable<RiskAssessment>
   - getRiskHistory(): Observable<RiskAssessment[]>
```

---

### PHASE 3: Polish & Optional Features

---

#### Step 3.1 — Global Styling and Responsive Design

```
Apply global styling to the Angular app:

1. In styles.scss:
   - Import Angular Material theme (custom theme with primary: blue-600, accent: teal-500)
   - Set base font: Roboto
   - Add CSS variables for health colors:
     --color-normal: #4caf50
     --color-warning: #ff9800
     --color-danger: #f44336
   - Responsive breakpoints (mobile: <768px, tablet: 768-1024px, desktop: >1024px)

2. Navigation layout:
   - Desktop: sidebar navigation
   - Mobile: bottom navigation bar or hamburger menu
   - All pages should be usable on mobile (forms stack vertically)

3. Add loading spinners for API calls (mat-progress-bar at top)
4. Add empty state illustrations when no data exists
5. Add Angular Material typography classes
```

---

#### Step 3.2 — Admin Panel (Optional)

```
Create an optional admin panel:

1. Backend:
   - Add "Admin" role to Identity
   - AdminController (requires Admin role):
     * GET /api/admin/users — list all users (paginated)
     * PUT /api/admin/users/{id}/role — change user role
     * DELETE /api/admin/users/{id} — deactivate user
     * GET /api/admin/statistics — system-wide stats (total users, measurements today, etc.)

2. Frontend:
   - Admin route (guarded with role check)
   - Users management table (search, paginate, edit role, deactivate)
   - System statistics dashboard (total users, active today, measurements count)

3. Add role-based navigation (show Admin link only for admin users)
```

---

#### Step 3.3 — Docker & CI/CD (Optional)

```
Add Docker containerization and CI/CD:

1. Create backend/Dockerfile:
   - Multi-stage build (SDK for build, ASP.NET runtime for run)
   - Expose port 5000

2. Create frontend/Dockerfile:
   - Multi-stage build (Node for build, nginx for serve)
   - nginx.conf for SPA routing

3. Create docker-compose.yml at project root:
   - Services: frontend (port 80), backend (port 5000), postgres (port 5432)
   - Volumes for postgres data persistence
   - Environment variables for connection strings

4. Create .github/workflows/ci.yml:
   - Trigger on push to main and PRs
   - Jobs: 
     * Backend: restore, build, test
     * Frontend: npm install, lint, test, build
   - Optional: build Docker images
```

---

### PHASE 4: Testing & Documentation

---

#### Step 4.1 — Backend Unit & Integration Tests

```
Add tests to the backend:

1. Create a Tests project (xUnit):
   - Unit tests for RiskService (test scoring algorithm with various inputs)
   - Unit tests for MeasurementService (validation logic)
   - Integration tests for AuthController (register, login, token refresh)
   - Integration tests for MeasurementsController (CRUD operations)
   - Use InMemory database for tests

2. Test scenarios:
   - Registration with valid/invalid data
   - Login with correct/incorrect credentials
   - Creating measurement with valid/out-of-range values
   - Risk calculation with edge cases (no data, partial data, all data)
   - Authorization (accessing other user's data should return 403)
```

---

#### Step 4.2 — Frontend Tests

```
Add frontend tests:

1. Unit tests (vitest):
   - AuthService: test login/logout/token storage
   - MeasurementService: test API calls (mock HttpClient)
   - Risk component: test display for each risk level
   - Add measurement form: test validation

2. Component tests:
   - Login form: renders, validates, calls service
   - Dashboard: renders summary cards with mock data
   - Chart component: renders with sample data
```

---

#### Step 4.3 — README and Documentation

```
Update README.md with comprehensive documentation:

1. Project description (PL + EN)
2. Screenshots/mockup descriptions
3. Tech stack list
4. Prerequisites (Node.js, .NET 8 SDK, PostgreSQL)
5. Setup instructions:
   - Clone repo
   - Backend: dotnet restore, update connection string, dotnet ef database update, dotnet run
   - Frontend: npm install, ng serve
   - Docker: docker-compose up
6. API documentation (link to Swagger at /swagger)
7. Project structure overview
8. Environment variables reference
9. License
```

---

## Recommended Execution Order

| # | Step | Phase | Priority | Estimated Complexity |
|---|------|-------|----------|---------------------|
| 1 | 1.1 | Backend project scaffold | Critical | Low |
| 2 | 1.2 | Database models | Critical | Medium |
| 3 | 1.3 | JWT authentication | Critical | Medium |
| 4 | 1.4 | Measurements API | Critical | Medium |
| 5 | 1.5 | Risk assessment API | Critical | Medium |
| 6 | 1.6 | Migrations & seed data | Critical | Low |
| 7 | 2.1 | Frontend infrastructure | Critical | Medium |
| 8 | 2.2 | Auth pages | Critical | Medium |
| 9 | 2.3 | Dashboard | High | High |
| 10 | 2.4 | Measurements UI | High | Medium |
| 11 | 2.5 | Charts & visualization | High | High |
| 12 | 2.6 | Risk assessment UI | High | Medium |
| 13 | 3.1 | Global styling | Medium | Medium |
| 14 | 4.1 | Backend tests | Medium | Medium |
| 15 | 4.2 | Frontend tests | Medium | Medium |
| 16 | 4.3 | Documentation | Medium | Low |
| 17 | 3.2 | Admin panel | Low (optional) | High |
| 18 | 3.3 | Docker & CI/CD | Low (optional) | Medium |

---

## Remarks & Recommendations

### Architecture Decisions
- **Standalone components** — The Angular project already uses standalone components (no NgModules). Keep this pattern throughout.
- **PostgreSQL preferred** — Easier to containerize with Docker; better for production. MS SQL is an alternative if you prefer Windows-native.
- **.NET 8 LTS** — Use .NET 8 (Long Term Support) for stability.

### Security Notes
- Never store JWT secret in source code — use environment variables or user-secrets in development.
- Validate ALL input on both frontend AND backend (defense in depth).
- The risk assessment algorithm is **educational only** — add prominent disclaimers everywhere it appears.
- Use HTTPS in production.
- Implement rate limiting on auth endpoints to prevent brute force.

### Development Tips
- Start the backend first — the frontend depends on API contracts.
- Use Swagger UI (`/swagger`) to test backend endpoints before building frontend.
- Consider creating shared TypeScript interfaces matching backend DTOs early.
- Run `ng serve --proxy-config proxy.conf.json` to proxy `/api` calls to backend during development.

### For the Engineering Thesis (Praca Inżynierska)
- Document your architecture decisions (why Angular, why ASP.NET Core, why PostgreSQL).
- Include UML diagrams: class diagram (models), sequence diagram (auth flow), deployment diagram.
- Show database schema (ER diagram).
- Describe the risk algorithm with references to medical guidelines (WHO, ESC).
- Include screenshots of the final application.
- Write about testing methodology and results.
- Mention security measures implemented.

### What Could Impress the Committee
- Working Docker deployment (`docker-compose up` and everything runs)
- CI/CD pipeline with automated tests
- Responsive design that works on mobile
- Well-documented API (Swagger)
- Clean code with proper separation of concerns
- Unit test coverage on critical business logic (risk algorithm)
