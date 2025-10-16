# Hedge Relationship Workflow Implementation Documentation

## Overview
This document describes the implementation of the Hedge Relationship workflow supporting multiple operational states: Draft, Designate, Redraft, De-Designate, and Re-Designate.

## Workflow States and Transitions

### State Diagram
```
Draft
  ↓ (Designate)
Designated
  ↓ (De-Designate)  ↓ (Redraft)  ↓ (Re-Designate - CashFlow only)
Dedesignated       Draft         Designated
  ↓ (Redraft)
Draft
```

### State Transition Rules

#### 1. Draft → Designate
**Trigger**: User clicks "Designate" in workflow dropdown  
**Prerequisites**:
- Hedge relationship must be in Draft state
- All designation requirements must be met (see DesignationRequirementsValidator)
- User must have required role (24, 17, or 5)

**Process**:
1. Validate designation requirements
2. Save hedge relationship
3. Check for document template existence (optional)
4. Check analytics service availability
5. Run regression with HedgeResultType.Inception
6. Update UI to show Designated state

**API Calls**:
- `POST /HedgeRelationship` - Save hedge relationship
- `GET /HedgeRelationship/FindDocumentTemplate/{id}` - Check for document template
- `GET /HedgeRelationship/IsAnalyticsAvailable` - Check analytics status
- `POST /HedgeRelationship/Regress?hedgeResultType=Inception` - Run designation regression

---

#### 2. Designated → De-Designate
**Trigger**: User clicks "De-Designate" in workflow dropdown  
**Prerequisites**:
- Hedge relationship must be in Designated state
- Must have hedging items
- User must have required role (24, 17, or 5)

**Process**:
1. Validate de-designation requirements
2. Open De-Designation modal to collect:
   - Dedesignation Date
   - Dedesignation Reason
   - Termination flag
   - Ineffectiveness flag
   - Time Values Start/End Dates
   - Cash Payment Type (Full/Partial/None)
   - Hedged Exposure Exists flag
   - Payment, Accrual, Basis Adjustment amounts
3. Calculate accrual from hedging instrument if applicable
4. Submit de-designation request
5. Update UI to show Dedesignated state

**API Calls**:
- `GET /{SecurityType}/Price?id={itemId}&valueDate={date}&instance=Last&discCurve=OIS` - Get accrual data for hedging instrument
- `POST /HedgeRelationship/Dedesignate` - Execute de-designation

**Modal Fields**:
```typescript
{
  DedesignationDate: DateTime,
  DedesignationReason: number,
  Termination: boolean,
  Ineffectiveness: boolean,
  TimeValuesStartDate: DateTime,
  TimeValuesEndDate: DateTime,
  FullCashPayment: boolean,
  PartialCashPayment: boolean,
  NoCashPayment: boolean,
  HedgedExposureNotExist: boolean,
  Payment: decimal,
  Accrual: decimal,
  BasisAdjustment: decimal,
  BasisAdjustmentBalance: decimal,
  CashPaymentType: number,
  HedgedExposureExist: boolean
}
```

---

#### 3. Designated → Redraft
**Trigger**: User clicks "Redraft" in workflow dropdown  
**Prerequisites**:
- Hedge relationship must be in Designated or Dedesignated state
- User must have required role (24, 17, or 5)

**Process**:
1. Validate redraft requirements
2. Confirm action with user
3. Handle option time value amortization cleanup if applicable
4. Call Redraft API to change state back to Draft
5. Update UI to show Draft state

**API Calls**:
- `DELETE /HedgeRelationshipOptionTimeValueAmort/{id}` - Delete option amortization if exists (optional)
- `POST /HedgeRelationship/Redraft` - Execute redraft

**Note**: Legacy code shows that if there's a selected option time value amortization, it should be deleted before executing the redraft.

---

#### 4. Designated → Re-Designate (CashFlow Only)
**Trigger**: User clicks "Re-Designate" in workflow dropdown  
**Prerequisites**:
- Hedge relationship must be in Designated state
- Hedge Type must be CashFlow
- User must have required role (24, 17, or 5)

**Process**:
1. Validate re-designation requirements
2. Save hedge relationship
3. Check analytics service availability
4. Retrieve re-designation data from API
5. Open Re-Designation modal to collect/edit:
   - Redesignation Date
   - Time Values Start/End Dates
   - Day Count Convention
   - Payment Business Day Convention
   - Payment Frequency
   - Adjusted Dates flag
   - Mark As Acquisition flag
