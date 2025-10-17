# Workflow Items Comparison: Old vs New

## Quick Reference Table

| Hedge State | Hedge Type | Legacy JS Output | New C# Output (Before Fix) | New C# Output (After Fix) | Match? |
|-------------|------------|------------------|---------------------------|--------------------------|--------|
| Draft | Any | `["Designate"]` | `["Designate"]` | `["Designate"]` | ✅ |
| Designated | CashFlow | `["Redraft", "De-Designate", "Re-Designate"]` | `["Redraft", "De-Designate", "Re-Designate"]` | `["Redraft", "De-Designate", "Re-Designate"]` | ✅ |
| Designated | FairValue | `["Redraft", "De-Designate"]` | `["Redraft", "De-Designate"]` | `["Redraft", "De-Designate"]` | ✅ |
| Designated | NetInvestment | `["Redraft", "De-Designate"]` | `["Redraft", "De-Designate"]` | `["Redraft", "De-Designate"]` | ✅ |
| **Dedesignated** | **Any** | **`["Redraft", "De-Designate"]`** | **`["Redraft"]`** ❌ | **`["Redraft", "De-Designate"]`** | ✅ |

## Visual Workflow Comparison

### Scenario 1: Draft State
```
┌─────────────────────────────────┐
│ Workflow Actions (Draft)        │
├─────────────────────────────────┤
│ • Designate                     │
└─────────────────────────────────┘

Legacy JS: ✅ Same
New C#:    ✅ Same
```

### Scenario 2: Designated State (CashFlow)
```
┌─────────────────────────────────┐
│ Workflow Actions (Designated)   │
├─────────────────────────────────┤
│ • Redraft                       │
│ • De-Designate                  │
│ • Re-Designate                  │
└─────────────────────────────────┘

Legacy JS: ✅ Same
New C#:    ✅ Same
```

### Scenario 3: Designated State (FairValue or NetInvestment)
```
┌─────────────────────────────────┐
│ Workflow Actions (Designated)   │
├─────────────────────────────────┤
│ • Redraft                       │
│ • De-Designate                  │
└─────────────────────────────────┘

Legacy JS: ✅ Same
New C#:    ✅ Same
```

### Scenario 4: Dedesignated State (THE FIX)
```
BEFORE FIX:
┌─────────────────────────────────┐
│ Workflow Actions (Dedesignated) │
├─────────────────────────────────┤
│ • Redraft                       │
└─────────────────────────────────┘
❌ Missing "De-Designate" option!

AFTER FIX:
┌─────────────────────────────────┐
│ Workflow Actions (Dedesignated) │
├─────────────────────────────────┤
│ • Redraft                       │
│ • De-Designate                  │
└─────────────────────────────────┘
✅ Now matches legacy behavior!
```

## Code Logic Comparison

### Legacy JavaScript (old/hr_hedgeRelationshipAddEditCtrl.js)
```javascript
setWorkFlow = function () {
    // Start: ["Designate", "De-Designate", "Re-Designate"]
    
    if ($scope.Model.HedgeState === 'Draft') {
        $scope.DropDownList.ActionList.splice(1, 1);
        // Draft: ["Designate", "Re-Designate"]
    }

    if ($scope.Model.HedgeState !== 'Designated' || $scope.Model.HedgeType !== "CashFlow") {
        $scope.DropDownList.ActionList.splice(2, 1);
        // Removes Re-Designate for non-Designated or non-CashFlow
    }

    if ($scope.Model.HedgeState === 'Designated' || $scope.Model.HedgeState === "Dedesignated") {
        $scope.DropDownList.ActionList.splice(0, 1);
        $scope.DropDownList.ActionList.splice(0, 0, { "Value": "Redraft", "Disabled": false });
        // Replaces "Designate" with "Redraft"
        // For Dedesignated: ["Redraft", "De-Designate"]
    }

    $scope.DropDownList.ActionList.map(function (v) {
        v.Disabled = !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'));
    });
};
```

