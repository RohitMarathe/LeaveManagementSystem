# Bruno API testing

This collection exercises the existing LeaveManagementSystem API without an Angular frontend.

## Import

Open Bruno, choose **Open Collection**, and select the `.bruno` folder in the repository.

## Local API

Run the API project:

```powershell
dotnet run --project LeaveManagementSystem.API --launch-profile http
```

The collection's `local` environment uses `http://localhost:5234`. Select the `local` environment in Bruno.

## Request order

1. `Employees/Get-All-Employees` to inspect available employees.
2. `Employees/Get-Employee-By-Id` using a valid employee ID.
3. `Leave-Requests/Submit-Leave-Request` to create a leave request and its Outbox event in one SQL transaction. Change `employeeId` to an active employee with an assigned manager. The date values are sample future dates; adjust them for your test.
4. Document requests are optional and require the API's Blob Storage configuration. The upload request is included as a template; confirm the multipart `fileStream` binding against the current `UploadDocumentRequest` before relying on it.

## Outbox publisher verification

Bruno tests the HTTP API. It does not invoke the Timer-triggered publisher directly. After a successful leave request:

- Confirm a new row appears in `dbo.OutboxMessages` with `ProcessedAtUtc` null and `FailedAtUtc` null.
- When the publisher Function runs, confirm `ProcessedAtUtc` becomes populated.
- Confirm the Service Bus `leave-events` topic receives the event and both subscriptions can independently receive it.

## Notes

- The API currently has no authentication middleware configured, so requests use `auth: none`.
- `employeeId` defaults to `1` only as a placeholder. Replace it with a real employee ID from the GET response.
- Do not commit secrets or production endpoints into this collection. Keep local overrides in Bruno environments.
- The leave request response is expected to use HTTP 201. The exact returned ID can be copied into a local variable if you want to query by ID later; this API snapshot does not expose a GET leave-request route.
