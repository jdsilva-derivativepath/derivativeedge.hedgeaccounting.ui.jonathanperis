# Assessment Method Filtering Fix Documentation

## Problem Statement
On the HedgeRelationshipDetails page, in the Instruments and Analysis tab, the Prospective Assessment Method and Retrospective Assessment Method dropdowns were loading all available effectiveness methods from the API without applying the filtering logic that existed in the legacy AngularJS system.

## Root Cause
The new Blazor implementation had a hardcoded list of assessment methods in the `GetAssessmentMethodOptions()` method that returned a static list of 9 methods, ignoring:
1. The actual effectiveness methods available from the API
2. The hedge relationship's HedgeType (FairValue, CashFlow, NetInvestment)
3. Whether the hedge is an option hedge (IsAnOptionHedge property)

## Legacy Filtering Logic
The legacy system (old/hr_hedgeRelationshipAddEditCtrl.js lines 149-180) applied the following filtering rules:

### For Non-Option Hedges (IsAnOptionHedge = false):
1. If HedgeType is **FairValue**: Include only methods where `IsForFairValue = true`
2. If HedgeType is **CashFlow** or **NetInvestment**: Include all methods
3. **Always exclude** "Regression - Change in Intrinsic Value" for non-option hedges

### For Option Hedges (IsAnOptionHedge = true):
- Include **ONLY** methods that contain "Regression - Change in" in the name
- This includes both "Regression - Change in Intrinsic Value" and "Regression - Change in Fair Value"

## Solution Implemented

### 1. Created GetEffectivenessMethodList Handler
**File:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Queries/GetEffectivenessMethodList.cs`

This MediatR query handler fetches all effectiveness methods from the Hedge Accounting API:
```csharp
public sealed class GetEffectivenessMethodList
{
    public sealed record Query : IRequest<Response>;
    public sealed record Response(List<DerivativeEDGEHAEntityEffectivenessMethod> EffectivenessMethods);
    
    // Handler calls: hedgeAccountingApiClient.EffectivenessMethodAllAsync()
}
```

### 2. Updated InstrumentAnalysisTab.razor.cs
**File:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipTabs/InstrumentAnalysisTab.razor.cs`

#### Added Properties:
```csharp
private List<DerivativeEDGEHAEntityEffectivenessMethod> AllEffectivenessMethods { get; set; } = [];
```

#### Added Initialization:
```csharp
protected override async Task OnInitializedAsync()
{
    await LoadCurrency();
    await LoadEffectivenessMethods(); // NEW: Load effectiveness methods from API
}

private async Task LoadEffectivenessMethods()
{
    var response = await Mediator.Send(new GetEffectivenessMethodList.Query());
    AllEffectivenessMethods = response.EffectivenessMethods;
}
```

#### Replaced GetAssessmentMethodOptions():
Changed from a static hardcoded list to a dynamic filtered list:

```csharp
private IEnumerable<DropdownModel> GetAssessmentMethodOptions()
{
    var filteredMethods = new List<DropdownModel>();
    filteredMethods.Add(new DropdownModel { ID = 0, Text = "None" });
    
    if (HedgeRelationship == null || AllEffectivenessMethods == null || AllEffectivenessMethods.Count == 0)
        return filteredMethods;

    // Set FairValueMethod to 'None' if HedgeType is not FairValue (matches legacy)
    if (HedgeRelationship.HedgeType != DerivativeEDGEHAEntityEnumHedgeType.FairValue)
        HedgeRelationship.FairValueMethod = "None";

    foreach (var method in AllEffectivenessMethods)
    {
        bool shouldInclude = false;
        
        if (!HedgeRelationship.IsAnOptionHedge)
        {
            // Non-option hedge: Include based on HedgeType and IsForFairValue
            if ((HedgeRelationship.HedgeType == DerivativeEDGEHAEntityEnumHedgeType.FairValue && method.IsForFairValue) ||
                HedgeRelationship.HedgeType != DerivativeEDGEHAEntityEnumHedgeType.FairValue)
            {
                // Exclude "Regression - Change in Intrinsic Value" for non-option hedges
                if (!method.Name.Contains("Regression - Change in Intrinsic Value"))
                    shouldInclude = true;
            }
        }
        else
        {
            // Option hedge: Only include "Regression - Change in" methods
            if (method.Name.Contains("Regression - Change in"))
                shouldInclude = true;
        }

        if (shouldInclude)
        {
            filteredMethods.Add(new DropdownModel
            {
                ID = (int)method.ID,
                Text = method.Name
            });
        }
    }

    return filteredMethods;
}
```

#### Enhanced RefreshGridData():
```csharp
public async Task RefreshGridData()
{
    await LoadInstrumentAnalysisData();
    await InvokeAsync(StateHasChanged); // Ensure dropdowns refresh with new filtering
}
```

## API Model Properties Used

### DerivativeEDGEHAEntityEffectivenessMethod
- `ID` (long): Unique identifier
- `Name` (string): Display name (e.g., "Regression - Change in Fair Value")
- `IsForFairValue` (bool): Whether this method applies to FairValue hedge types
- `IsForCashFlow` (bool): Whether this method applies to CashFlow hedge types
- `IsForProspective` (bool): Whether this method can be used for prospective assessment
- `IsForRetrospective` (bool): Whether this method can be used for retrospective assessment

### HedgeRelationship Properties
- `HedgeType` (enum): FairValue, CashFlow, or NetInvestment
- `IsAnOptionHedge` (bool): Whether this hedge involves options
- `ProspectiveEffectivenessMethodID` (long?): Selected prospective method
- `RetrospectiveEffectivenessMethodID` (long?): Selected retrospective method

## Testing Scenarios

### Scenario 1: Non-Option FairValue Hedge
**Given:** HedgeType = FairValue, IsAnOptionHedge = false
**Expected:** Only methods where IsForFairValue = true, excluding "Intrinsic Value"

### Scenario 2: Non-Option CashFlow Hedge
**Given:** HedgeType = CashFlow, IsAnOptionHedge = false
**Expected:** All methods, excluding "Regression - Change in Intrinsic Value"

### Scenario 3: Option Hedge (any type)
**Given:** IsAnOptionHedge = true
**Expected:** Only "Regression - Change in Fair Value" and "Regression - Change in Intrinsic Value"

## Migration Notes
This implementation exactly matches the legacy filtering logic from:
- **Legacy File:** `old/hr_hedgeRelationshipAddEditCtrl.js`
- **Legacy Function:** `setDropDownListEffectivenessMethods()` (lines 149-180)
- **Legacy Trigger:** Called on initialization and when HedgeType changes

## Files Changed
1. **NEW:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Queries/GetEffectivenessMethodList.cs`
2. **MODIFIED:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipTabs/InstrumentAnalysisTab.razor.cs`

## Impact
- Both Prospective and Retrospective Assessment Method dropdowns now show the correct filtered options
- The filtering dynamically updates based on HedgeType and IsAnOptionHedge values
- The dropdown options refresh when the tab becomes active or when effectiveness settings change
- No UI changes to the Razor template were required - the existing bindings work correctly

## Verification
To verify this fix works correctly:
1. Create a hedge relationship with HedgeType = FairValue and IsAnOptionHedge = false
   - Check that only FairValue-compatible methods appear (excluding Intrinsic Value)
2. Change HedgeType to CashFlow
   - Check that all methods appear (excluding Intrinsic Value)
3. Set IsAnOptionHedge = true
   - Check that only "Regression - Change in..." methods appear