6. Submit re-designation request
7. Update UI (state remains Designated)

**API Calls**:
- `POST /HedgeRelationship` - Save hedge relationship
- `GET /HedgeRelationship/IsAnalyticsAvailable` - Check analytics status
- `GET /HedgeRelationship/Redesignate/{id}` - Get re-designation data
- `POST /HedgeRelationship/Redesignate` - Execute re-designation

**Modal Fields**:
```typescript
{
  RedesignationDate: DateTime,
  TimeValuesStartDate: DateTime,
  TimeValuesEndDate: DateTime,
  Payment: decimal,
  DayCountConv: string,
  PayBusDayConv: string,
  PaymentFrequency: string,
  AdjustedDates: boolean,
  MarkAsAcquisition: boolean,
  IsDocTemplateFound: boolean
}
```

---

#### 5. Dedesignated → Redraft
**Trigger**: User clicks "Redraft" in workflow dropdown  
**Prerequisites**:
- Hedge relationship must be in Dedesignated state
- User must have required role (24, 17, or 5)

**Process**:
Same as "Designated → Redraft" above.

---

## Validation Requirements

### Designation Requirements (DesignationRequirementsValidator)
- ✅ Hedged Items and Hedging Items must exist
- ✅ Report Currency must be specified
- ✅ Prospective Effectiveness Method must be specified
- ✅ Retrospective Effectiveness Method must be specified
- ✅ Hedged Items must be in HA status
- ✅ Hedging Items must be in Validated status
- ✅ If Dedesignation Date exists, it must be later than Designation Date
- ✅ Hedge Type must be specified
- ✅ For FairValue: Fair Value Method and Benchmark must be specified
- ✅ For CashFlow (non-FX): Contractual Rate (Benchmark) must be specified
- ✅ Hedged Item Type must be specified
- ✅ Asset/Liability must be specified
- ✅ Designation Date must not be in the future
- ✅ Option hedge items validation (if IsAnOptionHedge is true)
- ✅ CashFlow + OffMarket requires Amortization schedule

### De-Designation Requirements (DeDesignateRequirementsValidator)
- Hedge State must be Designated
- Must have Hedging Items

### Redraft Requirements (RedraftRequirementsValidator)
- Hedge State must be Designated or Dedesignated

### Re-Designation Requirements (ReDesignateRequirementsValidator)
- Hedge State must be Designated
- Hedge Type must be CashFlow

---

## User Role Permissions

The following roles have permission to execute workflow actions:
- **Role 24**: Full workflow access
- **Role 17**: Full workflow access
- **Role 5**: Full workflow access

Users without these roles can only perform actions when the hedge relationship is in Draft state.

---

## UI Components

### Workflow Dropdown
Location: `HedgeRelationshipDetails.razor` (lines 98-103)

The workflow dropdown is dynamically populated based on the current hedge state:

**Draft State**:
- Designate

**Designated State**:
- Redraft
- De-Designate
- Re-Designate (CashFlow only)

**Dedesignated State**:
- Redraft

### BuildWorkflowItems Method
Location: `HedgeRelationshipDetails.razor.cs` (method BuildWorkflowItems)

This method constructs the workflow dropdown items based on:
- Current HedgeState
- Hedge Type
- User permissions

---

## Implementation Files

### Created/Modified Files

1. **HedgeRelationshipDetails.razor.cs** - Main workflow logic
   - `HandleWorkflowAction()` - Main dispatcher
   - `HandleDesignateAsync()` - Designation handler
   - `HandleDeDesignateAsync()` - De-designation handler (stub)
   - `HandleRedraftAsync()` - Redraft handler (stub)
   - `HandleReDesignateAsync()` - Re-designation handler (stub)
   - `ExecuteDesignationAsync()` - Designation execution logic
   - `BuildWorkflowItems()` - Workflow dropdown builder

2. **DesignationRequirementsValidator.cs** - Validation for Designate action
3. **DeDesignateRequirementsValidator.cs** - Validation for De-Designate action (NEW)
4. **RedraftRequirementsValidator.cs** - Validation for Redraft action (NEW)
5. **ReDesignateRequirementsValidator.cs** - Validation for Re-Designate action (NEW)

### Pending Implementation

