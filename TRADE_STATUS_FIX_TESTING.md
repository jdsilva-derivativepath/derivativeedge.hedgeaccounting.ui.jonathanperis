# Trade Status Field Fix - Testing Guide

## Problem Fixed
The "Trade Status" column in both Hedged and Hedging grids on the Instrument Analysis tab was displaying empty values.

## Root Cause
The API's `HedgeRelationshipItemVM` model has two properties:
- `ItemStatus` (enum) - Always populated by the API
- `ItemStatusText` (string) - **Not consistently populated by the API**

When hedge relationships are loaded, the API returns data with the `ItemStatus` enum set, but the `ItemStatusText` string field is often null or empty.

## Solution
Added client-side logic to automatically populate `ItemStatusText` from the `ItemStatus` enum value when loading data.

## Files Changed
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipTabs/InstrumentAnalysisTab.razor.cs`

## Changes Made

### 1. New Helper Method: `PopulateItemStatusText()`
Iterates through a list of items and populates `ItemStatusText` from `ItemStatus` when the text field is null or empty.

### 2. New Helper Method: `GetTradeStatusText()`
Converts the `TradeStatus` enum to its string representation. Handles all 23 possible status values:
- Pricing, Executed, Validated, Amended, Cancelled
- Inactive, Booked, Terminated, Novated
- RPABought, RPASold, Matured, Terminate, Archived
- HA, Hedge, Template, DealContingent
- FixPaymentRequested, ReversalRequested, MarkedAsReversed
- CancellationRequested, Tradeable

### 3. Updated: `LoadInstrumentAnalysisData()`
Now calls `PopulateItemStatusText()` for both `HedgedItems` and `HedgingItems` after loading from the hedge relationship.

### 4. Updated: `LinkTradeToHedging()`
Also populates `ItemStatusText` when linking individual trades to ensure consistency.

## Testing Checklist

### Scenario 1: Load Existing Hedge Relationship
**Steps:**
1. Navigate to an existing hedge relationship with hedged/hedging items
2. Click on the "Instrument and Analysis" tab
3. Check the "Trade Status" column in the Hedged Items grid
4. Check the "Trade Status" column in the Hedging Items grid

**Expected Results:**
- Both grids should show the Trade Status values (e.g., "Validated", "HA", "Executed")
- No empty cells in the Trade Status column
- Status values should match what was previously shown in the legacy system

### Scenario 2: Link Existing Trade
**Steps:**
1. Open a hedge relationship (Draft or Designated state)
2. Go to "Instrument and Analysis" tab
3. Click "Select Existing Trade" button for either Hedged or Hedging items
4. Select and link a trade from the list
5. Verify the new row in the grid

**Expected Results:**
- The newly added item should immediately show its Trade Status
- Status should match the trade's actual status (e.g., "Validated")

### Scenario 3: Different Trade Status Values
**Steps:**
1. Link or verify trades with different status values:
   - A Validated trade
   - An HA (Hedge Accounting) trade
   - A Terminated trade
   - An Executed trade
   - A Cancelled trade (if available)

**Expected Results:**
- Each trade should display its correct status text
- Status text should match the legacy system's display

### Scenario 4: Empty Hedge Relationship
**Steps:**
1. Create a new hedge relationship (or open one with no items)
2. Go to "Instrument and Analysis" tab
3. Link a trade to Hedged Items
4. Link a trade to Hedging Items

**Expected Results:**
- Both newly linked trades should show their Trade Status immediately
- No empty status columns

## Verification Against Legacy System

To verify the fix matches the legacy implementation:

1. **Legacy Reference:** `old/hr_hedgeRelationshipAddEditCtrl.js`
   - Grid column definition: Lines 669, 725 - Uses `ItemStatusText` field
   - Data mapping: Lines 2977-2978 - Shows how `ItemStatusText` is set

2. **Compare with Legacy:**
   - Open the same hedge relationship in both legacy and new UI
   - Compare the Trade Status values shown in both systems
   - They should match exactly

## Known Trade Status Values

Common status values you should see during testing:
- **Validated** - Most common for trades linked to hedge accounting
- **HA** - Trades marked for hedge accounting
- **Executed** - Recently executed trades
- **Terminated** - Terminated trades
- **Cancelled** - Cancelled trades
- **Booked** - Booked trades
- **Pricing** - Trades in pricing state

## Troubleshooting

### If Trade Status is still empty:
1. Check browser console for JavaScript errors
2. Verify the hedge relationship has items with `ItemStatus` set
3. Check network tab to see if API is returning data
4. Verify you're testing on the correct branch: `copilot/fix-trade-status-issue`

### If status shows enum value instead of text (e.g., "2" instead of "Validated"):
This would indicate the `GetTradeStatusText()` method isn't being called. Check:
1. The `LoadInstrumentAnalysisData()` method is calling `PopulateItemStatusText()`
2. The `LinkTradeToHedging()` method is populating the text before adding to grid

## Performance Impact
- **Minimal** - The status text population happens in-memory after data is loaded
- No additional API calls required
- Only processes items that have missing `ItemStatusText`
- Uses efficient switch expression for enum conversion

## Future Considerations
- Ideally, the API should populate `ItemStatusText` consistently
- This client-side fix ensures the UI works regardless of API behavior
- If API is updated to populate the field, this code will simply skip items that already have the text set
