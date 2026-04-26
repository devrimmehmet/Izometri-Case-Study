# TODO Roadmap

## Verified Done

- [x] Scaffold .NET 10 solution and Onion Architecture projects.
- [x] Add two independent APIs: `ExpenseService.Api` and `NotificationService.Api`.
- [x] Add database-per-service topology with separate Expense and Notification PostgreSQL databases.
- [x] Add shared integration event contracts.
- [x] Add ExpenseService domain entities and enums.
- [x] Add NotificationService domain entities.
- [x] Add EF Core DbContexts, Code First migrations, global query filters, audit fields and soft delete behavior.
- [x] Add generic repository and UnitOfWork.
- [x] Add seed data for 2 tenants and Admin/HR/Personnel users.
- [x] Add tenant-scoped email uniqueness with `(TenantId, Email)`.
- [x] Add JWT login endpoint with `UserId`, `TenantId` and role claims.
- [x] Add current user, tenant and correlation context.
- [x] Add FluentValidation request validators.
- [x] Add ExpenseService endpoints: login, create, list, detail, submit, approve, reject, delete.
- [x] Add NotificationService inspection endpoint: `GET /api/notifications`.
- [x] Add personnel vs HR/Admin visibility rules.
- [x] Add approval flow for `<= 5000 TRY` and `> 5000 TRY`.
- [x] Add rejection reason validation.
- [x] Add outbox message creation in expense create/approve/reject transactions.
- [x] Add RabbitMQ outbox publisher worker.
- [x] Add RabbitMQ consumer in NotificationService.
- [x] Add Notification persistence and processed-message idempotency.
- [x] Add mock notification logging in NotificationService.
- [x] Add ExpenseService HTTP client from NotificationService.
- [x] Add HTTP resilience/retry policy for NotificationService -> ExpenseService calls.
- [x] Add Swagger/OpenAPI to both APIs.
- [x] Add Dockerfiles and Docker Compose for RabbitMQ, Expense DB, Notification DB, Expense API and Notification API.
- [x] Change local host ports to avoid existing services: RabbitMQ `5673/15673`, PostgreSQL `15433/15434`.
- [x] Add xUnit + Moq test project.
- [x] Add unit tests for ExpenseService API controllers.
- [x] Add unit tests for NotificationService API controller.
- [x] Add unit tests for Notification event handler.
- [x] Add validator and approval-threshold unit tests.
- [x] Add README documentation.

## Verified Commands

- [x] `dotnet build Izometri.CaseStudy.slnx`
- [x] `dotnet test Izometri.CaseStudy.slnx`
- [x] `docker compose config`
- [x] `docker compose up -d --build`
- [x] Local smoke flow: login, expense create, submit, HR approve and RabbitMQ notification consumption.

## Remaining / Honest Pending Items

- [ ] Add automated integration tests for API + PostgreSQL + RabbitMQ instead of only manual Docker smoke testing.
- [ ] Add Admin user management endpoints.
- [ ] Add Admin role management endpoints.
- [ ] Optional bonus: add Keycloak/Auth0/IdentityServer OAuth2 integration mode without removing simple JWT login.
- [ ] Add richer authorization tests for multi-role users and tenant-crossing negative cases.
- [ ] Add outbox retry/dead-letter integration tests.
- [ ] Add notification consumer integration tests against real RabbitMQ test container.
