# Testing Guide: Hedge Type Field Visibility Fix

## Quick Reference: What Changed?

### Before Fix ❌
- **Benchmark/Contractual Rate field** was ALWAYS visible regardless of HedgeRiskType
- **Acquisition checkbox** had incorrect visibility logic

### After Fix ✅
- **Benchmark/Contractual Rate field** only shows when HedgeRiskType = InterestRate
- **Acquisition checkbox** always visible when HedgeType = CashFlow
- Field label changes dynamically based on HedgeType

## Testing Instructions

### Setup
1. Navigate to Hedge Relationship Details page
2. Locate the middle card "Risk Details" section
3. Find the Benchmark/Contractual Rate field and checkboxes

### Test Cases

#### Test 1: HedgeRiskType = InterestRate + HedgeType = CashFlow
**Steps:**
1. Set "Hedged Risk" dropdown to "Interest Rate"
2. Set "Hedged Type" dropdown to "Cash Flow"

**Expected Results:**
- ✅ Field labeled "Contractual Rate" should be VISIBLE
- ✅ "Pre-Issuance Hedge" checkbox should be VISIBLE
- ✅ "Acquisition" checkbox should be VISIBLE (check spelling is correct)
- ✅ "Portfolio Layer Method" checkbox should be HIDDEN
- ✅ "Shortcut" checkbox should be HIDDEN

---

#### Test 2: HedgeRiskType = InterestRate + HedgeType = FairValue
**Steps:**
1. Set "Hedged Risk" dropdown to "Interest Rate"
2. Set "Hedged Type" dropdown to "Fair Value"

**Expected Results:**
- ✅ Field labeled "Benchmark" should be VISIBLE
- ✅ "Portfolio Layer Method" checkbox should be VISIBLE
- ✅ "Shortcut" checkbox should be VISIBLE
- ✅ "Pre-Issuance Hedge" checkbox should be HIDDEN
- ✅ "Acquisition" checkbox should be HIDDEN

---

#### Test 3: HedgeRiskType = InterestRate + HedgeType = NetInvestment
**Steps:**
1. Set "Hedged Risk" dropdown to "Interest Rate"
2. Set "Hedged Type" dropdown to "Net Investment"

**Expected Results:**
- ✅ Field labeled "Benchmark" should be VISIBLE
- ✅ "Pre-Issuance Hedge" checkbox should be HIDDEN
- ✅ "Acquisition" checkbox should be HIDDEN
- ✅ "Portfolio Layer Method" checkbox should be HIDDEN
- ✅ "Shortcut" checkbox should be HIDDEN

---

#### Test 4: HedgeRiskType = ForeignExchange + HedgeType = CashFlow
**Steps:**
1. Set "Hedged Risk" dropdown to "Foreign Exchange"
2. Set "Hedged Type" dropdown to "Cash Flow"

**Expected Results:**
- ✅ Benchmark/Contractual Rate field should be HIDDEN (completely absent from UI)
- ✅ "Pre-Issuance Hedge" checkbox should be VISIBLE
- ✅ "Acquisition" checkbox should be VISIBLE
- ✅ "Portfolio Layer Method" checkbox should be HIDDEN
- ✅ "Shortcut" checkbox should be HIDDEN

---

#### Test 5: HedgeRiskType = ForeignExchange + HedgeType = FairValue
**Steps:**
1. Set "Hedged Risk" dropdown to "Foreign Exchange"
2. Set "Hedged Type" dropdown to "Fair Value"

**Expected Results:**
- ✅ Benchmark/Contractual Rate field should be HIDDEN (completely absent from UI)
- ✅ "Portfolio Layer Method" checkbox should be VISIBLE
- ✅ "Shortcut" checkbox should be VISIBLE
- ✅ "Pre-Issuance Hedge" checkbox should be HIDDEN
- ✅ "Acquisition" checkbox should be HIDDEN

---

#### Test 6: HedgeRiskType = Commodity + Any HedgeType
**Steps:**
1. Set "Hedged Risk" dropdown to "Commodity"
2. Try each "Hedged Type" option (Cash Flow, Fair Value, Net Investment)

**Expected Results:**
- ✅ Benchmark/Contractual Rate field should be HIDDEN for ALL HedgeType selections
- ✅ Checkboxes should follow the same rules as Test 4 and Test 5 based on HedgeType

---

### Dynamic Field Behavior Test

#### Test 7: Label Change on HedgeType Switch
**Steps:**
1. Set "Hedged Risk" to "Interest Rate"
2. Set "Hedged Type" to "Cash Flow"
3. Verify field shows "Contractual Rate"
4. Change "Hedged Type" to "Fair Value"
5. Verify field label changes to "Benchmark"

