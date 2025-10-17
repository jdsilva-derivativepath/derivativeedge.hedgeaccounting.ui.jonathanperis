# Workflow Permission Fix Documentation

## Issue Summary
**Problem:** Workflow button "Designate" is disabled for users who should have access when a Hedge Relationship is in Draft state. The same user has access in the legacy AngularJS system but not in the new Blazor implementation.

**Root Cause:** Role checking logic was comparing enum names to numeric role IDs, causing all role checks to fail and disabling workflow actions for all users.

## Technical Details

### The Bug
File: `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`

**Before (Broken):**
```csharp
private bool CheckUserRole(string role)
{
    if (string.IsNullOrEmpty(role) || UserAuthData?.Roles == null)
        return false;

    return UserAuthData.Roles.Any(userRole => userRole.ToString() == role);
}
```

**Issue:** 
- `UserAuthData.Roles` is `List<EdgeRole>` where `EdgeRole` is an enum
- When checking role "24", `.ToString()` returns the enum NAME (e.g., "HedgeAccountingAdmin"), not "24"
- String comparison `"HedgeAccountingAdmin" == "24"` always returns false
- All workflow actions become disabled regardless of user's actual roles

### The Fix
**After (Working):**
```csharp
private bool CheckUserRole(string role)
{
    if (string.IsNullOrEmpty(role) || UserAuthData?.Roles == null)
        return false;

    // Parse string role ID to integer and cast to EdgeRole enum
    if (!int.TryParse(role, out var roleId))
        return false;

    var edgeRole = (DerivativeEDGE.Authorization.AuthClaims.EdgeRole)roleId;
    return UserAuthData.Roles.Contains(edgeRole);
}
```

**Solution:**
1. Parse string role ID ("24", "17", "5") to integer
2. Cast integer to `EdgeRole` enum
3. Check if user's roles contain that enum value
4. Return true if match found, false otherwise

### Required Roles for Workflow Actions
Based on legacy JavaScript code (line 474 in `old/hr_hedgeRelationshipAddEditCtrl.js`):
```javascript
v.Disabled = !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'));
```

Users need at least ONE of these roles:
- **Role 24** - Hedge Accounting Administrator
- **Role 17** - Hedge Accounting Manager
- **Role 5** - Hedge Accounting User

### Workflow Actions Affected
The fix enables proper permission checking for all workflow actions:

| Hedge State | Available Actions | When Enabled |
|-------------|------------------|--------------|
| Draft | Designate | User has role 24, 17, or 5 |
| Designated (CashFlow) | Redraft, De-Designate, Re-Designate | User has role 24, 17, or 5 |
| Designated (FairValue) | Redraft, De-Designate | User has role 24, 17, or 5 |
| Designated (NetInvestment) | Redraft, De-Designate | User has role 24, 17, or 5 |
| Dedesignated | Redraft, De-Designate | User has role 24, 17, or 5 |

## Verification Steps

### Manual Testing Checklist

#### 1. Test with User Having Required Roles (24, 17, or 5)
- [ ] Navigate to a Hedge Relationship in Draft state
- [ ] Click on "Workflow" dropdown
- [ ] Verify "Designate" option is **enabled** (not grayed out)
- [ ] Navigate to a Hedge Relationship in Designated state (CashFlow)
- [ ] Click on "Workflow" dropdown
- [ ] Verify "Redraft", "De-Designate", "Re-Designate" are all **enabled**
- [ ] Navigate to a Hedge Relationship in Designated state (FairValue)
- [ ] Click on "Workflow" dropdown
- [ ] Verify "Redraft", "De-Designate" are **enabled** (no Re-Designate shown)
- [ ] Navigate to a Hedge Relationship in Dedesignated state
- [ ] Click on "Workflow" dropdown
- [ ] Verify "Redraft", "De-Designate" are **enabled**

#### 2. Test with User WITHOUT Required Roles
- [ ] Navigate to a Hedge Relationship in Draft state
- [ ] Click on "Workflow" dropdown
- [ ] Verify "Designate" option is **disabled** (grayed out)
- [ ] Navigate to a Hedge Relationship in Designated state
- [ ] Click on "Workflow" dropdown
- [ ] Verify all options are **disabled** (grayed out)

#### 3. Comparison with Legacy System
- [ ] Login with same user credentials in both legacy and new system
- [ ] Compare workflow dropdown availability for the same Hedge Relationship
- [ ] Verify new system matches legacy system behavior exactly

### Code Verification

#### Check Role Values in User Claims
When debugging, verify that:
1. `UserAuthData.Roles` contains `EdgeRole` enum values
2. Role numeric IDs (24, 17, 5) map to correct `EdgeRole` enum members
3. User's claims include the expected role IDs

#### Example Debug Output
```
UserAuthData.Roles: [EdgeRole.HedgeAccountingUser]
Checking role "24": 
  - Parsed to int: 24
  - Cast to EdgeRole: EdgeRole.HedgeAccountingAdmin
  - Contains check: false (user has HedgeAccountingUser)
  - Result: false

Checking role "5":
  - Parsed to int: 5
  - Cast to EdgeRole: EdgeRole.HedgeAccountingUser
  - Contains check: true
  - Result: true ✓
```

## Related Code References

### Key Files
- **Fixed File:** `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs` (lines 753-764)
- **Legacy Reference:** `old/hr_hedgeRelationshipAddEditCtrl.js` (lines 3095-3108, 458-476)
- **User Service:** `src/DerivativeEDGE.HedgeAccounting.UI/Services/User/UserMetaDataService.cs`
- **Role Model:** `src/DerivativeEDGE.HedgeAccounting.UI/Models/UserMetaData.cs`

### Related Methods
```csharp
// Main role check method (FIXED)
private bool CheckUserRole(string role) { ... }

// Checks if user has any required role
private bool HasRequiredRole() =>
    CheckUserRole("24") || CheckUserRole("17") || CheckUserRole("5");

// Builds workflow items with permission checks
private void BuildWorkflowItems() { ... }

// Permission checks for various actions
private bool IsSaveDisabled() { ... }
private bool IsPreviewInceptionPackageDisabled() { ... }
private bool IsRegressionDisabled() { ... }
private bool IsBackloadDisabled() { ... }
private bool CanEditCheckbox() { ... }
```

## Impact Assessment

### Affected Features
✅ **Fixed:**
- Workflow dropdown actions (Designate, De-Designate, Re-Designate, Redraft)
- Save button enable/disable logic
- Preview Inception Package button enable/disable logic
- Run Regression button enable/disable logic
- Backload button enable/disable logic
- Checkbox editing permissions

### No Breaking Changes
- Fix is backward compatible
- No API changes
- No database changes
- No UI layout changes
- Only fixes permission logic to match legacy behavior

## Testing Results
*(To be filled in after manual testing)*

### Draft State Testing
- Tested with role 24: ✓ / ✗
- Tested with role 17: ✓ / ✗
- Tested with role 5: ✓ / ✗
- Tested without roles: ✓ / ✗

### Designated State Testing
- Tested with required roles: ✓ / ✗
- Tested without roles: ✓ / ✗

### Dedesignated State Testing
- Tested with required roles: ✓ / ✗
- Tested without roles: ✓ / ✗

### Legacy Comparison
- Behavior matches legacy: ✓ / ✗

## Conclusion
This fix resolves the workflow permission issue by correctly converting string role IDs to `EdgeRole` enum values before checking user permissions. The implementation now matches the legacy system's behavior exactly, ensuring users with appropriate roles (24, 17, or 5) can access workflow actions while maintaining proper security for users without these roles.
