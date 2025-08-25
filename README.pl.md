# Document Access Approval System

REST API w **ASP.NET Core 8** + **EF Core (InMemory)** do obs³ugi wniosków o dostêp do dokumentów.  
Architektura warstwowa z **CQRS + MediatR**, **JWT auth z rolami**, walidacj¹ (FluentValidation), globalnym error handlingiem oraz przyk³adow¹ symulacj¹ background jobu.

---

## Architektura

- **Domain** – encje i enumy domenowe
- **Infrastructure** – `AppDbContext` (EF Core InMemory, mo¿liwoœæ podpiêcia SQLite)
- **Application** – CQRS (Commands, Queries), DTO, walidatory, pipeline behaviors, notyfikacje
- **Api** – kontrolery (AccessRequests, Auth), konfiguracja MediatR, JWT, Swagger, error handling
- **Tests** – NUnit + FluentAssertions (testy handlerów, walidatorów, eventów)

---

## Uruchomienie

```bash
dotnet restore
dotnet build
dotnet run --project src/DocumentAccessApprovalSystem.API


Swagger UI: https://localhost:7111/swagger

Seed danych (Program.cs)

	Przy starcie API tworzeni s¹ przyk³adowi u¿ytkownicy i dokumenty:

		Users:

			Alice – aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa – rola User, login alice/alice

			Bob – bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb – rola Approver, login bob/bob

		Documents:

			HR Handbook – 11111111-1111-1111-1111-111111111111

			Q3 Financials – 22222222-2222-2222-2222-222222222222

Authentication (JWT)
	Pobierz token:
		POST /api/auth/token
		{
		  "username": "alice",
		  "password": "alice"
		}
	W Swaggerze kliknij Authorize i wpisz:
	Bearer <TOKEN>

Dostêpy:

	User – mo¿e tworzyæ wnioski (POST /api/AccessRequests) i sprawdzaæ swoje (GET /mine/{userId})

	Approver – dodatkowo widzi pending (GET /pending) i decyduje (POST /{id}/decision)

Endpoints

	POST /api/AccessRequests – utworzenie wniosku

	GET /api/AccessRequests/pending – lista oczekuj¹cych (Approver)

	POST /api/AccessRequests/{id}/decision – akceptacja/odrzucenie (Approver)

	GET /api/AccessRequests/mine/{userId} – wnioski u¿ytkownika

Walidacja

	Walidatory (FluentValidation) weryfikuj¹ komendy CQRS.

	B³êdy walidacji ? 400 Bad Request w formacie ProblemDetails:

	Error handling

Globalny handler (GlobalExceptionHandler) mapuje wyj¹tki na ProblemDetails:

	ValidationException ? 400 + szczegó³y b³êdów

	KeyNotFoundException ? 404

	InvalidOperationException ? 400

	DbUpdateConcurrencyException ? 409

	inne ? 500

Background job / event simulation

	Po decyzji publikowany jest event DecisionMadeNotification (MediatR).
	Handler loguje zdarzenie i symuluje background job (np. wys³anie maila).

Testy

	Framework: NUnit + FluentAssertions + EFCore.InMemory

	Scenariusze testowe:

	approve i reject

	blokada duplikatu pending

	lista pending

	walidacja pustego reason

	publikacja eventu po decyzji

Uruchomienie:
	dotnet test

Za³o¿enia i trade-offs

	Jeden Pending na (User, Document, AccessType) – wymuszane aplikacyjnie.

	Has³a w seedzie s¹ plaintext ? w prawdziwej aplikacji: ASP.NET Core Identity + haszowanie.

	Pipeline behaviors: Logging i Validation.

	Notifications: prosta symulacja, w realnym wdro¿eniu ? kolejka / e-mail sender.

	Globalny error handler zapewnia spójne odpowiedzi w JSON.

