# TaskBoard — Practical Technical Assessment

**Time limit:** 1.5 hours
**Format:** AI-first development
**Goal:** Demonstrate your ability to debug, fix, and extend a full-stack application

---

## System Overview

TaskBoard is a team task management system with the following architecture:

| Layer | Technology |
|-------|-----------|
| Backend API | .NET 8, MediatR (CQRS), Dapper, FluentValidation, AutoMapper |
| Frontend | Angular 17, standalone components, NgRx (Store + Effects), Angular Material |
| Database | SQL Server LocalDB `(localdb)\MSSQLLocalDB`, database name `TaskBoard` |
| Auth | JWT tokens with BCrypt password hashing, session validation middleware |

### Key Architectural Patterns

- **CQRS via MediatR**: Commands and queries are separate request types handled by dedicated handlers
- **Dapper with raw SQL**: SQL files live in `src/TaskBoard.Infrastructure/Sql/` and are loaded at startup
- **NgRx**: Angular state management with actions → effects → reducers → selectors
- **AutoMapper**: Domain entities map to DTOs via profiles in `TaskBoard.Api.Logic/Mappings/`
- **Session validation**: Each JWT is tied to a server-side session stored in an in-memory Redis-like cache (`InMemoryRedis`)

---

## Startup Guide

### Prerequisites
- .NET 8 SDK
- Node.js 18+ with npm
- SQL Server LocalDB (included with Visual Studio or installable separately)

### Database Setup
```bash
# From the repository root
sqlcmd -S "(localdb)\MSSQLLocalDB" -i setup-database.sql
```

### Backend
```bash
cd src/TaskBoard.Api
dotnet run
# API starts at http://localhost:5000
```

### Frontend
```bash
cd taskboard-ui
npm install
ng serve
# UI starts at http://localhost:4200
```

### Test Accounts

All accounts use password: **`Password123!`**

| Email | Name | Role |
|-------|------|------|
| admin@taskboard.local | Alice Admin | Admin |
| manager@taskboard.local | Bob Manager | Manager |
| member1@taskboard.local | Charlie Member | Member |
| member2@taskboard.local | Diana Developer | Member |

---

## Architecture & Request Flow

### Solution Structure

```
TaskBoard/
├── src/
│   ├── TaskBoard.Api/                    # ASP.NET host, controllers, middleware
│   │   ├── Controllers/                  # REST endpoints (thin — delegate to MediatR)
│   │   ├── Middleware/                    # GlobalExceptionMiddleware, SessionValidationMiddleware
│   │   └── Program.cs                    # DI composition root, pipeline config
│   │
│   ├── TaskBoard.Api.Logic/              # Application layer (no DB access)
│   │   ├── Requests/                     # MediatR commands & queries + handlers
│   │   │   ├── Auth/                     # Login, Register
│   │   │   ├── Dashboard/                # Dashboard aggregation
│   │   │   ├── Projects/                 # CRUD, members
│   │   │   └── Tasks/                    # CRUD, assign/unassign
│   │   ├── Models/                       # DTOs returned by API
│   │   ├── Mappings/                     # AutoMapper profiles (Entity → DTO)
│   │   └── Validators/                   # FluentValidation rules
│   │
│   ├── TaskBoard.Domain/                 # Entities, events — no dependencies
│   │   ├── Entities/                     # TaskItem, Project, User, TaskComment, etc.
│   │   └── Events/                       # Domain events (TaskCreated, TaskUpdated, etc.)
│   │
│   ├── TaskBoard.Infrastructure/         # Data access & external services
│   │   ├── Repositories/                 # Dapper repositories (one per aggregate)
│   │   ├── Sql/                          # Raw .sql files loaded at startup
│   │   ├── Cache/                        # InMemoryRedis, SessionStore, QueueService
│   │   ├── Auth/                         # JWT token service, password hasher
│   │   └── Events/                       # DomainEventPublisher → outbox table
│   │
│   └── TaskBoard.NotificationConsumer/   # Background worker (hosted service)
│       ├── Workers/                      # DomainEventWorker (polls outbox), ReportGeneratorWorker
│       ├── Handlers/                     # Event-specific handlers (Created, Updated, Assigned)
│       └── Services/                     # Notification delivery strategies, deduplication
│
├── taskboard-ui/                         # Angular 17 SPA
│   └── src/app/
│       ├── core/services/                # HTTP services (AuthService, TaskService, etc.)
│       ├── store/                        # NgRx: actions, effects, reducers, selectors
│       └── features/                     # Page components (dashboard, projects, tasks, etc.)
│
└── setup-database.sql                    # Schema + seed data
```

### API Request Lifecycle

```
Browser (Angular)
    │
    ▼
┌─────────────────────────────────────────────────────┐
│  ASP.NET Pipeline                                   │
│                                                     │
│  1. CORS Middleware                                 │
│  2. GlobalExceptionMiddleware (catch-all → 500)     │
│  3. SessionValidationMiddleware                     │
│       → reads JWT "sessionId" claim                 │
│       → looks up session in InMemoryRedis           │
│       → rejects if missing / expired                │
│  4. Authentication / Authorization                  │
│  5. Controller                                      │
│       → thin: extracts route params, calls MediatR  │
└──────────────┬──────────────────────────────────────┘
               │  _mediator.Send(command)
               ▼
┌─────────────────────────────────────────────────────┐
│  Api.Logic — MediatR Handler                        │
│                                                     │
│  • Validates input (FluentValidation pipeline)      │
│  • Orchestrates business logic                      │
│  • Calls repository methods                         │
│  • Publishes domain events → outbox table           │
│  • Maps entity → DTO (AutoMapper)                   │
│  • Returns DTO to controller                        │
└──────────────┬──────────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────────────────────────┐
│  Infrastructure — Repository                        │
│                                                     │
│  • Loads SQL from .sql file                         │
│  • Opens SqlConnection (Dapper)                     │
│  • Executes parameterized query                     │
│  • Supports SqlTransaction pass-through             │
└─────────────────────────────────────────────────────┘
```

