# DerivativeEDGE Hedge Accounting UI - AI Coding Instructions

## Project Overview
This is a **Blazor Server** application (`.NET 8.0`) providing the UI for the Next Gen Hedge Accounting Service. The application follows a **Feature-Slice Vertical Architecture** using **MediatR** for CQRS, **Syncfusion** for UI components, and **Tailwind CSS** for styling.

## Architecture & Patterns

### Feature-Slice Organization
The application uses **Vertical Slice Architecture** where each feature is self-contained:
```
Features/HedgeRelationships/
├── Components/        # Blazor components specific to this feature
├── Handlers/
│   ├── Queries/      # MediatR query handlers
│   └── Commands/     # MediatR command handlers
├── Models/           # Feature-specific view models
├── Pages/            # Routable Blazor pages
└── Validation/       # FluentValidation rules
```

**Key Pattern**: Business logic lives in MediatR handlers, not in Blazor code-behind. Components invoke handlers via `IMediator.Send()`.

### MediatR CQRS Pattern
- **Queries**: Read operations return data (e.g., `GetHedgeRelationshipById.cs`)
- **Commands**: Write operations modify state (e.g., `CreateHedgeRelationship.cs`)
- **Handlers**: Each query/command has a dedicated handler class
- **Example Usage**:
  ```csharp
  var result = await Mediator.Send(new GetHedgeRelationshipById.Query { Id = id });
  ```

### API Communication
- **API Client**: Auto-generated from OpenAPI spec (`HedgeAccountingApi.json`) in `DerivativeEdge.HedgeAccounting.Api.Client`
- **Service Wrapper**: `HedgeAccountingApiService` in `Services/HedgeAccountingApi/` wraps the generated client
- **Authentication**: JWT tokens managed via `AuthorizationHeaderHandler` middleware
- **Pattern**: UI handlers call the service wrapper, never the API client directly

### UI Component Architecture

#### Syncfusion + Tailwind Separation
**CRITICAL**: Never mix Syncfusion CSS with Tailwind CSS on the same element.

```razor
<!-- ✅ CORRECT: Tailwind for layout, Syncfusion CSS for component -->
<div class="grid grid-cols-3 gap-6 mb-6">
    <div class="flex flex-col">
        <label class="block text-sm font-normal text-gray-700 mb-2">Name</label>
        <SfTextBox CssClass="input-textbox" @bind-Value="@Model.Name" />
    </div>
</div>

<!-- ❌ WRONG: Never do this -->
<SfTextBox CssClass="input-textbox w-full px-3" />
```

**Syncfusion Component CSS Classes** (see `.github/instructions/SYNCFUSION_COPILOT.instructions.md`):
- `input-textbox`, `dropdown-input`, `input-checkbox`, `input-radiobutton`
- `e-primary`, `e-secondary` (buttons)
- `custom-grid`, `listbox-container`, `custom-tab`

**Tailwind Usage** (see `.github/instructions/TAILWIND_COPILOT.instructions.md`):
- Apply to container `<div>` elements for layout/spacing
- Use for labels, validation messages, non-Syncfusion elements
- **Never use inline `style=""`** - always use Tailwind utilities

### State Management
- **Scoped Services**: Most services are scoped to circuit lifetime (Blazor Server)
- **Local Storage**: `Blazored.LocalStorage` for client-side persistence
- **User Context**: `IUserAuthData` provides authenticated user info via `AuthenticationStateProvider`

## Build & Test Workflow

### Build Process
```bash
cd src
# Setup CodeArtifact auth (requires AWS credentials)
chmod +x codeartifact.sh && ./codeartifact.sh
dotnet restore
dotnet build
```

### Running Locally
1. Clone repo
2. Debug via IDE (VS/Rider/VSCode)
3. **Dependencies**: Requires Hedge Accounting API running (see API repo docs)
4. App communicates with API via `ConfigurationKeys.HedgeAccountingServiceUrl` in `appsettings.json`

