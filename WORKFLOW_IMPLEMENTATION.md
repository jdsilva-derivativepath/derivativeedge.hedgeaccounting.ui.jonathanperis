# Hedge Relationship Workflow Implementation

## Overview
This document describes the implementation of the enhanced Hedge Relationship workflow system that supports multiple operational states: Draft, Designate, Redraft, De-Designate, and Re-Designate.

## Architecture

### Design Pattern
The implementation follows the **CQRS (Command Query Responsibility Segregation)** pattern using **MediatR**:
- **Commands**: Operations that change state (Create, Update, Designate, etc.)
- **Queries**: Operations that retrieve data (Get data for workflows)
- **Handlers**: Process commands and queries
- **Validators**: Validate business rules before execution

### Project Structure
```
Features/HedgeRelationships/
├── Handlers/
│   ├── Commands/
│   │   ├── DesignateHedgeRelationship.cs
│   │   ├── RedraftHedgeRelationship.cs
│   │   ├── DeDesignateHedgeRelationship.cs
│   │   ├── ReDesignateHedgeRelationship.cs
│   │   └── GenerateInceptionPackage.cs
│   └── Queries/
│       ├── GetDeDesignateData.cs
│       ├── GetReDesignateData.cs
│       └── FindDocumentTemplate.cs
├── Validation/
│   ├── DesignationRequirementsValidator.cs
│   ├── RedraftValidator.cs
│   ├── DeDesignateValidator.cs
│   └── ReDesignateValidator.cs
└── Pages/
    └── HedgeRelationshipDetails.razor.cs
```

## Workflow States

### State Diagram
```
Draft
  ↓ (Designate)
Designated
  ↓ (De-Designate)        ↓ (Redraft)
Dedesignated              Draft
  ↓ (Re-Designate)        
Designated
```

### State Transitions

| From State    | To State      | Operation     | Requirements |
|---------------|---------------|---------------|--------------|
| Draft         | Designated    | Designate     | All designation requirements met |
| Designated    | Draft         | Redraft       | User has permission |
| Designated    | Dedesignated  | De-Designate  | Valid dedesignation reason |
| Designated    | Designated    | Re-Designate  | Cash Flow hedge only |
| Dedesignated  | Draft         | Redraft       | User has permission |

## Implementation Details

### 1. Designate Workflow

**Handler**: `DesignateHedgeRelationship.Command`

**Process Flow**:
1. Validate designation requirements (DesignationRequirementsValidator)
2. Save current hedge relationship state
3. Check if document template exists
4. Run regression analysis for inception
5. Check analytics availability
6. Generate inception package
7. Return updated hedge relationship

**API Calls**:
- `GET /v1/HedgeRelationship/{id}/FindDocumentTemplate`
- `GET /v1/HedgeRelationship/{id}`
- `POST /v1/HedgeRelationship/Regress?hedgeResultType=Inception`
- `GET /v1/HedgeRelationship/IsAnalyticsAvailable`
- `POST /v1/HedgeRelationship/GenerateInceptionPackage?preview=false`

**Validation Rules**:
- Hedged and hedging items must exist
- Report currency is required
- Prospective and retrospective effectiveness methods required
- Hedged items must be in HA status
- Hedging items must be in Validated status
- Designation date cannot be in the future
- Fair Value hedges require fair value method and benchmark
- Cash Flow hedges require contractual rate
- Option hedges have specific item type requirements
- Off-market Cash Flow hedges require amortization schedule

### 2. Redraft Workflow

**Handler**: `RedraftHedgeRelationship.Command`

**Process Flow**:
1. Get current hedge relationship
2. Validate redraft requirements (RedraftValidator)
3. Call redraft API endpoint
4. Return updated hedge relationship in Draft state

**API Calls**:
- `GET /v1/HedgeRelationship/{id}`
- `POST /v1/HedgeRelationship/Redraft`

**Validation Rules**:
- Hedge must be in Designated or Dedesignated state
- Valid hedge relationship ID required

### 3. De-Designate Workflow

