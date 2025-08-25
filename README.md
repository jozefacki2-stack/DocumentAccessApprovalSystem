# Document Access Approval System

REST API built with **ASP.NET Core 8** + **EF Core (InMemory)** for handling document access requests.  
Layered architecture with **CQRS + MediatR**, **JWT auth with roles**, validation (FluentValidation), global error handling, and a background job simulation example.

---

## Architecture

- **Domain** – domain entities and enums
- **Infrastructure** – `AppDbContext` (EF Core InMemory, can be switched to SQLite)
- **Application** – CQRS (Commands, Queries), DTOs, validators, pipeline behaviors, notifications
- **Api** – controllers (AccessRequests, Auth), MediatR config, JWT, Swagger, error handling
- **Tests** – NUnit + FluentAssertions (handler, validator, and event tests)

---

## Run

bash
dotnet restore
dotnet build
dotnet run --project src/DocumentAccessApprovalSystem.API


Swagger UI: https://localhost:7111/swagger

Seed data (Program.cs)

	On startup the API seeds sample users and documents:

Users

	Alice – aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa – role User, login alice/alice

	Bob – bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb – role Approver, login bob/bob

Documents

	HR Handbook – 11111111-1111-1111-1111-111111111111

	Q3 Financials – 22222222-2222-2222-2222-222222222222

Authentication (JWT)

	Obtain a token:
		POST /api/auth/token
		{
		  "username": "alice",
		  "password": "alice"
		}
	In Swagger, click Authorize and enter:
		Bearer <TOKEN>

Access rights:

	User – can create requests (POST /api/AccessRequests) and view their own (GET /mine/{userId})

	Approver – in addition, can see pending (GET /pending) and make decisions (POST /{id}/decision)

Endpoints

	POST /api/AccessRequests – create request

	GET /api/AccessRequests/pending – list pending requests (Approver)

	POST /api/AccessRequests/{id}/decision – approve/reject (Approver)

	GET /api/AccessRequests/mine/{userId} – user’s requests

Validation

	Validators (FluentValidation) validate CQRS commands.

	Validation errors ? 400 Bad Request in ProblemDetails format:

Error handling

	Global handler (GlobalExceptionHandler) maps exceptions to ProblemDetails:

	ValidationException ? 400 + validation errors

	KeyNotFoundException ? 404

	InvalidOperationException ? 400

	DbUpdateConcurrencyException ? 409

	others ? 500

Background job / event simulation

	After a decision is made, a DecisionMadeNotification event is published (via MediatR).
	Handler logs the event and simulates a background job (e.g., sending an email).

Tests

	Framework: NUnit + FluentAssertions + EFCore.InMemory

	Test scenarios:

	approve and reject

	duplicate pending request blocked

	pending list

	validation of empty reason

	event published after decision

## Run

	bash
	dotnet test

Assumptions & trade-offs

	Only one Pending per (User, Document, AccessType) – enforced at application level.

	Passwords in seed are plaintext ? in production: ASP.NET Core Identity + hashing.

	Pipeline behaviors: Logging and Validation.

	Notifications: simple simulation, in production ? queue / email sender.

	Global error handler ensures consistent JSON responses.