### Testing Strategy
- **Playwright E2E Tests**: `DerivativeEDGE.HedgeAccounting.UI.Tests/`
  - Uses `Microsoft.Playwright` and `Verify.Playwright`
  - Requires Playwright install: `pwsh bin/Debug/net8.0/playwright.ps1 install`
  - Run: `dotnet test` in test project directory
  
- **Bunit Component Tests**: `DerivativeEDGE.HedgeAccounting.UI.Bunit.Tests/`
  - Uses `bunit` for Blazor component testing
  - Uses `Verify.Bunit` for snapshot testing
  - Example: `IndexCSharpTests.cs`

- **Test Coverage**: Uses `coverlet` for code coverage reporting to Codacy

### CI/CD Pipeline
**Bitbucket Pipelines** (`bitbucket-pipelines.yml`):
1. Build & Test (dotnet restore → build → test with coverage)
2. Deploy via AWS CDK (`cdk/` directory)
3. Docker builds use multi-stage Dockerfile with .NET 8 SDK

## Key Conventions

### Authentication & Authorization
- **OAuth2 + JWT**: Configured via `AddOAuth2()` and `AddCustomAuthentication()`
- **Policy**: `"auth"` policy requires authenticated user
- **User Metadata**: `IUserMetaDataService` provides user context
- **Identity Client**: `IIdentityClient` for user management

### Feature Flags
- Uses **Split.io** via `AddSplitIOFeatureManager()`
- Wrapped by `Microsoft.FeatureManagement`
- Check flags: `IFeatureManager.IsEnabledAsync("feature-name")`

### Telemetry
- **OpenTelemetry** configured in `Instramentation.cs` and `ServiceCollectionExtensions/OtelConfig.cs`
- Exports to AWS (instrumentation for ASP.NET Core, HTTP, AWS services)

### Error Handling
- **AlertService**: `IAlertService` for displaying user-facing alerts
- **Polly**: Retry policies for HTTP clients (see `Program.cs` `getRetryPolicy()`)

### AutoMapper
- Registered in `Program.cs`: `AddAutoMapper(typeof(Program), typeof(GetTrades))`
- Mappings defined near handlers using them

### Validation
- Uses DataAnnotations on models
- ValidationMessage components for inline errors
- Pattern: `<ValidationMessage For="@(() => Model.Property)" class="text-xs text-red-600 mt-1" />`

## Critical Domain Knowledge

### Hedge Relationship Workflow
The application manages hedge relationship lifecycle with state transitions:
- **Draft** → **Designated** → **Dedesignated**
- **Workflow Actions**: Designate, Redraft, Re-Designate, De-Designate
- See `WORKFLOW_COMPARISON.md` for business rules on which actions are available per state/type

### Document Templates
- Template creation/editing via `Pages/DocumentTemplate/`
- Hedge documents generated from templates
- Preview and gallery features for both templates and hedge documents

## Common Pitfalls

1. **Don't modify Playwright test snapshots manually** - they're auto-verified via Verify framework
2. **Private NuGet packages** require AWS CodeArtifact authentication - won't build without it
3. **Component Library** (`DerivativeEdge.Blazor.ComponentLibrary`) is proprietary - follow its conventions
4. **Never add Tailwind classes to Syncfusion components** - use wrapper divs
5. **All pages need `overflow-auto`** for scrolling when content exceeds viewport
6. **MediatR handlers should be thin** - delegate complex logic to services
7. **Test isolation** - Use `BlazorServerWebApplicationFactory` for E2E tests

## Key Files Reference
- `Program.cs` - Service registration, middleware pipeline
- `Features/HedgeRelationships/` - Main business feature
- `.github/instructions/` - Detailed Tailwind & Syncfusion rules
- `bitbucket-pipelines.yml` - CI/CD process
- `WORKFLOW_COMPARISON.md` - Business rules documentation
