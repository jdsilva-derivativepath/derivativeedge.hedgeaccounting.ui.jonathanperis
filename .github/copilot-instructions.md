# DerivativeEDGE Hedge Accounting UI - AI Coding Instructions

## What This Repository Does
This repository contains the **Next Generation Hedge Accounting User Interface** for DerivativeEDGE, a financial derivatives management platform. The UI enables users to:
- Create and manage hedge relationships (Draft → Designated → Dedesignated lifecycle)
- Document hedge accounting strategies (cash flow, fair value, net investment)
- Track and test hedge effectiveness
- Manage amortization schedules for hedge instruments
- Generate and preview hedge accounting documentation
- Audit hedge relationship history and changes

The application serves financial institutions performing hedge accounting under ASC 815 (U.S. GAAP) or IAS 39/IFRS 9 standards.

## Project Overview
This is a **Blazor Server** application (`.NET 8.0`) providing the UI for the Next Gen Hedge Accounting Service. The application is a **lift-and-shift migration** from a legacy AngularJS + .NET Framework system to modern Blazor Server + .NET 8. The application follows a **Feature-Slice Vertical Architecture** using **MediatR** for CQRS, **Syncfusion** for UI components, and **Tailwind CSS** for styling.

## Migration Context (CRITICAL)

### You Are Working on a Migration Project
This repository contains a **lift-and-shift migration** from legacy code to new technology stack:
- **Old System:** AngularJS + ASP.NET MVC (.NET Framework)
- **New System:** Blazor Server + .NET 8.0

### Migration Rules (MUST FOLLOW)
1. **Preserve All Business Logic:** Migrate functionality exactly as-is without adding or removing business rules
2. **Exact Behavior Match:** The new implementation MUST produce identical results to the legacy system
3. **No New Features:** Do not add enhancements or new features during migration
4. **No Rule Removal:** Do not simplify or "improve" legacy business logic
5. **Reference Legacy Code:** Always check `./old/` directory files to understand expected behavior

### Legacy-to-New File Mapping

#### Main Hedge Relationship Files
| Legacy File | New File | Purpose |
|------------|----------|---------|
| `./old/HedgeRelationship.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor` | Main page with workflow |
| `./old/hr_hedgeRelationshipAddEditCtrl.js` | `Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs` | Controller/code-behind with workflow logic |

#### Tab Views
| Legacy File | New File | Purpose |
|------------|----------|---------|
| `./old/initialView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor` | Initial view (embedded in main page) |
| `./old/detailsView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/HedgeRelationshipLogsTab.razor` | Details/logs tab |
| `./old/instrumentsAnalysisView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/InstrumentAnalysisTab.razor` | Instrument analysis |
| `./old/hedgetestResultsView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/TestResultsTab.razor` | Test results |
| `./old/accountingView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/AccountingDetailsTab.razor` | Accounting details |
| `./old/amortizationView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/AmortizationTab.razor` | Amortization |
| `./old/historyView.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/HedgeRelationshipHistoryTab.razor` | History |
| `./old/optionTimeValue.cshtml` | `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/OptionAmortizationTab.razor` | Option amortization |

#### Key Entry Point
- **Workflow Starting Point:** `HedgeRelationshipDetails.razor.cs` → `HandleWorkflowAction()` method
- **API Reference:** `./api/HedgeAccountingApiClient.cs` contains all API objects and methods
- **Handler Examples:** `Features/HedgeRelationships/Handlers/` shows how to implement queries/commands if needed

### Critical Migration Lessons Learned
1. **Workflow Logic:** The `BuildWorkflowItems()` method must exactly match legacy JavaScript `setWorkFlow()` - see `WORKFLOW_COMPARISON.md`
2. **Array Operations:** JavaScript splice/replace operations need careful translation to C# - don't assume equivalence
3. **State Transitions:** Hedge relationships follow `Draft → Designated → Dedesignated` lifecycle with specific actions per state
4. **Role-Based Permissions:** Workflow actions availability depends on HedgeState, HedgeType, AND user roles (24, 17, 5)

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

### IMPORTANT: Build Limitations
**⚠️ DO NOT ATTEMPT TO BUILD THE PROJECT/SOLUTION ⚠️**
- The project uses **private NuGet packages** from AWS CodeArtifact
- Build requires AWS credentials and VPN access to DerivativePath internal network
- Build will **always fail** in external environments
- Focus on code analysis and migration logic, not building

### Build Process (For Reference Only - Won't Work Without AWS Access)
```bash
cd src
# Setup CodeArtifact auth (requires AWS credentials + VPN)
chmod +x codeartifact.sh && ./codeartifact.sh
dotnet restore
dotnet build
```

### Private Dependencies
The following packages require AWS CodeArtifact authentication:
- `DerivativeEdge.Blazor.ComponentLibrary` (v1.0.2259)
- `DerivativeEDGE.Domain.Entities` (v1.0.1499)
- `DerivativeEDGE.FeatureFlag` (v1.0.37)
- `DerivativeEDGE.Identity.API.Client` (v1.0.313)

