# Re-Designation Workflow - Visual Diagram

## High-Level Flow Comparison

### Legacy AngularJS Flow
```
User clicks "Re-Designate" in workflow dropdown
    ↓
onChangeActionValue("Re-Designate")
    ↓
initiateReDesignation()
    ├─→ Check if document template exists
    │   ├─→ If YES: Save hedge relationship → Reload → Wait 1000ms → Call reDesignate(true)
    │   └─→ If NO: Call reDesignate(false)
    ↓
reDesignate(isDocTemplateFound)
    ├─→ Check analytics status (IsAnalyticsAvailable)
    │   ├─→ If UNAVAILABLE: Show confirm dialog
    │   │   ├─→ User clicks Cancel → STOP
    │   │   └─→ User clicks OK → Continue
    │   └─→ If AVAILABLE: Continue
    ↓
    ├─→ Call API: GET /HedgeRelationship/Redesignate/{id}
    ↓
    ├─→ Populate $scope.Model with response data
    │   - RedesignationDate
    │   - TimeValuesStartDate
    │   - TimeValuesEndDate
    │   - Payment = 0 (hardcoded)
    │   - DayCountConv
    │   - PayBusDayConv
    │   - PaymentFrequency
    │   - AdjustedDates
    │   - MarkAsAcquisition
    │   - IsDocTemplateFound
    ↓
ngDialog.open("redesignateDialog")
    ↓
┌─────────────────────────────────────────────┐
│   Re-Designation Workflow Modal            │
│                                             │
│   [Redesignation Date      ] *              │
│   [Payment Frequency    ▼] *                │
│   [Day Count Convention ▼] *                │
│   [Payment Business Day ▼] *                │
│   [Start Date          ] *                  │
│   [End Date            ] *                  │
│   [Amount: 0.00        ] *                  │
│   [☐] Adjusted Dates                        │
│   [☐] Mark as Acquisition                   │
│                                             │
│   [Cancel]  [Re-Designate]                  │
└─────────────────────────────────────────────┘
    ↓
User fills form and clicks "Re-Designate"
    ↓
isRedesignationValid() checks all fields
    ├─→ If INVALID: Button disabled
    └─→ If VALID: Button enabled
    ↓
reDesignate() (button action)
    ↓
POST /HedgeRelationship/Redesignate
    ↓
Success → Update hedge relationship → Close modal
```

### New Blazor Server Flow
```
User clicks "Re-Designate" in workflow dropdown
    ↓
HandleWorkflowAction("Re-Designate")
    ↓
HandleReDesignateAsync()
    ├─→ Step 1: Check analytics status (NEW FIX!)
    │   ├─→ Send CheckAnalyticsStatus.Query via MediatR
    │   │   └─→ Handler calls: hedgeAccountingApiClient.IsAnalyticsAvailableAsync()
    │   ↓
    │   ├─→ If ERROR: Show toast → STOP
    │   ├─→ If UNAVAILABLE: Show JSRuntime confirm dialog
    │   │   ├─→ User clicks Cancel → STOP
    │   │   └─→ User clicks OK → Continue
    │   └─→ If AVAILABLE: Continue
    ↓
    ├─→ Step 2: Check if document template exists
    │   ├─→ Send FindDocumentTemplate.Query via MediatR
    │   │   └─→ Handler calls: hedgeAccountingApiClient.FindDocumentTemplateAsync()
    │   ↓
    │   ├─→ If ERROR: Show toast → STOP
    │   ├─→ If template EXISTS:
    │   │   ├─→ SaveHedgeRelationshipAsync()
    │   │   └─→ GetHedgeRelationship(HedgeRelationshipId)
    │   └─→ Store: IsDocTemplateFound
    ↓
    ├─→ Step 3: Get re-designation data
    │   ├─→ Send GetReDesignateData.Query via MediatR
    │   │   └─→ Handler calls: hedgeAccountingApiClient.RedesignateGETAsync(id)
    │   ↓
    │   ├─→ If ERROR: Show toast → STOP
    │   └─→ Populate properties:
    │       - RedesignationDate
    │       - RedesignateTimeValuesStartDate
    │       - RedesignateTimeValuesEndDate
    │       - RedesignatePayment (defaults to 0 in component)
    │       - RedesignateDayCountConv
    │       - RedesignatePayBusDayConv
    │       - RedesignatePaymentFrequency
    │       - RedesignateAdjustedDates
    │       - MarkAsAcquisition
    ↓
    └─→ Step 4: Show Re-Designation dialog
        ├─→ Set: OpenModal = MODAL_REDESIGNATE
        └─→ StateHasChanged()
    ↓
<ReDesignateDialog> component renders
    ↓
┌─────────────────────────────────────────────┐
│   Re-Designation Workflow                  │
│                                             │
│   Redesignation Date *  [mm/dd/yyyy]        │
│   Time Values Start Date * [mm/dd/yyyy]     │
│   Time Values End Date *   [mm/dd/yyyy]     │
│   Payment * [0.00]                          │
│   Payment Frequency * [Select...        ▼]  │
│   Day Count Convention * [Select...     ▼]  │
│   Payment Business Day * [Select...     ▼]  │
│   [☐] Adjusted Dates                        │
│   [☐] Mark as Acquisition                   │
│                                             │
│   [Cancel]  [Re-Designate]                  │
└─────────────────────────────────────────────┘
    ↓
User fills form and clicks "Re-Designate"
    ↓
IsValid property checks all fields (computed)
    ├─→ If INVALID: Button.Disabled = true
    └─→ If VALID: Button.Disabled = false
    ↓
HandleReDesignate() in component
    ├─→ Validates again
    └─→ Invokes: OnReDesignated callback
    ↓
OnReDesignateConfirmed() in page
    ├─→ Create ReDesignateHedgeRelationship.Command
    │   └─→ Pass all form field values
    ↓
    ├─→ Send command via MediatR
    │   └─→ Handler:
    │       ├─→ Fetch current hedge relationship
    │       ├─→ Validate redesignation requirements
    │       ├─→ Map to entity + update redesignation properties
    │       └─→ Call: hedgeAccountingApiClient.RedesignatePOSTAsync()
    ↓
    ├─→ If ERROR: Show toast → Keep modal open
    └─→ If SUCCESS:
        ├─→ Update: HedgeRelationship = response.Data
        ├─→ Close modal: OpenModal = string.Empty
        └─→ Show success toast
```

