# Legacy vs New Implementation Comparison

## ExcludeIntrinsicValue Checkbox Behavior

### Legacy Code (AngularJS)
**File**: `old/hr_hedgeRelationshipAddEditCtrl.js`

#### Rule 1: Can only be enabled when IsAnOptionHedge is true
```javascript
// Line 2589-2592
if (type === "ExcludeIntrinsicValue") {
    if (!$scope.Model.IsAnOptionHedge) {
        value = false;  // Prevent checking if not an option hedge
    }
}
```

#### Rule 2: Only editable in Draft state or by role 24
```javascript
// Line 2585
if ($scope.Model.HedgeState === "Draft" || $scope.checkUserRole("24")) {
    // Allow changes
}
```

#### Rule 3: When IsAnOptionHedge is unchecked, clear ExcludeIntrinsicValue
```javascript
// Lines 424-435
$scope.$watch('Model.IsAnOptionHedge', function (new_) {
    if (new_ !== undefined) {
        if (!new_) {
            $scope.Model.AmortizeOptionPremimum = false;
            $scope.Model.IsDeltaMatchOption = false;
            $scope.Model.ExcludeIntrinsicValue = false;  // Cleared here
        }
    }
});
```

#### Rule 4: When ExcludeIntrinsicValue is unchecked, clear dependent fields
```javascript
// Lines 437-445
$scope.$watch('Model.ExcludeIntrinsicValue', function (new_) {
    if (new_ !== undefined) {
        if (!new_) {
            $scope.Model.IntrinsicMethod = "None";
            $scope.Model.AmortizeOptionPremimum = false;
            $scope.Model.IsDeltaMatchOption = false;
        }
    }
});
```

---

### New Code (Blazor)
**Files**: 
- `HedgeRelationshipDetails.razor`
- `HedgeRelationshipDetails.razor.cs`

#### Rule 1: Can only be enabled when IsAnOptionHedge is true
```razor
<!-- HedgeRelationshipDetails.razor, Line 392 -->
<SfCheckBox CssClass="input-checkbox"
    Disabled="@(!CanEditCheckbox() || !HedgeRelationship.IsAnOptionHedge)"
    @bind-Checked="HedgeRelationship.ExcludeIntrinsicValue"
    Label="Exclude Intrinsic Value">
```

```csharp
// HedgeRelationshipDetails.razor.cs
private void OnExcludeIntrinsicValueChanged(ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        // Legacy rule: ExcludeIntrinsicValue can only be true when IsAnOptionHedge is true
        if (args.Checked && !HedgeRelationship.IsAnOptionHedge)
        {
            // Prevent checking the box if not an option hedge
            HedgeRelationship.ExcludeIntrinsicValue = false;
        }
        else
        {
            HedgeRelationship.ExcludeIntrinsicValue = args.Checked;
        }
        // ... rest of handler
    }
}
```

#### Rule 2: Only editable in Draft state or by role 24
```csharp
// HedgeRelationshipDetails.razor.cs, Line 769
private bool CanEditCheckbox() =>
    HedgeRelationship?.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft || CheckUserRole("24");
```

#### Rule 3: When IsAnOptionHedge is unchecked, clear ExcludeIntrinsicValue
```csharp
// HedgeRelationshipDetails.razor.cs
private void OnIsAnOptionHedgeChanged(ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        HedgeRelationship.IsAnOptionHedge = args.Checked;
        
        // Legacy rule: When IsAnOptionHedge is unchecked, also clear related option hedge fields
        if (!args.Checked)
        {
            HedgeRelationship.AmortizeOptionPremimum = false;
            HedgeRelationship.IsDeltaMatchOption = false;
            HedgeRelationship.ExcludeIntrinsicValue = false;  // Cleared here
        }
        
        StateHasChanged();
    }
}
```

#### Rule 4: When ExcludeIntrinsicValue is unchecked, clear dependent fields
```csharp
// HedgeRelationshipDetails.razor.cs
private void OnExcludeIntrinsicValueChanged(ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        // ... validation logic ...
        
        if (!HedgeRelationship.ExcludeIntrinsicValue)
        {
            // When unchecked, reset IntrinsicMethod to None and related options
            HedgeRelationship.IntrinsicMethod = DerivativeEDGEHAEntityEnumIntrinsicMethod.None;
            HedgeRelationship.AmortizeOptionPremimum = false;
            HedgeRelationship.IsDeltaMatchOption = false;
        }
        
        StateHasChanged();
    }
}
```

---

## IsAnOptionHedge and OffMarket Mutual Exclusivity

### Legacy Code
```javascript
// Lines 2594-2599
if (type === "IsAnOptionHedge" && $scope.Model.OffMarket) {
    value = false;  // Can't check IsAnOptionHedge if OffMarket is true
}
if (type === "OffMarket" && $scope.Model.IsAnOptionHedge) {
    value = false;  // Can't check OffMarket if IsAnOptionHedge is true
}
```

### New Code
```razor
<!-- OffMarket checkbox is disabled when IsAnOptionHedge is checked -->
<SfCheckBox CssClass="input-checkbox"
    Disabled="@(!CanEditCheckbox() || HedgeRelationship.IsAnOptionHedge)"
    @bind-Checked="HedgeRelationship.OffMarket"
    Label="Off-Market">
```

```csharp
private void OnIsAnOptionHedgeChanged(ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        // Legacy rule: IsAnOptionHedge and OffMarket are mutually exclusive
        if (args.Checked && HedgeRelationship.OffMarket)
        {
            // Prevent checking the box if OffMarket is true
            HedgeRelationship.IsAnOptionHedge = false;
            return;
        }
        // ... rest of logic ...
    }
}

private void OnOffMarketChanged(ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        // Legacy rule: IsAnOptionHedge and OffMarket are mutually exclusive
        if (args.Checked && HedgeRelationship.IsAnOptionHedge)
        {
            // Prevent checking the box if IsAnOptionHedge is true
            HedgeRelationship.OffMarket = false;
            return;
        }
        
        HedgeRelationship.OffMarket = args.Checked;
        StateHasChanged();
    }
}
```

---

## Truth Table: ExcludeIntrinsicValue Enable/Disable Logic

| HedgeState | UserRole24 | IsAnOptionHedge | ExcludeIntrinsicValue Enabled? |
|------------|------------|-----------------|--------------------------------|
| Draft      | No         | true            | ✅ Yes                         |
| Draft      | No         | false           | ❌ No                          |
| Draft      | Yes        | true            | ✅ Yes                         |
| Draft      | Yes        | false           | ❌ No                          |
| Designated | No         | true            | ❌ No                          |
| Designated | No         | false           | ❌ No                          |
| Designated | Yes        | true            | ✅ Yes                         |
| Designated | Yes        | false           | ❌ No                          |

**Key Insight**: `ExcludeIntrinsicValue` requires **BOTH** conditions to be true:
1. `(HedgeState == Draft OR UserRole24)`
2. **AND** `IsAnOptionHedge == true`

This is now correctly enforced in the new implementation with:
```razor
Disabled="@(!CanEditCheckbox() || !HedgeRelationship.IsAnOptionHedge)"
```

Where:
- `!CanEditCheckbox()` = NOT (Draft OR Role24)
- `!HedgeRelationship.IsAnOptionHedge` = NOT IsAnOptionHedge

So the checkbox is disabled when EITHER condition is false, which matches the legacy behavior exactly.
