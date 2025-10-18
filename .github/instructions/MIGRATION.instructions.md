# Migration Instructions - Legacy Code Reference

## Purpose
This instruction file applies when working with files in the `./old/` directory or when migrating legacy functionality.

## Critical Rules for Migration Tasks

### READ ONLY - Legacy Files
The `./old/` directory contains **reference-only** legacy code from the AngularJS + .NET Framework system:
- **DO NOT modify files in `./old/`** - They are historical references
- **DO NOT delete files in `./old/`** - They document original behavior
- **DO use them as the source of truth** for expected functionality

### Lift-and-Shift Philosophy
When assigned a migration task, you are a Software Engineer performing a **lift-and-shift migration**:

1. **Preserve ALL Business Logic**
   - Every condition, calculation, and rule must be migrated exactly as-is
   - If legacy code seems odd or inefficient, migrate it anyway
   - Document quirks with comments referencing the legacy file/line

2. **No Feature Additions**
   - Don't add validation that didn't exist in legacy
   - Don't add error handling that wasn't there
   - Don't improve UX without explicit approval

3. **No Feature Removals**
   - If legacy code checks something, new code must check it too
   - If legacy code has a workflow step, new code must have it
   - If legacy code displays a field, new code must display it

4. **Expected Results Must Match**
   - Same workflow options in same states
   - Same calculations with same rounding
   - Same data displayed in same order
   - Same permissions and role checks

## Legacy Code Structure

### AngularJS Controller Pattern
```javascript
// Legacy: old/hr_hedgeRelationshipAddEditCtrl.js
app.controller('hedgeRelationshipAddEditCtrl', function($scope, $http) {
    $scope.init = function(id) {
        // Initialization logic
    };
    
    $scope.methodName = function() {
        // Business logic
    };
});
```

**Migrates to Blazor Code-Behind:**
```csharp
// New: HedgeRelationshipDetails.razor.cs
public partial class HedgeRelationshipDetails : ComponentBase
{
    protected override async Task OnInitializedAsync()
    {
        // Initialization logic from $scope.init
    }
    
    private async Task MethodName()
    {
        // Business logic from $scope.methodName
    }
}
```

### AngularJS View Pattern
```html
<!-- Legacy: old/HedgeRelationship.cshtml -->
<div data-ng-controller="hedgeRelationshipAddEditCtrl">
    <div data-ng-if="condition">{{expression}}</div>
</div>
```

**Migrates to Blazor Razor:**
```razor
<!-- New: HedgeRelationshipDetails.razor -->
<div>
    @if (condition)
    {
        <div>@expression</div>
    }
</div>
```

### AngularJS HTTP Calls
```javascript
// Legacy: AngularJS
$http.get('/api/hedgerelationships/' + id).success(function(data) {
    $scope.hedgeRelationship = data;
});
```

**Migrates to MediatR Handler:**
```csharp
// New: Blazor with MediatR
var result = await Mediator.Send(new GetHedgeRelationshipById.Query { Id = id });
if (result.IsSuccess)
{
    hedgeRelationship = result.Value;
}
```

## Common Legacy Patterns to Watch For

### 1. Role Checking
**Legacy:**
```javascript
function checkUserRole(roleId) {
    return Session.userRoles.indexOf(roleId) > -1;
}

if (checkUserRole('24') || checkUserRole('17') || checkUserRole('5')) {
    // Enable action
}
```

**New:**
```csharp
private async Task<bool> HasRequiredRole()
{
    var userRoles = await GetUserRoles();
    return userRoles.Contains("24") || 
           userRoles.Contains("17") || 
           userRoles.Contains("5");
}
```

### 2. Array Manipulation
**Legacy JavaScript:**
```javascript
var items = ["Designate", "De-Designate", "Re-Designate"];
items.splice(2, 1); // Remove "Re-Designate" → ["Designate", "De-Designate"]
items[0] = "Redraft"; // Replace first → ["Redraft", "De-Designate"]
```

**New C#:**
```csharp
var items = new List<string> { "Designate", "De-Designate", "Re-Designate" };
items.RemoveAt(2); // Remove "Re-Designate"
items[0] = "Redraft"; // Replace first
// Result: ["Redraft", "De-Designate"]
```

⚠️ **Warning:** Don't use LINQ `.Select()` or other "clever" approaches - migrate the exact logic pattern.

