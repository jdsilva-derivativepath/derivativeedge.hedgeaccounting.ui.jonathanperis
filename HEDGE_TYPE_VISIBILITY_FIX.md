# Hedge Type Field Visibility Fix Documentation

## Issue Summary
The Benchmark/Contractual Rate field and various checkboxes were not displaying correctly based on the selected Hedge Type (CashFlow, FairValue, NetInvestment).

## Root Cause
The Benchmark/Contractual Rate field was displayed unconditionally (always visible), when it should only be visible when `HedgeRiskType == InterestRate`. Additionally, the Acquisition checkbox had an incorrect `Visible` attribute that prevented it from always showing when HedgeType is CashFlow.

## Changes Made

### 1. HedgeRelationshipInfoSection.razor (Lines 294-310)
**Fixed Benchmark/Contractual Rate Field Visibility:**
- Added conditional rendering: `@if (HedgeRelationship.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.InterestRate)`
- Field now only displays when HedgeRiskType is InterestRate (matching legacy behavior)
- Label dynamically changes via `@BenchMarkLabel` property:
  - "Contractual Rate" when HedgeType = CashFlow
  - "Benchmark" when HedgeType = FairValue or NetInvestment

### 2. HedgeRelationshipInfoSection.razor (Lines 444-452)
**Fixed Acquisition Checkbox:**
- Removed incorrect `Visible="HedgeRelationship.Acquisition"` attribute
- Checkbox now always visible when HedgeType = CashFlow
- Fixed typo: "Aquisition" → "Acquisition"

## Expected Behavior by Hedge Type

### HedgeType = NetInvestment (None)
| Field/Checkbox | Visibility | Condition |
|---|---|---|
| Benchmark field | Conditional | Only if HedgeRiskType = InterestRate |
| Label | "Benchmark" | Always |
| Pre-Issuance Hedge | Hidden | N/A |
| Acquisition | Hidden | N/A |
| Portfolio Layer Method | Hidden | N/A |
| Shortcut | Hidden | N/A |

### HedgeType = CashFlow
| Field/Checkbox | Visibility | Condition |
|---|---|---|
| Contractual Rate field | Conditional | Only if HedgeRiskType = InterestRate |
| Label | "Contractual Rate" | When HedgeRiskType = InterestRate |
| Pre-Issuance Hedge | Visible | Always (when HedgeType = CashFlow) |
| Acquisition | Visible | Always (when HedgeType = CashFlow) |
| Portfolio Layer Method | Hidden | N/A |
| Shortcut | Hidden | N/A |

### HedgeType = FairValue
| Field/Checkbox | Visibility | Condition |
|---|---|---|
| Benchmark field | Conditional | Only if HedgeRiskType = InterestRate |
| Label | "Benchmark" | Always |
| Pre-Issuance Hedge | Hidden | N/A |
| Acquisition | Hidden | N/A |
| Portfolio Layer Method | Visible | Always (when HedgeType = FairValue) |
| Shortcut | Visible | Always (when HedgeType = FairValue) |

## Legacy References

### Source Files
- `old/initialView.cshtml` line 67: Benchmark field visibility condition
  ```html
  <div class="row form-group" data-ng-show="Model.HedgeRiskType === 'InterestRate'">
  ```

- `old/detailsView.cshtml` lines 235-283: Checkbox visibility conditions
  - PreIssuanceHedge: `data-ng-show="Model.HedgeType === 'CashFlow'"`
  - PortfolioLayerMethod: `data-ng-show="Model.HedgeType === 'FairValue'"`
  - Acquisition: `data-ng-show="Model.HedgeType === 'CashFlow'"`

- `old/initialView.cshtml` line 102: Shortcut checkbox
  ```html
  <div class="row form-group" data-ng-show="Model.HedgeType === 'FairValue'">
  ```

