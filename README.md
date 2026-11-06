# LeaveManagementSystem

A .NET-based Leave Management System built with Clean Architecture principles. The repository snapshot reviewed for this README contains the API, Application, Domain, and Infrastructure projects. The current working branch may also contain the newer Contracts and Azure Functions projects discussed during development.

## Solution structure

- **LeaveManagementSystem.API** — ASP.NET Core HTTP endpoints, request handling, middleware, and API response codes.
- **LeaveManagementSystem.Application** — application services and service/repository abstractions.
- **LeaveManagementSystem.Domain** — domain entities and request/response models.
- **LeaveManagementSystem.Infrastructure** — EF Core persistence, repositories, Azure Blob Storage, and external-service implementations.
- **LeaveManagementSystem.Contracts** *(added in the current working branch)* — shared integration message contracts used across application boundaries.
- **LeaveManagementSystem.Functions** *(added in the current working branch)* — Azure Functions consumers, including the Service Bus-triggered `ProcessLeaveRequest` function.

## Existing API endpoints

The routes below are present in the uploaded repository snapshot. ASP.NET Core route matching is case-insensitive, so `/api/Employee` and `/api/employee` resolve equivalently.

| Method | Endpoint | Purpose | Success response |
|---|---|---|---|
| `GET` | `/api/Employee` | Retrieves all employees. | `200 OK`, API response containing employee data. |
| `GET` | `/api/Employee/{employeeId}` | Retrieves one employee by its integer ID. | `200 OK` when found; `404 Not Found` through the application response when missing. |
| `POST` | `/api/Document/upload` | Uploads an employee document. Accepts `multipart/form-data` with the upload request fields. The employee must exist. | `200 OK` with document metadata. |
| `GET` | `/api/Document/{documentId}` | Downloads a document by its integer ID. | File response (`application/octet-stream`). |
| `DELETE` | `/api/Document/{documentId}` | Deletes a document from Blob Storage and removes its database record. | `204 No Content`. |

### Document upload fields

`POST /api/Document/upload` is declared with `[Consumes("multipart/form-data")]` and binds `UploadDocumentRequest` from the form. In the uploaded snapshot, that model contains `EmployeeId`, `FileStream` (`Stream`), `FileName`, `ContentType`, and `FileSize`.

**Implementation note:** ASP.NET Core file uploads are normally bound through `IFormFile`. Since the snapshot uses a `Stream` property, verify the actual binding behavior and multipart field names before relying on this endpoint from a client. The Application service stores the file in Azure Blob Storage and records document metadata in the database.

## Service Bus integration

The intended asynchronous flow is:

```text
LeaveManagementSystem.API
        |
        | sends JSON message using managed identity
        v
Azure Service Bus namespace: sb-leavemanagement-dev
        |
        v
Queue: leave-requests
        |
        | ServiceBusTrigger
        v
Azure Function: ProcessLeaveRequest
```

The queue-based integration is being implemented. The actual leave-request entity and business workflow are not part of the reviewed snapshot, so the initial message is a test/integration message and does not create or approve a leave request.

### Message contract

The current planned contract is `ProcessLeaveRequestMessage` in `LeaveManagementSystem.Contracts`:

```json
{
  "messageVersion": 1,
  "leaveRequestId": 501
}
```

`501` is a sample ID for integration testing only.

### Temporary integration-test endpoint (to be added)

The following endpoint is proposed for testing API-to-Service-Bus delivery. It is **not present in the uploaded repository snapshot** and should be documented as available only after the controller is implemented:

| Method | Endpoint | Purpose | Expected response |
|---|---|---|---|
| `POST` | `/api/service-bus-test/leave-request` | Sends a sample `ProcessLeaveRequestMessage` to the configured `leave-requests` queue. This verifies message publishing, not leave-request creation. | `202 Accepted` after Service Bus accepts the send. |

Example request body:

```json
{
  "messageVersion": 1,
  "leaveRequestId": 501
}
```

Keep this endpoint temporary and protect it from public use in any deployed environment. Replace it with a business-oriented endpoint/use case when leave-request creation is implemented.

## Configuration

The API uses configuration for the database connection, Blob Storage, Application Insights, and (during Service Bus integration) the Service Bus namespace and queue.

Example non-secret Service Bus settings:

```json
{
  "ServiceBus": {
    "FullyQualifiedNamespace": "sb-leavemanagement-dev.servicebus.windows.net",
    "QueueName": "leave-requests"
  }
}
```

Do not store Service Bus connection-string secrets in source control. In Azure, configure the App Service's managed identity with **Azure Service Bus Data Sender** at the required scope. Configure the Function App's managed identity with **Azure Service Bus Data Receiver**. Local developer credentials require their own appropriate role assignments.

## Running locally

1. Install the .NET SDK matching the projects' target framework.
2. Configure local database and Blob Storage settings using User Secrets or another untracked local configuration source.
3. Sign in with an Azure developer identity that has the required resource permissions if testing Azure services locally.
4. Build the solution:

   ```bash
   dotnet build
   ```

5. Run the API project from Visual Studio or with `dotnet run --project LeaveManagementSystem.API`.
6. If using the Functions project, run it separately with Azure Functions Core Tools or from the IDE and verify its logs.

## Architecture and reliability notes

- Controllers should remain thin and delegate application work through injected interfaces.
- Infrastructure owns Azure SDK integrations; application-facing interfaces should not expose Azure SDK types.
- Reuse long-lived Azure SDK clients and configured senders through dependency injection.
- A successful Service Bus send confirms that the broker accepted the message; it does not confirm that the Function completed business processing.
- Consumers should be designed for idempotent processing because a message can be delivered again.
- For production workflows that write to a database and publish a message, evaluate the transactional outbox pattern to avoid database/message dual-write inconsistency.
- The namespace currently uses the Basic tier. Topics and subscriptions require the Standard or Premium tier, so plan the learning exercise accordingly before changing tiers.

## Current scope

The existing endpoints cover employee retrieval and document upload/download/deletion. Service Bus queue integration is being added as a learning/integration milestone. Actual leave-request creation, approval, and persistence should be documented as available only after those use cases are implemented.
