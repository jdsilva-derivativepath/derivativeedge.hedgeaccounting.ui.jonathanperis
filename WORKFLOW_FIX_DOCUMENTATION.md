# Workflow Items Fix Documentation

## Issue Description
The `BuildWorkflowItems()` method in `HedgeRelationshipDetails.razor.cs` was not correctly matching the legacy JavaScript `setWorkFlow()` function behavior, specifically for hedge relationships in the **Dedesignated** state.

## Root Cause
When a hedge relationship was in the `Dedesignated` state:
- **Legacy JavaScript behavior:** Showed workflow items `["Redraft", "De-Designate"]`
- **New C# behavior (before fix):** Only showed `["Redraft"]`

This discrepancy meant users lost access to the "De-Designate" workflow option for Dedesignated relationships in the new system.

## Solution
Modified the `BuildWorkflowItems()` method to add both "Redraft" and "De-Designate" options for Dedesignated state, matching the legacy JavaScript logic.

### Code Changes
**File:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`

**Before (lines 434-438):**
```csharp
else if (state == DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
{
    // Dedesignated state: Show only Redraft (DE-2731)
    WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
}
```

**After (lines 434-440):**
```csharp
else if (state == DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)
{
    // Dedesignated state: Show Redraft and De-Designate (DE-2731)
    // Old JS logic: removes "Re-Designate", then replaces "Designate" with "Redraft", leaving ["Redraft", "De-Designate"]
    WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasWorkflowPermission });
    WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasWorkflowPermission });
}
```

## Expected Behavior After Fix

### Workflow Items by Hedge State

| Hedge State | Hedge Type | Workflow Items Displayed | Has Required Role | Items Enabled |
|-------------|------------|-------------------------|-------------------|---------------|
| Draft | Any | ["Designate"] | Yes | Yes |
| Draft | Any | ["Designate"] | No | No |
| Designated | CashFlow | ["Redraft", "De-Designate", "Re-Designate"] | Yes | Yes |
| Designated | CashFlow | ["Redraft", "De-Designate", "Re-Designate"] | No | No |
| Designated | FairValue | ["Redraft", "De-Designate"] | Yes | Yes |
| Designated | FairValue | ["Redraft", "De-Designate"] | No | No |
| Designated | NetInvestment | ["Redraft", "De-Designate"] | Yes | Yes |
| Designated | NetInvestment | ["Redraft", "De-Designate"] | No | No |
| Dedesignated | Any | **["Redraft", "De-Designate"]** | Yes | Yes |
| Dedesignated | Any | **["Redraft", "De-Designate"]** | No | No |

**Note:** The bolded row indicates the fix applied in this change.

### Role-Based Permissions
The workflow items are enabled/disabled based on whether the user has one of the required roles:
- Role 24 (Hedge Accounting Administrator)
- Role 17 (Hedge Accounting Manager)  
- Role 5 (Hedge Accounting User)

If the user does NOT have any of these roles, all workflow items are disabled (grayed out but still visible).

## Legacy JavaScript Logic Analysis

The old `setWorkFlow()` function in `hr_hedgeRelationshipAddEditCtrl.js` used a series of array splice operations:

```javascript
setWorkFlow = function () {
    // Start with: ["Designate", "De-Designate", "Re-Designate"]
    
    if ($scope.Model.HedgeState === 'Draft') {
        $scope.DropDownList.ActionList.splice(1, 1); // Remove "De-Designate"
    }

    if ($scope.Model.HedgeState !== 'Designated' || $scope.Model.HedgeType !== "CashFlow") {
        $scope.DropDownList.ActionList.splice(2, 1); // Remove "Re-Designate"
    }

    if ($scope.Model.HedgeState === 'Designated' || $scope.Model.HedgeState === "Dedesignated") {
        $scope.DropDownList.ActionList.splice(0, 1); // Remove "Designate"
        $scope.DropDownList.ActionList.splice(0, 0, { "Value": "Redraft", "Disabled": false }); // Add "Redraft" at index 0
    }

    $scope.DropDownList.ActionList.map(function (v) {
        v.Disabled = !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'));
    });
};
```

### Tracing for Dedesignated State:
1. **Initial:** `["Designate", "De-Designate", "Re-Designate"]`
2. **After line 459-461:** State !== 'Draft', skip → `["Designate", "De-Designate", "Re-Designate"]`
3. **After line 463-465:** State !== 'Designated' is TRUE, remove index 2 → `["Designate", "De-Designate"]`
4. **After line 467-471:** State === 'Dedesignated' is TRUE, remove index 0 and add "Redraft" → `["Redraft", "De-Designate"]`
5. **After line 473-475:** Set disabled flags → `["Redraft" (disabled?), "De-Designate" (disabled?)]`

**Result:** `["Redraft", "De-Designate"]` ✓

## Testing Recommendations

### Manual Testing
1. **Test Dedesignated State:**
   - Create or navigate to a hedge relationship in Dedesignated state
   - Verify the workflow dropdown shows both "Redraft" and "De-Designate" options
   - With required roles: Both options should be enabled
   - Without required roles: Both options should be disabled

2. **Test Other States (Regression):**
   - **Draft:** Should only show "Designate"
   - **Designated with CashFlow:** Should show "Redraft", "De-Designate", "Re-Designate"
   - **Designated with FairValue:** Should show "Redraft", "De-Designate" (no Re-Designate)

3. **Test Role Permissions:**
   - Test with user having role 24, 17, or 5: All workflow items enabled
   - Test with user without these roles: All workflow items disabled

### Automated Testing
If creating automated tests, verify:
```csharp
[Fact]
public void BuildWorkflowItems_WhenDedesignated_ShowsRedraftAndDeDesignate()
{
    // Arrange
    var hedgeRelationship = new HedgeRelationshipVM 
    { 
        HedgeState = DerivativeEDGEHAEntityEnumHedgeState.Dedesignated 
    };
    
    // Act
    BuildWorkflowItems(hedgeRelationship);
    
    // Assert
    Assert.Equal(2, WorkflowItems.Count);
    Assert.Equal("Redraft", WorkflowItems[0].Text);
    Assert.Equal("De-Designate", WorkflowItems[1].Text);
}
```

## References
- **Legacy File:** `old/hr_hedgeRelationshipAddEditCtrl.js` (lines 458-476)
- **Fixed File:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs` (lines 402-441)
- **Related Ticket:** DE-2731 (Add ability to Re-Draft a hedge relationship in De-Designated status)
- **Related Ticket:** DE-3928 (Show Re-Designate on designated relationship only)