### Background Event Processing

```
API Handler
    │  PublishAsync(domainEvent, transaction)
    ▼
┌──────────────────────┐
│  DomainEventOutbox   │  (SQL table — transactional with main operation)
│  Status: Pending     │
└──────────┬───────────┘
           │  polled every 2s
           ▼
┌──────────────────────────────────────────────────┐
│  DomainEventWorker  (BackgroundService)          │
│                                                  │
│  1. Fetch pending events (batch of 50)           │
│  2. Mark as "Processing"                         │
│  3. Route to handler by EventName:               │
│       TaskCreated  → TaskCreatedEventHandler     │
│       TaskUpdated  → TaskUpdatedEventHandler     │
│       TaskAssigned → TaskAssignedEventHandler    │
│  4. Handler runs deduplication (SHA256 hash)     │
│  5. Delivers notification via strategy:          │
│       InApp  → inserts into UserNotifications    │
│       Email  → logs (stub)                       │
│  6. Mark as "Processed" (or retry on failure)    │
└──────────────────────────────────────────────────┘
```

### Frontend State Flow (NgRx)

```
User action (click, submit)
    │
    ▼
Component  →  dispatch(action)
                    │
                    ▼
              Effect (listens for action)
                    │  calls HTTP service
                    ▼
              API response
                    │
                    ▼
              dispatch(successAction / failureAction)
                    │
                    ▼
              Reducer  →  updates Store state
                    │
                    ▼
              Selector  →  Component re-renders
```

---

## Your Tasks

The application has been intentionally broken in several ways. Your job is to find and fix these issues, then implement new features. The tasks are ordered by difficulty — start from the top.

---

### Phase 1: Build Errors

The backend does not compile. Run `dotnet build` from the solution root and fix all compilation errors.

There are **2 build errors** to fix. The compiler output tells you what's wrong — use it.

---

### Phase 2: Runtime Errors

Once the app compiles and starts, several features are broken at runtime. Below are the symptoms you will encounter during normal use of the application.

#### R1 — The frontend cannot reach the API at all

After starting both the backend and frontend, every API call from the Angular app fails immediately. The browser console shows CORS errors. The API itself works fine when called directly (e.g., via Swagger or curl).

#### R2 — Login succeeds but every subsequent API call returns 401

After logging in with valid credentials, the JWT token is issued correctly, but all authenticated requests immediately fail with `401 Unauthorized`. The login response itself is fine — the problem occurs on the next request.

#### R3 — Creating a new task fails with 500

Navigate to a project, click "New Task", fill in the form and submit. The API returns a 500 error. The task is not created.

#### R4 — Task creation still fails with 500 (after fixing R3)

Even after resolving R3, creating a task still produces a 500 error. This time the error comes from the database layer.

#### R5 — Task detail page shows no description

Open any task that has a description. The detail page renders, but the description field is blank — it shows "No description provided" even though the task clearly has one.

#### R6 — Moving a task to "In Progress" breaks the board view

Drag or edit a task to change its status to "In Progress". The task disappears from the board view — it no longer appears in any column. The list view shows it with an unrecognized status. Other status transitions (Todo → Review, Review → Done) work fine.

#### R7 — No notifications for task updates or assignments

When a task is updated (e.g., status changed) or a user is assigned to a task, the affected users should receive in-app notifications. Task-created notifications work fine, but update and assignment notifications never appear.

*Hint: The notification consumer processes events from an outbox. Look at how the event handlers decide whether to send a notification.*

#### R8 — Notification list fills with duplicates

After using the app for a while, the notifications page is flooded with duplicate entries — the same notification appears dozens of times and keeps growing. This affects all notification types.

*Hint: Trace what happens to an event in the outbox after it has been successfully processed by the background worker.*

---

### Phase 3: Feature Implementation

Implement the following two features end-to-end (backend API + frontend UI). Follow the patterns and conventions already established in the codebase.

---

#### Feature 1: Task Comments

As a team member, I want to leave comments on tasks so that I can discuss progress, ask questions, and share updates with my colleagues directly in context.

**Acceptance criteria:**
- On the task detail page, there is a "Comments" section below the existing task information
- I can see all comments on a task, showing who wrote each comment and when (newest first)
- I can type a comment and post it — it appears immediately without a page refresh
- Comments cannot be empty
- The commenter's name is displayed (not their ID)

---

#### Feature 2: Project Archival

As a project manager, I want to archive completed or inactive projects so that they don't clutter the active project list, but I can still access them when needed.

**Acceptance criteria:**
- I can archive a project from the project list or project detail view
- Archived projects are hidden from the main project list by default
- There is a way to show/hide archived projects on the project list page
- Archived projects are visually distinguishable from active ones
- I can unarchive a previously archived project to make it active again

---

**Code quality notes:**
- Follow the patterns already established in the codebase (MediatR handlers, Dapper repos, NgRx actions/effects, Angular Material UI)
- Use the existing project structure — don't reorganize
- Commit your work with meaningful messages after each defect is fixed or feature is implemented.

Good luck!