1. **De-Designation Modal Component** - Dialog for de-designation workflow
2. **Re-Designation Modal Component** - Dialog for re-designation workflow
3. **API Handlers**:
   - Designate with Inception parameter
   - De-Designate endpoint
   - Redraft endpoint
   - Re-Designate endpoints (GET and POST)
   - FindDocumentTemplate endpoint
   - GetTerminationDate endpoint

---

## Testing Considerations

### Manual Test Scenarios

1. **Draft to Designated**
   - Create new hedge relationship
   - Fill in all required fields
   - Click Designate
   - Verify regression runs
   - Verify state changes to Designated

2. **Designated to De-Designated**
   - Start with Designated hedge
   - Click De-Designate
   - Fill in modal fields
   - Submit
   - Verify state changes to Dedesignated

3. **Designated to Draft (Redraft)**
   - Start with Designated hedge
   - Click Redraft
   - Confirm action
   - Verify state changes to Draft

4. **Designated to Re-Designated (CashFlow only)**
   - Start with Designated CashFlow hedge
   - Click Re-Designate
   - Fill in modal fields
   - Submit
   - Verify state remains Designated but data is updated

5. **Dedesignated to Draft (Redraft)**
   - Start with Dedesignated hedge
   - Click Redraft
   - Confirm action
   - Verify state changes to Draft

### Role-Based Testing
- Test each workflow action with users having roles 24, 17, and 5
- Test that users without these roles cannot execute workflow actions (except in Draft state)

---

## Code Patterns from Legacy Implementation

### Document Template Checking
From legacy code (lines 1680-1701):
```javascript
function initiateDesignation() {
    $haService
        .setUrl("HedgeRelationship/FindDocumentTemplate/" + id)
        .get()
        .then(function (response) {
            if (response.data) {
                // Template exists - save first then designate
                $scope.submit(undefined, function () {
                    $scope.init(id, function () {
                        designate(checkDocumentTemplateKeywordsOnDesignated);
                    });
                }, "InitiateDesignation");
            }
            else {
                // No template - proceed with designation
                designate(function () {
                    $scope.generatePackage(false);
                });
            }
        });
}
```

### Analytics Service Checking
From legacy code (lines 1558-1571):
```javascript
function checkAnalyticsStatus(callback) {
    $haService
        .setUrl("HedgeRelationship/IsAnalyticsAvailable")
        .get()
        .then(function (response) {
            var proceed = response.data;
            if (!response.data) {
                proceed = confirm("Analytics service is currently unavailable. Are you sure you want to continue?");
            }
            if (proceed) {
                callback();
            }
        });
}
```

### Option Time Value Amortizations
From legacy code (lines 1573-1583):
```javascript
function setHedgeRelationshipOptionTimeValueAmorts() {
    if ($scope.Model.HedgeRelationshipOptionTimeValueAmorts === undefined) {
        $scope.Model.HedgeRelationshipOptionTimeValueAmorts = [];
    }

    if ($scope.Model.HedgeRelationshipOptionTimeValues !== undefined) {
        $scope.Model.HedgeRelationshipOptionTimeValues.map(function (v) {
            $scope.Model.HedgeRelationshipOptionTimeValueAmorts.push(v);
        });
    }
}
```

---

## Notes and Considerations

1. **Analytics Service**: All designation and re-designation operations check analytics service availability before proceeding.

2. **Document Templates**: The designation flow checks for document template existence and may require additional keyword checking after designation is complete.

3. **Option Hedge Cleanup**: When redrafting, if there are option time value amortizations selected, they should be deleted before the redraft operation.

4. **CashFlow-Specific**: Re-designation is only available for CashFlow hedge types (legacy ticket DE-3928).

5. **Dedesignated Redraft**: The ability to redraft from Dedesignated state was added in legacy ticket DE-2731.

6. **Confirmation Dialogs**: Both Redraft and certain analytics scenarios require user confirmation before proceeding.

7. **Inception Package**: After successful designation, the system may automatically generate an inception package.

---

## Future Enhancements

1. Create reusable modal components for De-Designation and Re-Designation
2. Implement full API integration when backend endpoints are available
3. Add unit tests for all validators
4. Add integration tests for workflow state transitions
5. Implement document template keyword checking
6. Add audit logging for workflow state changes
7. Implement option time value amortization cleanup for redraft operations

---

## References

- Legacy Implementation: `HedgeRelationshipDetailsLegacy.js`
- New Implementation: `HedgeRelationshipDetails.razor.cs`
- Validators: `Features/HedgeRelationships/Validation/*RequirementsValidator.cs`
