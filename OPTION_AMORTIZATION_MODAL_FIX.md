# Option Amortization Modal Data Loading Fix

## Problem Statement
The OptionAmortization modal in the new Blazor system was opening with empty/default values, while the legacy AngularJS system loaded pre-calculated default values from the API.

## Root Cause
The new system was not calling the `GetOptionAmortizationDefaults` API endpoint when opening the Option Amortization modal. This API endpoint returns important calculated values:
- GL Account IDs for Option Time Value and Intrinsic Value
- Contra Account IDs for both value types
- Calculated Intrinsic Value from hedge items
- Calculated Time Value (option premium)
- Default Intrinsic Value Amortization Method

## Solution Implemented

### 1. Created MediatR Query Handler
**File:** `Features/HedgeRelationships/Handlers/Queries/GetOptionAmortizationDefaults.cs`

- Accepts `HedgeRelationshipVM` as input
- Uses AutoMapper to convert VM to Entity for API call
- Calls `IHedgeAccountingApiClient.GetOptionAmortizationDefaultsAsync()`
- Returns `OptionAmortizationDefaultValues` object with:
  - `GlAccountId` - GL Account for Option Time Value
  - `GlContraAcctId` - Contra Account for Option Time Value  
  - `GlAccountId2` - GL Account for Intrinsic Value
  - `GlContraAcctId2` - Contra Account for Intrinsic Value
  - `IntrinsicValue` - Calculated intrinsic value
  - `IVAmortizationMethod` - Default amortization method for IV
  - `TimeValue` - Calculated option time value/premium

### 2. Updated HedgeRelationshipDetails Component
**File:** `Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`

#### Changed Method: `NewMenuOnItemSelected`
- Changed from `void` to `async void` to support API call
- Now calls `InitializeOptionAmortizationModelAsync()` when opening Option Amortization modal

#### New Method: `InitializeOptionAmortizationModelAsync`
This method:
1. Calls the GetOptionAmortizationDefaults API via MediatR
2. Maps API response to OptionAmortizationModel properties:
   - `GLAccountID` = `defaults.GlAccountId`
   - `ContraAccountID` = `defaults.GlContraAcctId`
   - `IVGLAccountID` = `defaults.GlAccountId2`
   - `IVContraAccountID` = `defaults.GlContraAcctId2`
   - `IntrinsicValue` = `defaults.IntrinsicValue`
   - `IVAmortizationMethod` = `defaults.IVAmortizationMethod`
   - `TotalAmount` = `defaults.TimeValue`
3. Sets `AmortizationMethod` from current HedgeRelationship
4. Sets start/end dates from `DesignationDate` and first `HedgingItem.MaturityDate`
5. Includes error handling with fallback to basic defaults

### 3. Updated OptionAmortizationDialog Component
**File:** `Features/HedgeRelationships/Components/OptionAmortizationDialog.razor`

Updated Intrinsic Value fields to properly bind to model properties:
- **Intrinsic Value GL Account**: Bound to `OptionAmortizationModel.IVGLAccountID`
- **Intrinsic Value Contra**: Bound to `OptionAmortizationModel.IVContraAccountID`  
- **Intrinsic Value Amortization Method**: Bound to `OptionAmortizationModel.IVAmortizationMethod`
- **Intrinsic Value**: Bound to `OptionAmortizationModel.IntrinsicValue`

All dropdowns now use the same datasources as the Time Value fields (AmortizationGLAccounts, AmortizationContraAccounts, AmortizationMethodOptions).

## Legacy Code Reference
**File:** `old/hr_hedgeRelationshipAddEditCtrl.js`

- Line 3300-3350: `$scope.openOptionTimeValueAmortDialog()` function
- Line 3300: API call to `HedgeRelationship/GetOptionAmortizationDefaults`
- Line 3313-3328: Initialization of HedgeRelationshipOptionTimeValueAmort with API response values
- Line 3333-3334: Setting start/end dates from DesignationDate and MaturityDate

## Expected Behavior

### When Opening NEW Option Amortization Modal:
1. User clicks "Option Amortization" from New menu
2. System calls GetOptionAmortizationDefaults API
3. Modal opens with pre-populated values:
   - GL Account dropdown shows the default GL account from API
   - Contra Account dropdown shows the default contra account from API
   - Total Amount field shows calculated time value
   - Intrinsic Value GL Account shows default from API (if IsAnOptionHedge)
   - Intrinsic Value Contra shows default from API (if IsAnOptionHedge)
   - Intrinsic Value Amortization Method shows default from API (if IsAnOptionHedge)
   - Intrinsic Value shows calculated value from API (if IsAnOptionHedge)
   - Start Date shows hedge relationship designation date
   - End Date shows first hedging item maturity date
   - Amortization Method shows current hedge relationship's amortization method
   - AmortizeOptionPremium checkbox is checked

### When EDITING Existing Option Amortization:
1. User selects "Edit" action on grid row
2. System loads existing option amortization data (no API call)
3. Modal opens with existing values from selected row
4. AmortizeOptionPremium is set based on IsAnOptionHedge flag

## Testing Checklist

Due to build limitations (requires AWS packages), manual testing is required:

### Manual Testing Steps:
1. **Prerequisites:**
   - Have a hedge relationship with HedgingItems configured
   - Ensure the hedge relationship has an AmortizationMethod set
   - Ensure the hedge relationship has a DesignationDate set
   
2. **Test New Option Amortization Modal:**
   - Navigate to HedgeRelationshipDetails page
   - Click "New" → "Option Amortization"
   - Verify modal opens with populated values (not empty/zeros)
   - Verify GL Account dropdown has a value selected
   - Verify Contra Account dropdown has a value selected
   - Verify Total Amount field has a calculated value
   - Verify Start Date matches hedge relationship designation date
   - Verify End Date matches first hedging item maturity date
   - If IsAnOptionHedge is true:
     - Verify Intrinsic Value fields are visible
     - Verify Intrinsic Value GL Account has a value
     - Verify Intrinsic Value Contra has a value
     - Verify Intrinsic Value Amortization Method has a value
     - Verify Intrinsic Value field has a calculated value
     
3. **Test Edit Option Amortization Modal:**
   - Navigate to OptionAmortizationTab
   - Select an existing option amortization row
   - Click "Edit" action
   - Verify modal opens with values from selected row
   - Verify all fields match the row data

4. **Compare with Legacy System:**
   - Perform same actions in legacy AngularJS system
   - Compare field values between new and legacy systems
   - Verify values match exactly

## Files Changed
1. `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Queries/GetOptionAmortizationDefaults.cs` (NEW)
2. `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs` (MODIFIED)
3. `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Components/OptionAmortizationDialog.razor` (MODIFIED)

## No Breaking Changes
- Edit functionality continues to work as before (loads from selected row)
- All existing option amortization data remains intact
- Only affects NEW option amortization modal opening behavior
- Falls back to basic defaults if API call fails

## Migration Notes
This fix is a **lift-and-shift migration** of legacy behavior:
- No new business logic added
- No existing logic removed or changed
- Exact behavior match with legacy system
- Preserves all data structures and workflows
