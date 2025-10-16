# Hedge Relationship Workflow API Integration Guide

This document outlines the API endpoints and MediatR handlers that need to be implemented to complete the workflow functionality.

## Overview

The workflow implementation includes 5 operational states:
1. **Draft** - Initial state, can Designate
2. **Designate** - Validates and designates the hedge relationship
3. **Designated** - Active state, can De-Designate, Redraft, or Re-Designate (CashFlow only)
4. **De-Designate** - Removes designation
5. **Dedesignated** - Inactive state, can Redraft
6. **Redraft** - Returns to Draft state
7. **Re-Designate** - Modifies designation (CashFlow only)

## Required API Endpoints

### 1. Designate Workflow

#### FindDocumentTemplate Query
**Purpose**: Check if a hedge document template exists for the relationship

```csharp
// Query
public class FindDocumentTemplate
{
    public record Query(long HedgeRelationshipId) : IRequest<Response>;
    
    public record Response
    {
        public bool HasTemplate { get; set; }
    }
}
```

**Legacy Endpoint**: `GET /api/HedgeRelationship/FindDocumentTemplate/{id}`

#### RunRegression Command
**Purpose**: Execute regression analysis for inception

```csharp
// Command
public class RunRegression
{
    public record Command(
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship,
        string HedgeResultType
    ) : IRequest<Response>;
    
    public record Response
    {
        public bool Success { get; set; }
    }
}
```

**Legacy Endpoint**: `POST /api/HedgeRelationship/Regress?hedgeResultType=Inception`

**Notes**: 
- Should call `checkAnalyticsStatus` before running
- Should set `HedgeRelationshipOptionTimeValueAmorts` before calling

#### GenerateInceptionPackage Command
**Purpose**: Generate the inception package document

```csharp
// Command
public class GenerateInceptionPackage
{
    public record Command(
        DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship,
        bool Preview
    ) : IRequest<FileResponse>;
}
```

**Legacy Endpoint**: `POST /api/HedgeRelationship/GenerateInceptionPackage?preview={bool}`

---

### 2. De-Designate Workflow

#### GetTerminationDate Query
**Purpose**: Retrieve the termination date for a trade/hedging item

```csharp
// Query
public class GetTerminationDate
{
    public record Query(long ItemId) : IRequest<Response>;
    
    public record Response
    {
        public DateTime? TerminationDate { get; set; }
    }
}
```

**Legacy Endpoint**: `GET /api/Trade/GetTerminationDate/{itemId}`

#### PriceInstrument Query
**Purpose**: Price the instrument to calculate accrual amount

```csharp
// Query
public class PriceInstrument
{
    public record Query(
        long ItemId,
        DateTime ValueDate,
        string SecurityType
    ) : IRequest<Response>;
    
    public record Response
    {
        public decimal? Accrual { get; set; }
        // Other pricing data...
    }
}
```

**Legacy Endpoint**: Varies by security type, e.g.:
- `GET /api/SwapSummary/Price?id={id}&valueDate={date}&instance=Last&discCurve=OIS`
- `GET /api/CapFloor/Price?id={id}&valueDate={date}&instance=Last&discCurve=OIS`
- `GET /api/Collar/Price?id={id}&valueDate={date}&instance=Last&discCurve=OIS&userAction=price_collar`

**Notes**: Endpoint varies based on SecurityType (Swap, CapFloor, Debt, Swaption, Collar, etc.)

#### GetDedesignateData Query
**Purpose**: Load de-designation data based on reason (Termination or Ineffectiveness)

```csharp
// Query
public class GetDedesignateData
{
    public record Query(long HedgeRelationshipId, int Reason) : IRequest<Response>;
    
    public record Response
    {
        public DateTime? DedesignationDate { get; set; }
        public decimal? Payment { get; set; }
        public DateTime? TimeValuesEndDate { get; set; }
        public bool ShowBasisAdjustmentBalance { get; set; }
        public decimal? BasisAdjustment { get; set; }
        public decimal? BasisAdjustmentBalance { get; set; }
        public string ErrorMessage { get; set; }
    }
}
```

**Legacy Endpoint**: `GET /api/HedgeRelationship/Dedesignate/{id}/{reason}`

**Business Rules**:
- Reason 0 = Termination: Check if hedge item status is "Terminated"
- Reason 1 = Ineffectiveness: Different validation rules
- Returns error message if validation fails

#### DeDesignateHedgeRelationship Command
**Purpose**: Execute the de-designation

```csharp
// Command
public class DeDesignateHedgeRelationship
{
    public record Command(
        long HedgeRelationshipId,
        DateTime? DedesignationDate,
        int DedesignationReason,
        decimal? Payment,
        DateTime? TimeValuesStartDate,
        DateTime? TimeValuesEndDate,
        int CashPaymentType,
        bool HedgedExposureExist
    ) : IRequest<Response>;
    
    public record Response
    {
        public bool Success { get; set; }
    }
}
```

**Legacy Implementation**: Updates HedgeState to "Dedesignated" and saves related data

---

### 3. Redraft Workflow

#### DeleteOptionTimeValueAmort Command (Optional)
**Purpose**: Delete option time value amortization if it exists

```csharp
// Command
public class DeleteOptionTimeValueAmort
{
    public record Command(long AmortizationId) : IRequest<Response>;
    
    public record Response
    {
        public bool Success { get; set; }
    }
}
```

