# Option Amortization Tab Visibility Fix

## Problem Statement
When clicking on "Hedge is an Option" checkbox on the HedgeRelationshipDetails page, the Option Amortization tab did not immediately appear or disappear, even though the visibility binding was already correctly configured.

## Root Cause
The Syncfusion `SfTab` component maintains internal state for tab items. When the `Visible` property of a `TabItem` changes through a bound expression, the tab component needs to be explicitly refreshed to re-evaluate the visibility conditions and update the UI.

## Solution
Added explicit tab refresh calls using `hedgerelationshiptabRef.RefreshAsync()` when the checkbox state changes.

### Files Modified
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`

### Methods Updated

#### 1. OnIsAnOptionHedgeChanged
**Before:**
```csharp
private void OnIsAnOptionHedgeChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        HedgeRelationship.IsAnOptionHedge = args.Checked;
        
        // ... other logic ...
        
        StateHasChanged();
    }
}
```

**After:**
```csharp
private async Task OnIsAnOptionHedgeChanged(Syncfusion.Blazor.Buttons.ChangeEventArgs<bool> args)
{
    if (HedgeRelationship != null)
    {
        HedgeRelationship.IsAnOptionHedge = args.Checked;
        
        // ... other logic ...
        
        // Refresh the tab component to update visibility of Option Amortization tab
        if (hedgerelationshiptabRef != null)
        {
            await hedgerelationshiptabRef.RefreshAsync();
        }
        
        StateHasChanged();
    }
}
```

#### 2. OnOffMarketChanged
Also updated this method to refresh the tab when Off-Market checkbox is checked, which programmatically unchecks the IsAnOptionHedge checkbox.

## Legacy Behavior Reference
From `old/detailsView.cshtml` line 436:
```html
<li id="optionTimeValueHedgeRelationshipli" data-index="5" 
    data-ng-show="Model.IsAnOptionHedge && Model.HedgeState !== 'Dedesignated'">
    <a href="#tabs-hedgeRelationship-6">Option Amortization</a>
</li>
```

The legacy AngularJS implementation used `data-ng-show` which automatically updates visibility when the model changes. In Blazor with Syncfusion, we need the explicit `RefreshAsync()` call.

## Visibility Rules
The Option Amortization tab is visible when:
1. `IsAnOptionHedge` is `true` AND
2. `HedgeState` is NOT `Dedesignated`

This is already correctly implemented in the Razor markup:
```razor
<TabItem Visible="@(HedgeRelationship.IsAnOptionHedge && 
                    HedgeRelationship.HedgeState != DerivativeEDGEHAEntityEnumHedgeState.Dedesignated)">
```

## Testing
To test this fix:
1. Open a Hedge Relationship in Draft or Designated state
2. Uncheck "Hedge is an Option" checkbox
3. Verify Option Amortization tab disappears
4. Check "Hedge is an Option" checkbox
5. Verify Option Amortization tab appears
6. Check "Off-Market" checkbox
7. Verify Option Amortization tab disappears (because Off-Market unchecks IsAnOptionHedge)

## Related Behavior
The "New" dropdown button already correctly shows/hides menu items based on `IsAnOptionHedge`:
- When unchecked: Shows both "Amortization" and "Option Amortization" items
- When checked: Shows only "Option Amortization" item

The tab visibility now has the same dynamic behavior.