## Key Differences Explained

### 1. Architecture Pattern
| Aspect | Legacy | New |
|--------|--------|-----|
| **Pattern** | Direct function calls | CQRS with MediatR |
| **API Layer** | `$haService` wrapper | Auto-generated API client |
| **State Management** | `$scope` two-way binding | Component parameters + EventCallback |
| **Async Handling** | Promises (`.then()`) | async/await |

### 2. Analytics Check Timing
| Legacy | New (Fixed) |
|--------|-------------|
| Called **inside** `reDesignate()` | Called **at start** of `HandleReDesignateAsync()` |
| Uses `checkAnalyticsStatus(callback)` pattern | Uses async/await with MediatR query |
| Browser `confirm()` directly | JSRuntime.InvokeAsync&lt;bool&gt;("confirm") |

### 3. Document Template Handling
| Legacy | New |
|--------|-----|
| 1000ms timeout after reload | No timeout (async/await handles sequencing) |
| Nested callbacks | Linear async/await flow |
| Same functional behavior | ✅ Same functional behavior |

### 4. Modal Display
| Legacy | New |
|--------|-----|
| ngDialog.open() | Component visibility binding |
| Template ID: "redesignateDialog" | Component: &lt;ReDesignateDialog&gt; |
| Shared $scope | Explicit parameter passing |

### 5. Validation
| Legacy | New |
|--------|-----|
| Function: `isRedesignationValid()` | Property: `IsValid` (computed getter) |
| Called on every digest cycle | Computed on access |
| Same rules | ✅ Same rules |

### 6. Form Submission
| Legacy | New |
|--------|-----|
| Direct POST via scope | Command pattern with MediatR |
| Implicit model binding | Explicit command properties |
| Response updates $scope | Response updates component state |

## Component Communication Flow

### Legacy: Scope-Based
```
Controller ($scope)
    ↓ (shared scope)
Modal Template ($scope)
    ↓ (ng-model)
Form Fields ($scope.Model.*)
```

### New: Parameter-Based
```
HedgeRelationshipDetails Page
    ↓ (property parameters)
ReDesignateDialog Component
    ↓ (@bind-Value)
Form Fields (component properties)
    ↓ (EventCallback)
OnReDesignateConfirmed()
```

## Error Handling Comparison

### Legacy
```javascript
// Error handling implicit in promises
.then(function(response) { /* success */ })
.catch(function(error) { /* error handling minimal */ });
```

### New
```csharp
try
{
    // Step 1
    if (response.HasError) { /* show toast and return */ }
    
    // Step 2
    if (response.HasError) { /* show toast and return */ }
    
    // Step 3
    if (response.HasError) { /* show toast and return */ }
    
    // Step 4
    if (response.HasError) { /* show toast and return */ }
}
catch (Exception ex)
{
    // Catch-all error handler
}
```

**Improvement**: New implementation has explicit error handling at each step with user-friendly messages.

## Validation Rules (Identical in Both)

1. ✅ Payment must exist and ≠ 0
2. ✅ Redesignation Date must be valid
3. ✅ Time Values Start Date must be valid
4. ✅ Time Values End Date must be valid
5. ✅ Start Date must be < End Date
6. ✅ Payment Business Day Convention must not be empty
7. ✅ Payment Frequency must not be empty
8. ✅ Day Count Convention must not be empty

## Summary

### Functional Equivalence: ✅ 100%
- All legacy logic migrated
- All form fields present
- All validation rules implemented
- Analytics check now included (fixed)

### Architectural Improvements: 🎯
- ✅ Explicit error handling
- ✅ Type safety with C#
- ✅ CQRS pattern for separation of concerns
- ✅ Auto-generated API client
- ✅ Component-based UI
- ✅ Modern async/await patterns

### Testing Status
- ⚠️ Manual testing required for analytics check
- ⚠️ End-to-end Re-Designation workflow test needed
- ✅ All code committed and documented