Authentication is configured in:
- `src/NuGet.config` - Package source configuration
- `src/codeartifact.sh` - AWS authentication script (requires `BITBUCKET_STEP_OIDC_TOKEN` env var)

### Running Locally
1. Clone repo
2. Debug via IDE (VS/Rider/VSCode)
3. **Dependencies**: Requires Hedge Accounting API running (see API repo docs)
4. App communicates with API via `ConfigurationKeys.HedgeAccountingServiceUrl` in `appsettings.json`

### Testing Strategy
- **Playwright E2E Tests**: `DerivativeEDGE.HedgeAccounting.UI.Tests/`
  - Uses `Microsoft.Playwright` and `Verify.Playwright`
  - Requires Playwright install: `pwsh bin/Debug/net8.0/playwright.ps1 install`
  - Run: `cd src/DerivativeEDGE.HedgeAccounting.UI.Tests && dotnet test`
  - Uses `BlazorServerWebApplicationFactory` for test isolation
  
- **Bunit Component Tests**: `DerivativeEDGE.HedgeAccounting.UI.Bunit.Tests/`
  - Uses `bunit` for Blazor component testing
  - Uses `Verify.Bunit` for snapshot testing
  - Example: `IndexCSharpTests.cs`
  - Run: `cd src/DerivativeEDGE.HedgeAccounting.UI.Bunit.Tests && dotnet test`

- **Test Coverage**: Uses `coverlet` for code coverage reporting to Codacy
  - Coverage command: `dotnet test /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=cobertura`

**NOTE:** Tests will fail without AWS CodeArtifact access due to private package dependencies.

### CI/CD Pipeline
**Bitbucket Pipelines** (`bitbucket-pipelines.yml`):
1. **Build & Test Step:**
   - Runs in custom Docker image: `765057520137.dkr.ecr.us-west-2.amazonaws.com/bitbucket/dotnet:v8`
   - Authenticates with AWS CodeArtifact via `codeartifact.sh`
   - Runs: `dotnet restore → dotnet build → dotnet test`
   - Installs Playwright: `pwsh bin/Debug/net8.0/playwright.ps1 install`
   - Collects coverage and reports to Codacy

2. **Docker Build Step:**
   - Uses `docker-compose.yml` in `src/` directory
   - Multi-stage Dockerfile: `src/DerivativeEDGE.HedgeAccounting.UI/Dockerfile`
   - Base image: `mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled-extra`
   - Build image: `mcr.microsoft.com/dotnet/sdk:8.0`
   - Pushes to AWS ECR with tag format: `{branch}-{build_number}`

3. **Deployment Steps:**
   - Deploy via AWS CDK TypeScript (`cdk/` directory)
   - Environments: Development, Staging, Production, Production-DR
   - Stack names: `{env}-hedge-accounting-ui`

### Runtimes and Versions
- **.NET SDK:** 8.0 (specified in `src/global.json`)
- **PowerShell:** 7.3.10 (for Playwright installation)
- **Docker:** Multi-stage builds with .NET 8 SDK and ASP.NET 8.0 runtime
- **Node.js:** Used in CDK deployment steps (version from ECR image)

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

## Common Pitfalls & Critical Rules

### Migration-Specific Pitfalls
1. **Always check legacy code first** - Never assume behavior without verifying against `./old/` files
2. **JavaScript array operations ≠ C# operations** - `splice()`, `shift()`, `unshift()` need careful translation
3. **Workflow state logic is complex** - See `WORKFLOW_COMPARISON.md` for documented expected behavior
4. **Don't "fix" legacy code** - If legacy code has quirks, preserve them in the migration
5. **Role-based permissions** - Workflow actions are gated by user roles 24, 17, and 5

### Build & Environment Pitfalls
6. **Don't try to build without AWS access** - Build will fail, focus on code logic
7. **Private NuGet packages** require AWS CodeArtifact + VPN authentication
8. **Component Library** (`DerivativeEdge.Blazor.ComponentLibrary`) is proprietary - follow its conventions

### Code Style & Architecture Pitfalls
9. **Don't modify Playwright test snapshots manually** - they're auto-verified via Verify framework
10. **Never add Tailwind classes to Syncfusion components** - use wrapper divs
11. **All pages need `overflow-auto`** for scrolling when content exceeds viewport
12. **MediatR handlers should be thin** - delegate complex logic to services
13. **Test isolation** - Use `BlazorServerWebApplicationFactory` for E2E tests
14. **Never use inline `style=""`** - Always use Tailwind utilities or Syncfusion CSS classes

## Key Files Reference

### Core Application Files
- `src/DerivativeEDGE.HedgeAccounting.UI/Program.cs` - Service registration, middleware pipeline, startup
- `src/DerivativeEDGE.HedgeAccounting.UI/App.razor` - Root component
- `src/DerivativeEDGE.HedgeAccounting.UI/_Imports.razor` - Global Razor using statements
- `src/DerivativeEDGE.HedgeAccounting.UI/Usings.cs` - Global C# using statements