**Legacy Endpoint**: `DELETE /api/HedgeRelationshipOptionTimeValueAmort/{id}`

#### RedraftHedgeRelationship Command
**Purpose**: Move hedge relationship back to Draft state

```csharp
// Command
public class RedraftHedgeRelationship
{
    public record Command(long HedgeRelationshipId) : IRequest<Response>;
    
    public record Response
    {
        public bool Success { get; set; }
    }
}
```

**Legacy Endpoint**: `POST /api/HedgeRelationship/Redraft`

**Business Rules**:
- Changes HedgeState from Designated or Dedesignated to Draft
- May need to clean up option time value amortizations

---

### 4. Re-Designate Workflow

#### GetRedesignateData Query
**Purpose**: Load defaults and prepare data for re-designation

```csharp
// Query
public class GetRedesignateData
{
    public record Query(long HedgeRelationshipId) : IRequest<Response>;
    
    public record Response
    {
        public DateTime? RedesignationDate { get; set; }
        public DateTime? TimeValuesStartDate { get; set; }
        public DateTime? TimeValuesEndDate { get; set; }
        public string DayCountConv { get; set; }
        public string PayBusDayConv { get; set; }
        public string PaymentFrequency { get; set; }
        public bool AdjustedDates { get; set; }
        public bool MarkAsAcquisition { get; set; }
    }
}
```

**Legacy Endpoint**: `GET /api/HedgeRelationship/Redesignate/{id}`

**Notes**: Must call `checkAnalyticsStatus` before executing

#### ReDesignateHedgeRelationship Command
**Purpose**: Execute the re-designation

```csharp
// Command
public class ReDesignateHedgeRelationship
{
    public record Command(
        long HedgeRelationshipId,
        DateTime? RedesignationDate,
        DateTime? TimeValuesStartDate,
        DateTime? TimeValuesEndDate,
        decimal? Payment,
        string PaymentFrequency,
        string DayCountConv,
        string PayBusDayConv,
        bool AdjustedDates,
        bool MarkAsAcquisition
    ) : IRequest<Response>;
    
    public record Response
    {
        public bool Success { get; set; }
    }
}
```

**Business Rules**:
- Only available for CashFlow hedge types
- Only available when HedgeState is Designated
- All date and payment fields are required

---

## Implementation Order

Recommended implementation order:

1. **Redraft** (Simplest)
   - `RedraftHedgeRelationship.Command`
   - `DeleteOptionTimeValueAmort.Command` (if needed)

2. **Designate** (Core functionality)
   - `FindDocumentTemplate.Query`
   - `RunRegression.Command`
   - `GenerateInceptionPackage.Command`

3. **De-Designate** (Medium complexity)
   - `GetTerminationDate.Query`
   - `PriceInstrument.Query`
   - `GetDedesignateData.Query`
   - `DeDesignateHedgeRelationship.Command`

4. **Re-Designate** (Most complex)
   - `GetRedesignateData.Query`
   - `ReDesignateHedgeRelationship.Command`

---

## Validation Rules

### Designation Requirements (Already Implemented)
See `DesignationRequirementsValidator.cs` for complete validation logic:
- Hedged and Hedging Items required
- Report currency required
- Effectiveness methods required (Prospective and Retrospective)
- Hedged items must be in HA status
- Hedging items must be in Validated status
- Hedge Type specific validations (FairValue vs CashFlow)
- Date validations
- Option hedge items validation
- Off-Market amortization schedule requirement

### Re-Designation Validation
From ReDesignateDialog component:
- Payment must be non-zero
- All dates must be valid
- TimeValuesStartDate must be before TimeValuesEndDate
- PayBusDayConv, PaymentFrequency, and DayCountConv are required

---

## User Role Permissions

Workflow actions require one of the following roles:
- Role 24
- Role 17
- Role 5

Implemented in `HasWorkflowPermission()` method.

---

## Testing Checklist

For each workflow:
- [ ] Workflow button appears/disappears based on HedgeState
- [ ] User role permissions are enforced
- [ ] Validation errors display correctly
- [ ] Dialog opens with correct default values
- [ ] API calls execute successfully
- [ ] HedgeState updates correctly after workflow completion
- [ ] Hedge relationship reloads with updated data
- [ ] Success/error messages display appropriately

---

## Legacy Code References

Key sections in `hr_hedgeRelationshipAddEditCtrl.js`:
- Lines 458-476: `setWorkFlow()` - Workflow button logic
- Lines 2621-2714: `designate()` - Designation logic
- Lines 2743-2770: `reDesignate()` - Re-designation logic
- Lines 2797-2890: De-designation logic
- Lines 2894-2923: Redraft logic

---

## Additional Notes

1. **Analytics Status Check**: Several workflows call `checkAnalyticsStatus()` before proceeding. This should be implemented as a separate query.

2. **Document Template Keywords**: The Designate workflow checks for empty keywords in templates and displays a warning.

3. **Error Handling**: All API calls should return appropriate error messages that can be displayed to users.

4. **Loading States**: The UI includes loading indicators for each workflow action. Backend should return appropriate status updates.

5. **Transaction Management**: Consider wrapping workflow operations in transactions to ensure data consistency.