- `old/hr_hedgeRelationshipAddEditCtrl.js` lines 254-268: `setBenchmarkLabel()` function
  ```javascript
  function setBenchmarkLabel() {
      if (($scope.Model.HedgeRiskType === 'InterestRate')
          && ($scope.Model.HedgeType === 'CashFlow')) {
          $scope.Model.BenchMarkLabel = 'Contractual Rate';
      }
      else if (($scope.Model.HedgeRiskType === 'InterestRate')
          && ($scope.Model.HedgeType === 'FairValue')) {
          $scope.Model.BenchMarkLabel = 'Benchmark';
      }
      else {
          $scope.Model.BenchMarkLabel = 'Benchmark';
      }
  }
  ```

## Testing Scenarios

### Scenario 1: HedgeRiskType = InterestRate, HedgeType = CashFlow
- ✅ "Contractual Rate" field should be visible
- ✅ Pre-Issuance Hedge checkbox should be visible
- ✅ Acquisition checkbox should be visible
- ✅ Portfolio Layer Method checkbox should be hidden
- ✅ Shortcut checkbox should be hidden

### Scenario 2: HedgeRiskType = InterestRate, HedgeType = FairValue
- ✅ "Benchmark" field should be visible
- ✅ Pre-Issuance Hedge checkbox should be hidden
- ✅ Acquisition checkbox should be hidden
- ✅ Portfolio Layer Method checkbox should be visible
- ✅ Shortcut checkbox should be visible

### Scenario 3: HedgeRiskType = InterestRate, HedgeType = NetInvestment
- ✅ "Benchmark" field should be visible
- ✅ Pre-Issuance Hedge checkbox should be hidden
- ✅ Acquisition checkbox should be hidden
- ✅ Portfolio Layer Method checkbox should be hidden
- ✅ Shortcut checkbox should be hidden

### Scenario 4: HedgeRiskType = ForeignExchange (any HedgeType)
- ✅ Benchmark/Contractual Rate field should be hidden
- ✅ Checkboxes should follow same rules as above based on HedgeType

### Scenario 5: HedgeRiskType = Commodity (any HedgeType)
- ✅ Benchmark/Contractual Rate field should be hidden
- ✅ Checkboxes should follow same rules as above based on HedgeType

## Related Components

### HedgeRelationshipLabelHelper.cs
Provides the `GetBenchMarkLabel()` method that returns the correct label based on HedgeRiskType and HedgeType:
```csharp
public static string GetBenchMarkLabel(DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship)
{
    if (HedgeRelationship?.HedgeRiskType == DerivativeEDGEHAEntityEnumHedgeRiskType.InterestRate 
        && HedgeRelationship?.HedgeType == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
    {
        return "Contractual Rate";
    }
    return "Benchmark";
}
```

### HedgeRelationshipDetails.razor.cs
Contains the edit permission methods:
- `CanEditCheckbox()`: Controls general checkbox editing
- `CanEditPreIssuanceHedge()`: Controls Pre-Issuance Hedge checkbox
- `CanEditPortfolioLayerMethod()`: Controls Portfolio Layer Method checkbox

## Known Limitations

### Benchmark Dropdown Filtering (Not Implemented)
The legacy system filters the Benchmark dropdown options based on HedgeType:
- CashFlow excludes: FFUTFDTR, FHLBTopeka, USDTBILL4WH15
- FairValue/NetInvestment exclude: FFUTFDTR, FHLBTopeka, USDTBILL4WH15, Other, Prime

This filtering exists in `HedgeRelationshipLabelHelper.FilterBenchmarkList()` but is not currently applied to the dropdown. This is a separate enhancement and was not part of the visibility fix.

## Verification Checklist
- [x] Benchmark field shows only when HedgeRiskType = InterestRate
- [x] Label changes correctly between "Benchmark" and "Contractual Rate"
- [x] Pre-Issuance Hedge shows only for CashFlow
- [x] Acquisition shows only for CashFlow (fixed typo and visibility)
- [x] Portfolio Layer Method shows only for FairValue
- [x] Shortcut shows only for FairValue
- [x] All changes match legacy system behavior
- [x] Code includes comments referencing legacy files
