# Manual Testing Guide - Workflow Permission Fix

## Overview
This guide provides step-by-step instructions to verify the workflow permission fix is working correctly.

## Prerequisites
- Access to the Blazor application
- Test users with different role configurations
- Hedge Relationships in various states (Draft, Designated, Dedesignated)

## Test Users Required

### User A: Has Required Role
- Must have at least ONE of: Role 24, 17, or 5
- **Expected:** All workflow actions should be enabled

### User B: No Required Roles
- Must NOT have roles 24, 17, or 5
- **Expected:** All workflow actions should be disabled (grayed out)

## Test Scenarios

### Scenario 1: Draft State with Required Role
**User:** User A (has role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Draft** state
3. Click on the **"Workflow"** dropdown button (top right)
4. **Verify:** 
   - ✅ "Designate" option appears
   - ✅ "Designate" option is **enabled** (clickable, not grayed out)
5. **Expected Result:** PASS if "Designate" is enabled

### Scenario 2: Draft State without Required Role
**User:** User B (does NOT have role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Draft** state
3. Click on the **"Workflow"** dropdown button
4. **Verify:**
   - ✅ "Designate" option appears
   - ✅ "Designate" option is **disabled** (grayed out, not clickable)
5. **Expected Result:** PASS if "Designate" is disabled

### Scenario 3: Designated State (CashFlow) with Required Role
**User:** User A (has role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Designated** state with **HedgeType = CashFlow**
3. Click on the **"Workflow"** dropdown button
4. **Verify:**
   - ✅ "Redraft" option appears and is **enabled**
   - ✅ "De-Designate" option appears and is **enabled**
   - ✅ "Re-Designate" option appears and is **enabled**
5. **Expected Result:** PASS if all three options are enabled

### Scenario 4: Designated State (FairValue) with Required Role
**User:** User A (has role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Designated** state with **HedgeType = FairValue**
3. Click on the **"Workflow"** dropdown button
4. **Verify:**
   - ✅ "Redraft" option appears and is **enabled**
   - ✅ "De-Designate" option appears and is **enabled**
   - ❌ "Re-Designate" option does NOT appear (this is correct behavior)
5. **Expected Result:** PASS if only Redraft and De-Designate are shown and enabled

### Scenario 5: Dedesignated State with Required Role
**User:** User A (has role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Dedesignated** state
3. Click on the **"Workflow"** dropdown button
4. **Verify:**
   - ✅ "Redraft" option appears and is **enabled**
   - ✅ "De-Designate" option appears and is **enabled**
   - ❌ "Re-Designate" option does NOT appear (this is correct behavior)
5. **Expected Result:** PASS if Redraft and De-Designate are enabled

### Scenario 6: Designated State without Required Role
**User:** User B (does NOT have role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Designated** state (any type)
3. Click on the **"Workflow"** dropdown button
4. **Verify:**
   - ✅ Workflow options appear
   - ✅ All options are **disabled** (grayed out)
5. **Expected Result:** PASS if all options are disabled

### Scenario 7: Additional Button Permissions
**User:** User A (has role 24, 17, or 5)

1. Login to the application
2. Navigate to a Hedge Relationship in **Draft** state
3. **Verify Save Button:**
   - ✅ "Save" button is **enabled**
4. **Verify Run Regression Button:**
   - ✅ "Run Regression" button is **enabled** (if Benchmark is set)
5. **Verify Preview Inception Package Button:**
   - ✅ "Preview Inception Package" button state depends on LatestHedgeRegressionBatch
6. **Expected Result:** PASS if Save and Run Regression are enabled in Draft state

### Scenario 8: Legacy System Comparison
**User:** Same user in both systems

1. Login to **Legacy AngularJS** system
2. Navigate to a specific Hedge Relationship (note the ID)
3. Check which workflow actions are enabled/disabled
4. Login to **New Blazor** system with same user
5. Navigate to the SAME Hedge Relationship (use same ID)
6. Check which workflow actions are enabled/disabled
7. **Verify:**
   - ✅ Workflow options match exactly between legacy and new system
   - ✅ Enabled/disabled state matches exactly
8. **Expected Result:** PASS if behavior is identical

## Quick Test Matrix

| User Role | State | Expected Workflow Actions | Enabled? |
|-----------|-------|--------------------------|----------|
| Has 24/17/5 | Draft | Designate | ✅ Yes |
| No 24/17/5 | Draft | Designate | ❌ No |
| Has 24/17/5 | Designated (CF) | Redraft, De-Designate, Re-Designate | ✅ Yes |
| Has 24/17/5 | Designated (FV) | Redraft, De-Designate | ✅ Yes |
| Has 24/17/5 | Dedesignated | Redraft, De-Designate | ✅ Yes |
| No 24/17/5 | Designated | Redraft, De-Designate | ❌ No |
| No 24/17/5 | Dedesignated | Redraft, De-Designate | ❌ No |

**Legend:** CF = CashFlow, FV = FairValue

## Debugging Tips

### How to Check User Roles
1. Open browser Developer Tools (F12)
2. Go to Console tab
3. Check the user's JWT token or claims
4. Look for "roles" claim containing numeric values like "24", "17", "5"

### How to Verify Role Check Logic
1. Add breakpoint in `CheckUserRole` method (line 753)
2. When breakpoint hits, inspect:
   - `role` parameter value (should be "24", "17", or "5")
   - `roleId` after parsing (should be integer 24, 17, or 5)
   - `edgeRole` after casting (should be EdgeRole enum value)
   - `UserAuthData.Roles` collection (should contain EdgeRole enum values)
   - Return value (should be true if user has the role)

### Common Issues
- **All actions disabled:** User might not have roles 24, 17, or 5
- **Role check fails:** Check if UserAuthData.Roles is properly populated
- **Enum conversion error:** Verify EdgeRole enum contains values 24, 17, 5

## Test Results Template

Copy and fill this out:

```
Test Date: [DATE]
Tester: [NAME]
Environment: [DEV/QA/PROD]

Scenario 1 (Draft + Role): ☐ PASS ☐ FAIL - Notes: _____
Scenario 2 (Draft - Role): ☐ PASS ☐ FAIL - Notes: _____
Scenario 3 (Designated CF + Role): ☐ PASS ☐ FAIL - Notes: _____
Scenario 4 (Designated FV + Role): ☐ PASS ☐ FAIL - Notes: _____
Scenario 5 (Dedesignated + Role): ☐ PASS ☐ FAIL - Notes: _____
Scenario 6 (Designated - Role): ☐ PASS ☐ FAIL - Notes: _____
Scenario 7 (Additional Buttons): ☐ PASS ☐ FAIL - Notes: _____
Scenario 8 (Legacy Comparison): ☐ PASS ☐ FAIL - Notes: _____

Overall Result: ☐ ALL PASS ☐ FAILED

Additional Notes:
_________________________________________________
_________________________________________________
```

## Success Criteria
- All 8 scenarios must PASS
- Behavior must match legacy AngularJS system exactly
- No console errors related to role checking
- Workflow actions work as expected when clicked

## Rollback Plan
If testing fails:
1. Revert commit: `6b039f3` (documentation) and `63058db` (code fix)
2. Original behavior: All workflow actions will be disabled for all users
3. Re-investigate the issue with additional logging
