# Option Amortization Modal - Before and After Fix

## Problem
The OptionAmortization modal was opening with empty/default values instead of loading calculated defaults from the API.

## Data Flow - BEFORE (Broken)

```
User clicks "New" → "Option Amortization"
           ↓
NewMenuOnItemSelected() executes
           ↓
Initializes OptionAmortizationModel with hardcoded defaults:
  - GLAccountID = 0 (None)
  - ContraAccountID = 0 (None)
  - TotalAmount = 0 (empty)
  - IntrinsicValue = 0 (empty)
  - IVGLAccountID = 0 (None)
  - IVContraAccountID = 0 (None)
  - IVAmortizationMethod = None
           ↓
Modal opens with EMPTY VALUES ❌
```

## Data Flow - AFTER (Fixed)

```
User clicks "New" → "Option Amortization"
           ↓
NewMenuOnItemSelected() executes
           ↓
InitializeOptionAmortizationModelAsync() called
           ↓
GetOptionAmortizationDefaults.Query sent to MediatR
           ↓
Handler executes:
  1. Maps HedgeRelationshipVM → HedgeRelationship Entity (AutoMapper)
  2. Calls API: POST /v1/HedgeRelationship/GetOptionAmortizationDefaults
  3. API returns OptionAmortizationDefaultValues:
     - GlAccountId (calculated from hedge setup)
     - GlContraAcctId (calculated from hedge setup)
     - GlAccountId2 (for intrinsic value)
     - GlContraAcctId2 (for intrinsic value)
     - IntrinsicValue (calculated from hedge items)
     - IVAmortizationMethod (default method)
     - TimeValue (calculated option premium)
           ↓
OptionAmortizationModel initialized with API values:
  - GLAccountID = defaults.GlAccountId
  - ContraAccountID = defaults.GlContraAcctId
  - TotalAmount = defaults.TimeValue
  - IntrinsicValue = defaults.IntrinsicValue
  - IVGLAccountID = defaults.GlAccountId2
  - IVContraAccountID = defaults.GlContraAcctId2
  - IVAmortizationMethod = defaults.IVAmortizationMethod
  - StartDate = HedgeRelationship.DesignationDate
  - EndDate = HedgingItems[0].MaturityDate
  - AmortizationMethod = HedgeRelationship.AmortizationMethod
           ↓
Modal opens with PRE-POPULATED VALUES ✅
```

## Code Changes Summary

### 1. New Handler Class
```csharp
// File: GetOptionAmortizationDefaults.cs
public sealed class GetOptionAmortizationDefaults
{
    public sealed record Query(HedgeRelationshipVM HedgeRelationship) : IRequest<Response>;
    
    public sealed class Response : ResponseBase
    {
        public OptionAmortizationDefaultValues Data { get; set; }
    }
    
    public sealed class Handler : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
        {
            // Map VM to Entity
            var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(query.HedgeRelationship);
            
            // Call API
            var defaults = await hedgeAccountingApiClient.GetOptionAmortizationDefaultsAsync(apiEntity, cancellationToken);
            
            return new Response(false, "Success", defaults);
        }
    }
}
```

### 2. Updated HedgeRelationshipDetails.razor.cs
```csharp
// BEFORE: Synchronous method with hardcoded defaults
private void NewMenuOnItemSelected(MenuEventArgs args)
{
    if (args.Item.Text == MODAL_OPTION_AMORTIZATION)
    {
        OptionAmortizationModel = new DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM
        {
            ID = 0,
            GLAccountID = 0,  // Hardcoded ❌
            ContraAccountID = 0,  // Hardcoded ❌
            // ... all defaults hardcoded
        };
    }
    OpenModal = args.Item.Text;
}

// AFTER: Async method with API call
private async void NewMenuOnItemSelected(MenuEventArgs args)
{
    if (args.Item.Text == MODAL_OPTION_AMORTIZATION)
    {
        await InitializeOptionAmortizationModelAsync();  // API call ✅
    }
    OpenModal = args.Item.Text;
}

private async Task InitializeOptionAmortizationModelAsync()
{
    // Call API to get defaults
    var defaultsResult = await Mediator.Send(new GetOptionAmortizationDefaults.Query(HedgeRelationship));
    
    if (!defaultsResult.HasError && defaultsResult.Data != null)
    {
        var defaults = defaultsResult.Data;
        
        OptionAmortizationModel = new DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM
        {
            ID = 0,
            GLAccountID = defaults.GlAccountId,  // From API ✅
            ContraAccountID = defaults.GlContraAcctId,  // From API ✅
            TotalAmount = defaults.TimeValue,  // From API ✅
            IntrinsicValue = defaults.IntrinsicValue,  // From API ✅
            IVGLAccountID = defaults.GlAccountId2,  // From API ✅
            IVContraAccountID = defaults.GlContraAcctId2,  // From API ✅
            IVAmortizationMethod = defaults.IVAmortizationMethod,  // From API ✅
            // ... other fields set from HedgeRelationship
        };
        
        // Set dates from hedge relationship
        if (HedgeRelationship?.HedgingItems?.Any() == true)
        {
            OptionAmortizationModel.StartDate = HedgeRelationship.DesignationDate;
            OptionAmortizationModel.EndDate = HedgeRelationship.HedgingItems.First().MaturityDate;
        }
    }
}
```

