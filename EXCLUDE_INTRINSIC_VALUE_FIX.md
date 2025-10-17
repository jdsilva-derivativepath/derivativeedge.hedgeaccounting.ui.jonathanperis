# Fix: Exclude Intrinsic Value Checkbox Behavior

## Issue
The "Exclude Intrinsic Value" checkbox on the HedgeRelationshipDetails page was not correctly enforcing the legacy system's rules regarding when it should be enabled or disabled.

## Legacy System Rules (from `old/hr_hedgeRelationshipAddEditCtrl.js`)

### 1. ExcludeIntrinsicValue Checkbox Behavior
- **Enabled only when**: `HedgeState === "Draft" OR user has role "24"` **AND** `IsAnOptionHedge === true`
- **When clicked**: If `IsAnOptionHedge` is false, the checkbox cannot be checked (lines 2589-2592)
- **When unchecked**: Resets `IntrinsicMethod` to "None" and clears `AmortizeOptionPremimum` and `IsDeltaMatchOption` (lines 439-443)

### 2. IsAnOptionHedge Checkbox Behavior
- **When unchecked**: Clears `AmortizeOptionPremimum`, `IsDeltaMatchOption`, and `ExcludeIntrinsicValue` (lines 426-429)
- **Mutually exclusive with OffMarket**: Cannot be checked if `OffMarket` is true (lines 2594-2596)

### 3. OffMarket Checkbox Behavior
- **Mutually exclusive with IsAnOptionHedge**: Cannot be checked if `IsAnOptionHedge` is true (lines 2597-2599)

## Changes Made

### 1. HedgeRelationshipDetails.razor
#### Changed: ExcludeIntrinsicValue Checkbox
```diff
- Disabled="@(!CanEditCheckbox())"
+ Disabled="@(!CanEditCheckbox() || !HedgeRelationship.IsAnOptionHedge)"
```
**Reason**: Adds the requirement that `IsAnOptionHedge` must be true for the checkbox to be enabled.

#### Changed: OffMarket Checkbox
```diff
- Disabled="@(!CanEditCheckbox())"
+ Disabled="@(!CanEditCheckbox() || HedgeRelationship.IsAnOptionHedge)"
+ <CheckBoxEvents TChecked="bool" ValueChange="OnOffMarketChanged" />
```
**Reason**: Enforces mutual exclusivity with `IsAnOptionHedge` and adds event handler.

#### Changed: IsAnOptionHedge Checkbox
```diff
+ <CheckBoxEvents TChecked="bool" ValueChange="OnIsAnOptionHedgeChanged" />
```
**Reason**: Adds event handler to enforce business rules when checkbox value changes.

### 2. HedgeRelationshipDetails.razor.cs
#### Added: OnIsAnOptionHedgeChanged Handler
- Enforces mutual exclusivity with `OffMarket`
- When unchecked, clears `AmortizeOptionPremimum`, `IsDeltaMatchOption`, and `ExcludeIntrinsicValue`

#### Added: OnOffMarketChanged Handler  
- Enforces mutual exclusivity with `IsAnOptionHedge`
- Prevents checking when `IsAnOptionHedge` is true

#### Modified: OnExcludeIntrinsicValueChanged Handler
- Added check to prevent enabling when `IsAnOptionHedge` is false
- Maintains existing logic for clearing related fields when unchecked

## Testing Scenarios

### Scenario 1: Draft State with IsAnOptionHedge = false
- **Expected**: ExcludeIntrinsicValue checkbox is disabled
- **Result**: ✓ Checkbox is disabled due to `!HedgeRelationship.IsAnOptionHedge` condition

### Scenario 2: Draft State with IsAnOptionHedge = true
- **Expected**: ExcludeIntrinsicValue checkbox is enabled
- **Result**: ✓ Checkbox is enabled

### Scenario 3: Checking IsAnOptionHedge when OffMarket is checked
- **Expected**: IsAnOptionHedge cannot be checked
- **Result**: ✓ OnIsAnOptionHedgeChanged handler prevents the change

### Scenario 4: Checking OffMarket when IsAnOptionHedge is checked
- **Expected**: OffMarket cannot be checked
- **Result**: ✓ OnOffMarketChanged handler prevents the change

### Scenario 5: Unchecking IsAnOptionHedge
- **Expected**: ExcludeIntrinsicValue, AmortizeOptionPremimum, and IsDeltaMatchOption are all cleared
- **Result**: ✓ OnIsAnOptionHedgeChanged handler clears all three fields

### Scenario 6: Non-Draft State without Role 24
- **Expected**: ExcludeIntrinsicValue checkbox is disabled (regardless of IsAnOptionHedge)
- **Result**: ✓ CanEditCheckbox() returns false, disabling the checkbox

## Alignment with Legacy Code
All changes are direct implementations of the rules found in the legacy AngularJS code:
- Lines 424-435: IsAnOptionHedge watcher behavior
- Lines 437-445: ExcludeIntrinsicValue watcher behavior  
- Lines 2585-2603: Checkbox click event handler logic

## Files Modified
1. `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor`
2. `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`

## No Breaking Changes
- These changes only restrict functionality that was previously incorrectly allowed
- All existing valid workflows remain unchanged
- No database schema changes required
- No API changes required
