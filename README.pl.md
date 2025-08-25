# Document Access Approval System

REST API w **ASP.NET Core 8** + **EF Core (InMemory)** do obs�ugi wniosk�w o dost�p do dokument�w.  
Architektura warstwowa z **CQRS + MediatR**, **JWT auth z rolami**, walidacj� (FluentValidation), globalnym error handlingiem oraz przyk�adow� symulacj� background jobu.

---

## Architektura

- **Domain** � encje i enumy domenowe
- **Infrastructure** � `AppDbContext` (EF Core InMemory, mo�liwo�� podpi�cia SQLite)
- **Application** � CQRS (Commands, Queries), DTO, walidatory, pipeline behaviors, notyfikacje
- **Api** � kontrolery (AccessRequests, Auth), konfiguracja MediatR, JWT, Swagger, error handling
- **Tests** � NUnit + FluentAssertions (testy handler�w, walidator�w, event�w)

---

## Uruchomienie

```bash
dotnet restore
dotnet build
dotnet run --project src/DocumentAccessApprovalSystem.API


Swagger UI: https://localhost:7111/swagger

Seed danych (Program.cs)

	Przy starcie API tworzeni s� przyk�adowi u�ytkownicy i dokumenty:

		Users:

			Alice � aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa � rola User, login alice/alice

			Bob � bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb � rola Approver, login bob/bob

		Documents:

			HR Handbook � 11111111-1111-1111-1111-111111111111

			Q3 Financials � 22222222-2222-2222-2222-222222222222

Authentication (JWT)
	Pobierz token:
		POST /api/auth/token
		{
		  "username": "alice",
		  "password": "alice"
		}
	W Swaggerze kliknij Authorize i wpisz:
	Bearer <TOKEN>

Dost�py:

	User � mo�e tworzy� wnioski (POST /api/AccessRequests) i sprawdza� swoje (GET /mine/{userId})

	Approver � dodatkowo widzi pending (GET /pending) i decyduje (POST /{id}/decision)

Endpoints

	POST /api/AccessRequests � utworzenie wniosku

	GET /api/AccessRequests/pending � lista oczekuj�cych (Approver)

	POST /api/AccessRequests/{id}/decision � akceptacja/odrzucenie (Approver)

	GET /api/AccessRequests/mine/{userId} � wnioski u�ytkownika

Walidacja

	Walidatory (FluentValidation) weryfikuj� komendy CQRS.

	B��dy walidacji ? 400 Bad Request w formacie ProblemDetails:

	Error handling

Globalny handler (GlobalExceptionHandler) mapuje wyj�tki na ProblemDetails:

	ValidationException ? 400 + szczeg�y b��d�w

	KeyNotFoundException ? 404

	InvalidOperationException ? 400

	DbUpdateConcurrencyException ? 409

	inne ? 500

Background job / event simulation

	Po decyzji publikowany jest event DecisionMadeNotification (MediatR).
	Handler loguje zdarzenie i symuluje background job (np. wys�anie maila).

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

Za�o�enia i trade-offs

	Jeden Pending na (User, Document, AccessType) � wymuszane aplikacyjnie.

	Has�a w seedzie s� plaintext ? w prawdziwej aplikacji: ASP.NET Core Identity + haszowanie.

	Pipeline behaviors: Logging i Validation.

	Notifications: prosta symulacja, w realnym wdro�eniu ? kolejka / e-mail sender.

	Globalny error handler zapewnia sp�jne odpowiedzi w JSON.