### 3. Updated OptionAmortizationDialog.razor
```razor
<!-- BEFORE: Unbound fields -->
<SfComboBox TValue="string" Placeholder="Intrinsic Value GL Account">
    <!-- No @bind-Value ❌ -->
</SfComboBox>

<!-- AFTER: Properly bound fields -->
<SfComboBox TValue="long"
            TItem="DerivativeEDGEHAEntityGLAccount"
            Placeholder="Intrinsic Value GL Account"
            @bind-Value="OptionAmortizationModel.IVGLAccountID"  <!-- Bound ✅ -->
            DataSource="AmortizationGLAccounts">
    <ComboBoxFieldSettings Value="Id" Text="AccountDescription" />
</SfComboBox>
```

## Visual Comparison

### Modal - BEFORE Fix
```
┌─────────────────────────────────────────┐
│  Option Amortization                    │
├─────────────────────────────────────────┤
│                                         │
│  GL Account:        [None ▼]            │  ← Empty
│  Contra:            [None ▼]            │  ← Empty
│  Amortization Method: [None ▼]          │  ← Empty
│  Total Amount:      [____]              │  ← 0
│                                         │
│  Intrinsic Value GL: [None ▼]           │  ← Empty
│  Intrinsic Value Contra: [None ▼]       │  ← Empty
│  IV Amort Method:   [None ▼]            │  ← Empty
│  Intrinsic Value:   [____]              │  ← 0
│                                         │
│  Start Date:        [____]              │  ← Empty
│  End Date:          [____]              │  ← Empty
│                                         │
│  [Cancel]  [Generate]                   │
└─────────────────────────────────────────┘
```

### Modal - AFTER Fix
```
┌─────────────────────────────────────────┐
│  Option Amortization                    │
├─────────────────────────────────────────┤
│                                         │
│  GL Account:        [12345-Hedge Adj ▼] │  ← Populated ✅
│  Contra:            [67890-Premium ▼]   │  ← Populated ✅
│  Amortization Method: [Straightline ▼]  │  ← Populated ✅
│  Total Amount:      [50,000.00]         │  ← Calculated ✅
│                                         │
│  Intrinsic Value GL: [12346-IV Adj ▼]   │  ← Populated ✅
│  Intrinsic Value Contra: [67891-IV ▼]   │  ← Populated ✅
│  IV Amort Method:   [Straightline ▼]    │  ← Populated ✅
│  Intrinsic Value:   [45,000.00]         │  ← Calculated ✅
│                                         │
│  Start Date:        [01/15/2025]        │  ← From Designation ✅
│  End Date:          [12/31/2027]        │  ← From Maturity ✅
│                                         │
│  [Cancel]  [Generate]                   │
└─────────────────────────────────────────┘
```

## Legacy Code Reference
This matches the behavior in `old/hr_hedgeRelationshipAddEditCtrl.js`:
- Line 3300: `$scope.openOptionTimeValueAmortDialog()` function
- Line 3301-3303: API call to GetOptionAmortizationDefaults
- Line 3313-3328: Initialize with API response values
- Line 3333-3334: Set dates from DesignationDate and MaturityDate

## Impact
- **User Experience**: Users now see pre-populated, calculated values instead of empty fields
- **Data Accuracy**: Values come from backend calculations based on hedge relationship setup
- **Consistency**: New system now matches legacy system behavior exactly
- **No Breaking Changes**: Edit functionality unaffected, existing data preserved