### New C# - BEFORE Fix
```csharp
private void BuildWorkflowItems()
{
    WorkflowItems.Clear();
    var state = HedgeRelationship?.HedgeState;
    var type = HedgeRelationship?.HedgeType;
    var hasWorkflowPermission = HasRequiredRole();

    if (state == DerivativeEDGEHAEntityEnumHedgeState.Draft)
    {
        WorkflowItems.Add(new DropDownMenuItem { Text = "Designate", Disabled = !hasWorkflowPermission });
    }
    else if (state == DerivativeEDGEHAEntityEnumHedgeState.Designated)
    {
        WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
        WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasWorkflowPermission });
        
        if (type == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
        {
            WorkflowItems.Add(new DropDownMenuItem { Text = "Re-Designate", Disabled = !hasWorkflowPermission });
        }
    }
    else if (state == DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
    {
        // ❌ BUG: Only adds Redraft
        WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
    }
}
```

### New C# - AFTER Fix
```csharp
private void BuildWorkflowItems()
{
    WorkflowItems.Clear();
    var state = HedgeRelationship?.HedgeState;
    var type = HedgeRelationship?.HedgeType;
    var hasWorkflowPermission = HasRequiredRole();

    if (state == DerivativeEDGEHAEntityEnumHedgeState.Draft)
    {
        WorkflowItems.Add(new DropDownMenuItem { Text = "Designate", Disabled = !hasWorkflowPermission });
    }
    else if (state == DerivativeEDGEHAEntityEnumHedgeState.Designated)
    {
        WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
        WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasWorkflowPermission });
        
        if (type == DerivativeEDGEHAEntityEnumHRHedgeType.CashFlow)
        {
            WorkflowItems.Add(new DropDownMenuItem { Text = "Re-Designate", Disabled = !hasWorkflowPermission });
        }
    }
    else if (state == DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
    {
        // ✅ FIXED: Now adds both Redraft and De-Designate
        WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
        WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasWorkflowPermission });
    }
}
```

## Role Permission Logic

Both old and new implementations correctly apply role-based permissions:

### JavaScript
```javascript
v.Disabled = !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'));
```

### C#
```csharp
var hasWorkflowPermission = HasRequiredRole();
// Where HasRequiredRole() = CheckUserRole("24") || CheckUserRole("17") || CheckUserRole("5")

Disabled = !hasWorkflowPermission
```

**Logic:**
- If user HAS role 24, 17, or 5 → `hasWorkflowPermission = true` → `Disabled = false` (enabled)
- If user does NOT have these roles → `hasWorkflowPermission = false` → `Disabled = true` (disabled)

## Verification Checklist

Use this checklist when manually testing the fix:

### For Dedesignated Hedge Relationship:
- [ ] Navigate to a hedge relationship with state = "Dedesignated"
- [ ] Open the workflow dropdown
- [ ] Verify "Redraft" option is present
- [ ] Verify "De-Designate" option is present ✅ (This is the fix!)
- [ ] With required roles (24, 17, or 5): Both options should be enabled
- [ ] Without required roles: Both options should be disabled but visible

### Regression Testing (Other States):
- [ ] Draft state: Only shows "Designate"
- [ ] Designated + CashFlow: Shows "Redraft", "De-Designate", "Re-Designate"
- [ ] Designated + FairValue: Shows "Redraft", "De-Designate" (no Re-Designate)
- [ ] Designated + NetInvestment: Shows "Redraft", "De-Designate" (no Re-Designate)

## Related Tickets
- **DE-2731:** Add ability to Re-Draft a hedge relationship in De-Designated status
- **DE-3928:** Show Re-Designate on designated relationship only

## Conclusion

The fix ensures that the new Blazor implementation provides the exact same workflow options as the legacy AngularJS implementation, specifically addressing the missing "De-Designate" option for Dedesignated hedge relationships.