**Handler**: `DeDesignateHedgeRelationship.Command`
**Query Handler**: `GetDeDesignateData.Query`

**Process Flow**:
1. User selects de-designation reason
2. System fetches de-designation data for that reason
3. User reviews and confirms de-designation parameters
4. Get current hedge relationship
5. Validate de-designation requirements (DeDesignateValidator)
6. Update hedge with de-designation properties
7. Call de-designation API endpoint
8. Return updated hedge relationship

**API Calls**:
- `GET /v1/HedgeRelationship/Dedesignate/{id}?reason={reason}`
- `GET /v1/HedgeRelationship/{id}`
- `POST /v1/HedgeRelationship/Dedesignate`

**Validation Rules**:
- Hedge must be in Designated state
- Dedesignation date required and cannot be in future
- Dedesignation date must be after designation date
- Dedesignation reason required
- Warning if dedesignation within 3 months of designation

**De-designation Parameters**:
- Dedesignation Date
- Dedesignation Reason (enum)
- Payment amount
- Time Values Start/End Dates
- Cash Payment Type
- Hedged Exposure Exist flag
- Basis Adjustment amounts

### 4. Re-Designate Workflow

**Handler**: `ReDesignateHedgeRelationship.Command`
**Query Handler**: `GetReDesignateData.Query`

**Process Flow**:
1. Check if document template exists
2. If exists, save current state
3. Fetch re-designation data
4. User reviews and confirms re-designation parameters
5. Get current hedge relationship
6. Validate re-designation requirements (ReDesignateValidator)
7. Update hedge with re-designation properties
8. Call re-designation API endpoint
9. Return updated hedge relationship

**API Calls**:
- `GET /v1/HedgeRelationship/{id}/FindDocumentTemplate`
- `GET /v1/HedgeRelationship/Redesignate/{id}`
- `GET /v1/HedgeRelationship/{id}`
- `POST /v1/HedgeRelationship/Redesignate`

**Validation Rules**:
- Hedge must be in Designated state
- Hedge type must be CashFlow (re-designation only for cash flow hedges)
- Redesignation date required and cannot be in future
- Redesignation date must be after designation date
- If dedesignation date exists, redesignation must be after it

**Re-designation Parameters**:
- Redesignation Date
- Payment amount
- Time Values Start/End Dates
- Payment Frequency
- Day Count Convention
- Pay Business Day Convention
- Adjusted Dates flag
- Mark as Acquisition flag

## Error Handling

### Validation Errors
All validators return `List<string>` of error messages:
```csharp
var validationErrors = DesignationRequirementsValidator.Validate(hedgeRelationship);
if (validationErrors.Any())
{
    // Display errors to user
    return new Response(true, string.Join("; ", validationErrors));
}
```

### API Errors
All handlers catch and log ApiException:
```csharp
catch (ApiException apiEx)
{
    _logger.LogError(apiEx, 
        "API error for hedge ID: {Id}. Status: {Status}, Response: {Response}", 
        hedgeId, apiEx.StatusCode, apiEx.Response);
    return new Response(true, $"Failed: {apiEx.Message}");
}
```

### UI Error Display
Errors are shown via toast notifications:
```csharp
if (response.HasError)
{
    await AlertService.ShowToast(
        response.ErrorMessage, 
        AlertKind.Error, 
        "Operation Failed", 
        showButton: true);
    return;
}
```

## Usage Examples

### Designating a Hedge Relationship

```csharp
// In UI component
private async Task HandleDesignateAsync()
{
    // Validate
    ValidationErrors = DesignationRequirementsValidator.Validate(HedgeRelationship);
    if (ValidationErrors.Any())
    {
        StateHasChanged();
        return;
    }

    // Save first
    await SaveHedgeRelationshipAsync();

    // Execute designation
    var response = await Mediator.Send(
        new DesignateHedgeRelationship.Command(HedgeId));
    
    if (!response.HasError)
    {
        HedgeRelationship = response.HedgeRelationship;
        await AlertService.ShowToast("Successfully designated", ...);
    }
}
```