### Feature Implementation
- `Features/HedgeRelationships/` - Main business feature (hedge relationship lifecycle)
- `Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs` - **Critical:** Workflow entry point
- `Features/HedgeRelationships/Handlers/` - MediatR query/command handlers
- `Features/HedgeRelationships/Models/` - View models and DTOs
- `Features/HedgeRelationships/Validation/` - FluentValidation rules

### API Integration
- `api/HedgeAccountingApiClient.cs` - **Reference file:** All API objects/methods (auto-generated from OpenAPI spec)
- `src/DerivativeEdge.HedgeAccounting.Api.Client/` - API client project (auto-generated)
- `Services/HedgeAccountingApi/HedgeAccountingApiService.cs` - Service wrapper for API client
- `Services/HedgeAccountingApi/ApiTokenManager.cs` - JWT token management

### Legacy Reference Files (READ ONLY - For Migration Context)
- `old/hr_hedgeRelationshipAddEditCtrl.js` - **162KB** - Legacy AngularJS controller with all business logic
- `old/HedgeRelationship.cshtml` - Legacy main view
- `old/initialView.cshtml` - Legacy initial tab
- `old/detailsView.cshtml` - Legacy details/logs tab
- `old/instrumentsAnalysisView.cshtml` - Legacy instrument analysis
- `old/hedgetestResultsView.cshtml` - Legacy test results
- `old/accountingView.cshtml` - Legacy accounting details
- `old/amortizationView.cshtml` - Legacy amortization
- `old/historyView.cshtml` - Legacy history
- `old/optionTimeValue.cshtml` - Legacy option amortization

### Documentation
- `.github/copilot-instructions.md` - This file (AI coding instructions)
- `.github/instructions/TAILWIND_COPILOT.instructions.md` - Tailwind CSS guidelines
- `.github/instructions/SYNCFUSION_COPILOT.instructions.md` - Syncfusion component guidelines
- `WORKFLOW_COMPARISON.md` - **Critical:** Workflow state/action comparison (old vs new)
- `WORKFLOW_FIX_DOCUMENTATION.md` - Detailed workflow fix documentation
- `FIX_SUMMARY.md` - Summary of workflow role matching issue resolution
- `README.md` - Basic repository overview

### Build & CI/CD
- `bitbucket-pipelines.yml` - CI/CD pipeline definition
- `src/codeartifact.sh` - AWS CodeArtifact authentication script
- `src/global.json` - .NET SDK version specification (8.0)
- `src/NuGet.config` - NuGet package sources (including private CodeArtifact)
- `src/docker-compose.yml` - Docker compose for local/build
- `src/DerivativeEDGE.HedgeAccounting.UI/Dockerfile` - Multi-stage Docker build
- `cdk/` - AWS CDK infrastructure-as-code (TypeScript)

### Configuration
- `src/DerivativeEDGE.HedgeAccounting.UI/appsettings.json` - Application configuration
- `src/DerivativeEDGE.HedgeAccounting.UI/appsettings.Development.json` - Development overrides

### Testing
- `src/DerivativeEDGE.HedgeAccounting.UI.Tests/` - Playwright E2E tests
- `src/DerivativeEDGE.HedgeAccounting.UI.Bunit.Tests/` - Bunit component tests

## Quick Start for Migration Tasks

### When Migrating a Feature from Legacy
1. **Read legacy code first:** Check corresponding file in `./old/` directory
2. **Understand data models:** Review `api/HedgeAccountingApiClient.cs` for API objects
3. **Check existing handlers:** Look at `Features/HedgeRelationships/Handlers/` for patterns
4. **Follow architecture:** MediatR handlers for logic, Razor components for UI
5. **Preserve behavior:** Match legacy functionality exactly - no improvements
6. **Verify against documentation:** Check `WORKFLOW_COMPARISON.md` for state/workflow rules
7. **Test manually:** Build won't work, so focus on code correctness

### Common Migration Patterns
```csharp
// AngularJS: $http.get('/api/hedgerelationships/' + id)
// Blazor: var result = await Mediator.Send(new GetHedgeRelationshipById.Query { Id = id });

// AngularJS: $scope.workflowItems = ['Redraft', 'De-Designate'];
// Blazor: WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasPermission });

// AngularJS: if (checkUserRole('24') || checkUserRole('17') || checkUserRole('5'))
// Blazor: if (await HasRequiredRole())
```

## Repository Statistics
- **Repository Type:** Blazor Server Web Application
- **Primary Language:** C# (.NET 8.0)
- **UI Framework:** Blazor Server + Syncfusion Components
- **Styling:** Tailwind CSS
- **Architecture Pattern:** Vertical Slice (Feature-based)
- **Legacy System:** AngularJS + ASP.NET MVC (.NET Framework)
- **Lines of Code:** ~50K+ C#, ~162KB legacy JavaScript
- **Test Projects:** 2 (Playwright E2E + Bunit Component)
- **Private Dependencies:** 4 packages from AWS CodeArtifact
- **Deployment:** AWS (ECS/Fargate via CDK)