**Expected Results:**
- ✅ Field remains visible throughout
- ✅ Label changes from "Contractual Rate" to "Benchmark"
- ✅ No page refresh required

---

#### Test 8: Field Appearance/Disappearance on HedgeRiskType Switch
**Steps:**
1. Set "Hedged Risk" to "Interest Rate"
2. Set "Hedged Type" to "Cash Flow"
3. Verify "Contractual Rate" field is visible
4. Change "Hedged Risk" to "Foreign Exchange"
5. Verify field disappears

**Expected Results:**
- ✅ Field visible when HedgeRiskType = Interest Rate
- ✅ Field disappears when switching to Foreign Exchange
- ✅ "Pre-Issuance Hedge" and "Acquisition" checkboxes remain visible (Cash Flow selected)
- ✅ No page refresh required

---

### Data Persistence Test

#### Test 9: Save and Reload
**Steps:**
1. Set "Hedged Risk" to "Interest Rate"
2. Set "Hedged Type" to "Cash Flow"
3. Select a value in the "Contractual Rate" dropdown
4. Check "Pre-Issuance Hedge" and "Acquisition" checkboxes
5. Click "Save"
6. Reload the page

**Expected Results:**
- ✅ "Contractual Rate" field shows with saved value
- ✅ Both checkboxes remain checked
- ✅ All visibility rules still apply correctly

---

#### Test 10: Cross-Type Data Handling
**Steps:**
1. Set "Hedged Risk" to "Interest Rate"
2. Set "Hedged Type" to "Cash Flow"
3. Select a value in "Contractual Rate" dropdown
4. Check "Pre-Issuance Hedge" checkbox
5. Change "Hedged Type" to "Fair Value"
6. Click "Save"

**Expected Results:**
- ✅ "Benchmark" field visible (label changed from "Contractual Rate")
- ✅ "Pre-Issuance Hedge" checkbox disappears (but data preserved in backend)
- ✅ "Portfolio Layer Method" and "Shortcut" checkboxes appear
- ✅ Save succeeds without errors

---

## Visual Checklist

Use this checklist during testing to quickly verify all field states:

### Interest Rate + Cash Flow
```
☑ Contractual Rate field (visible)
☑ Pre-Issuance Hedge (visible)
☑ Acquisition (visible)
☐ Portfolio Layer Method (hidden)
☐ Shortcut (hidden)
```

### Interest Rate + Fair Value
```
☑ Benchmark field (visible)
☐ Pre-Issuance Hedge (hidden)
☐ Acquisition (hidden)
☑ Portfolio Layer Method (visible)
☑ Shortcut (visible)
```

### Interest Rate + Net Investment
```
☑ Benchmark field (visible)
☐ Pre-Issuance Hedge (hidden)
☐ Acquisition (hidden)
☐ Portfolio Layer Method (hidden)
☐ Shortcut (hidden)
```

### Foreign Exchange + Cash Flow
```
☐ Benchmark/Contractual Rate (hidden completely)
☑ Pre-Issuance Hedge (visible)
☑ Acquisition (visible)
☐ Portfolio Layer Method (hidden)
☐ Shortcut (hidden)
```

### Foreign Exchange + Fair Value
```
☐ Benchmark/Contractual Rate (hidden completely)
☐ Pre-Issuance Hedge (hidden)
☐ Acquisition (hidden)
☑ Portfolio Layer Method (visible)
☑ Shortcut (visible)
```

## Known Issues / Limitations

### Not Implemented (Out of Scope)
- **Benchmark Dropdown Filtering**: The legacy system filters available benchmark options based on HedgeType. This filtering logic exists in the codebase (`HedgeRelationshipLabelHelper.FilterBenchmarkList()`) but is not currently applied. This is a separate enhancement.

### Spelling Corrections
- Fixed typo: "Aquisition" → "Acquisition" ✅

## Questions to Answer During Testing

1. Does the field appear/disappear smoothly without flickering? ✅ / ❌
2. Does the label change immediately when switching HedgeType? ✅ / ❌
3. Do checkboxes show/hide correctly for all combinations? ✅ / ❌
4. Does data persist correctly after save/reload? ✅ / ❌
5. Are there any console errors when switching types? ✅ / ❌
6. Does the UI remain responsive during type switches? ✅ / ❌

## Reporting Issues

If you find any issues during testing, please report with:
1. **Steps to reproduce**
2. **Expected behavior**
3. **Actual behavior**
4. **HedgeRiskType and HedgeType values used**
5. **Screenshots if applicable**

## Success Criteria

✅ All 10 test cases pass
✅ All visibility rules match legacy system
✅ Field labels change correctly
✅ Data persists correctly
✅ No console errors
✅ Smooth UI transitions