### De-Designating a Hedge Relationship

```csharp
// Load de-designation data
var dataResponse = await Mediator.Send(
    new GetDeDesignateData.Query(
        HedgeId, 
        DerivativeEDGEHAEntityEnumDedesignationReason.VoluntaryTermination));

// Execute de-designation
var response = await Mediator.Send(
    new DeDesignateHedgeRelationship.Command(
        HedgeRelationshipId: HedgeId,
        DedesignationDate: DateTime.Today,
        DedesignationReason: 1,
        Payment: 0,
        TimeValuesStartDate: DateTime.Today,
        TimeValuesEndDate: DateTime.Today.AddMonths(6),
        CashPaymentType: 0,
        HedgedExposureExist: true));
```

## Testing Considerations

### Unit Testing
Each handler should have tests covering:
- Successful operation
- Validation failures
- API exceptions
- State transitions

Example test structure:
```csharp
[Fact]
public async Task Handle_ValidDesignation_ReturnsSuccess()
{
    // Arrange
    var command = new DesignateHedgeRelationship.Command(hedgeId);
    
    // Act
    var response = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    Assert.False(response.HasError);
    Assert.NotNull(response.HedgeRelationship);
    Assert.NotNull(response.InceptionPackage);
}
```

### Integration Testing
Test workflow transitions:
1. Create hedge (Draft state)
2. Designate (Draft → Designated)
3. Redraft (Designated → Draft)
4. Designate again
5. De-Designate (Designated → Dedesignated)
6. Redraft (Dedesignated → Draft)
7. Designate for Cash Flow hedge
8. Re-Designate (Designated → Designated with new parameters)

## Migration from Legacy Code

### Key Differences from AngularJS Implementation

1. **State Management**:
   - Legacy: Client-side state in `$scope.Model`
   - New: Server-side state via API, UI state in component properties

2. **Validation**:
   - Legacy: Inline validation in controller
   - New: Dedicated validator classes

3. **API Communication**:
   - Legacy: Direct `$http` calls
   - New: Typed API client with proper async/await

4. **Error Handling**:
   - Legacy: Global error array `$scope.ha_errors`
   - New: Structured error responses with toast notifications

5. **Workflow Logic**:
   - Legacy: Single `onChangeActionValue` function
   - New: Dedicated handler for each workflow operation

### Preserved Business Rules

All business rules from `hr_hedgeRelationshipAddEditCtrl.js` have been preserved:
- Workflow state transitions
- Validation requirements
- Date validations
- API call sequences
- User permission checks (via validators)

## Performance Considerations

1. **Async Operations**: All API calls are fully async
2. **Validation**: Client-side validation before API calls
3. **State Updates**: Minimal state updates to reduce re-renders
4. **Error Logging**: Comprehensive logging for debugging
5. **Cancellation Tokens**: Support for operation cancellation

## Security Considerations

1. **API Authentication**: Handled via TokenProvider
2. **Authorization**: Server-side validation of permissions
3. **Input Validation**: All inputs validated before API calls
4. **Audit Trail**: All operations logged with user context

## Future Enhancements

Potential improvements:
1. Add unit tests for all handlers
2. Implement workflow state machine
3. Add workflow history tracking
4. Implement optimistic UI updates
5. Add retry logic for failed API calls
6. Implement bulk workflow operations
7. Add workflow approval process
8. Implement workflow notifications

## Troubleshooting

### Common Issues

**Issue**: Designation fails with "Hedged items must be in HA status"
**Solution**: Verify all hedged items have correct status before designation

**Issue**: Re-designation not available
**Solution**: Ensure hedge is Designated and hedge type is CashFlow

**Issue**: API calls fail with 401 Unauthorized
**Solution**: Check TokenProvider authentication setup

**Issue**: Validation passes but API rejects
**Solution**: Server-side validation may have additional rules; check API logs

## Support

For questions or issues:
1. Check validation error messages
2. Review handler logging output
3. Verify API endpoint availability
4. Check user permissions
5. Consult legacy code for business rule clarification