### 3. Workflow State Logic
**Legacy:**
```javascript
function setWorkFlow() {
    if (vm.hedgeRelationshipAddEdit.hedgeState === 'Draft') {
        vm.workflowItems = ["Designate"];
    } else if (vm.hedgeRelationshipAddEdit.hedgeState === 'Designated') {
        if (vm.hedgeRelationshipAddEdit.hedgeType === 'CashFlow') {
            vm.workflowItems = ["Redraft", "De-Designate", "Re-Designate"];
        } else {
            vm.workflowItems = ["Redraft", "De-Designate"];
        }
    } else if (vm.hedgeRelationshipAddEdit.hedgeState === 'Dedesignated') {
        // Original: ["Designate", "De-Designate", "Re-Designate"]
        vm.workflowItems.splice(2, 1); // Remove "Re-Designate"
        vm.workflowItems[0] = "Redraft"; // Replace "Designate"
        // Final: ["Redraft", "De-Designate"]
    }
}
```

**New (Correct):**
```csharp
private void BuildWorkflowItems()
{
    WorkflowItems.Clear();
    var hasPermission = await HasRequiredRole();
    
    if (state == HedgeState.Draft)
    {
        WorkflowItems.Add(new DropDownMenuItem { Text = "Designate", Disabled = !hasPermission });
    }
    else if (state == HedgeState.Designated)
    {
        if (hedgeType == HedgeType.CashFlow)
        {
            WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasPermission });
            WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasPermission });
            WorkflowItems.Add(new DropDownMenuItem { Text = "Re-Designate", Disabled = !hasPermission });
        }
        else
        {
            WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasPermission });
            WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasPermission });
        }
    }
    else if (state == HedgeState.Dedesignated)
    {
        // Match legacy: ["Redraft", "De-Designate"] (not just "Redraft")
        WorkflowItems.Add(new DropDownMenuItem { Text = "Redraft", Disabled = !hasPermission });
        WorkflowItems.Add(new DropDownMenuItem { Text = "De-Designate", Disabled = !hasPermission });
    }
}
```

## Migration Verification Checklist

When migrating a feature, verify:
- [ ] All legacy conditions/branches are present in new code
- [ ] Array operations produce same results as JavaScript
- [ ] Role checks use exact same role IDs
- [ ] Workflow states have exact same action options
- [ ] Data displayed in same order/format
- [ ] Calculations use same precision/rounding
- [ ] Error messages match (or are improved with approval)
- [ ] No new validations added
- [ ] No old validations removed
- [ ] Comments reference legacy file/line for complex logic

## Testing Without Building

Since you cannot build the project:
1. **Trace through code mentally** - Follow execution paths
2. **Compare side-by-side** - Legacy JS next to new C#
3. **Check API objects** - Reference `api/HedgeAccountingApiClient.cs`
4. **Document assumptions** - If unsure, note in comments
5. **Request manual testing** - Ask user to verify behavior

## Example: Complete Migration Pattern

**Legacy (old/hr_hedgeRelationshipAddEditCtrl.js):**
```javascript
$scope.disableSave = function() {
    if ($scope.hedgeRelationshipAddEdit.hedgeState === 'Designated' ||
        $scope.hedgeRelationshipAddEdit.hedgeState === 'Dedesignated') {
        return true;
    }
    return !(checkUserRole('24') || checkUserRole('17') || checkUserRole('5'));
};
```

**New (HedgeRelationshipDetails.razor.cs):**
```csharp
private async Task<bool> IsSaveDisabled()
{
    // Disable save for Designated and Dedesignated states (legacy: disableSave())
    if (HedgeRelationship.HedgeState == HedgeState.Designated ||
        HedgeRelationship.HedgeState == HedgeState.Dedesignated)
    {
        return true;
    }
    
    // Enable only for users with required roles (legacy: checkUserRole('24') || '17' || '5')
    var hasRequiredRole = await HasRequiredRole();
    return !hasRequiredRole;
}
```

## Additional Resources
- `WORKFLOW_COMPARISON.md` - Expected workflow behavior (verified against legacy)
- `WORKFLOW_FIX_DOCUMENTATION.md` - Detailed workflow fix documentation
- `FIX_SUMMARY.md` - Summary of known migration issues and resolutions
- `api/HedgeAccountingApiClient.cs` - All API models and endpoints (1.6MB)
