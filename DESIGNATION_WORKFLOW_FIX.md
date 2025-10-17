# Hedge Relationship Designation Workflow Fix

## Issue
When attempting to update a hedge relationship workflow from Draft to Designated, the system returned:
```
Failed to update hedge relationship: "An error occurred while updating the entity."
```

## Root Cause
The new Blazor implementation unconditionally called `SaveHedgeRelationshipAsync()` (which performs a PUT operation on the hedge relationship entity) before executing the designation workflow. This caused the API to reject the update because:

1. The hedge relationship was still in Draft state
2. A direct PUT operation on a Draft entity may have validation issues
3. The state transition from Draft to Designated should happen through the designation workflow, not through a general update

## Solution
Modified the designation workflow to match the legacy JavaScript implementation:

### Legacy JavaScript Flow (old/hr_hedgeRelationshipAddEditCtrl.js)
```javascript
initiateDesignation = function () {
    $haService
        .setUrl("HedgeRelationship/FindDocumentTemplate/" + id)
        .get()
        .then(function (response) {
            if (response.data) {
                // If template exists: Save → Reload → Designate
                $scope.submit(undefined, function () {
                    $scope.init(id, function () {
                        designate(checkDocumentTemplateKeywordsOnDesignated);
                    });
                }, "InitiateDesignation");
            }
            else {
                // If no template: Directly designate
                designate(function () {
                    $scope.generatePackage(false);
                });
            }
        });
}
```

### New Blazor Flow (after fix)
```csharp
private async Task HandleDesignateAsync()
{
    // Validate designation requirements
    ValidationErrors = DesignationRequirementsValidator.Validate(HedgeRelationship);
    if (ValidationErrors.Any())
    {
        StateHasChanged();
        return;
    }

    try
    {
        // Check if document template exists (matching legacy behavior)
        var documentTemplateResponse = await Mediator.Send(
            new FindDocumentTemplate.Query(HedgeId));

        if (documentTemplateResponse.HasError)
        {
            await AlertService.ShowToast(documentTemplateResponse.ErrorMessage, AlertKind.Error, "Designation Failed", showButton: true);
            return;
        }

        // If document template exists, save current state before designation (legacy: submit → init → designate)
        if (documentTemplateResponse.HasTemplate)
        {
            await SaveHedgeRelationshipAsync();
            
            // Reload the hedge relationship after save
            await GetHedgeRelationship(HedgeId);
        }

        // Execute designation workflow
        var response = await Mediator.Send(new DesignateHedgeRelationship.Command(HedgeId));
        
        if (response.HasError)
        {
            await AlertService.ShowToast(response.ErrorMessage, AlertKind.Error, "Designation Failed", showButton: true);
            return;
        }

        // Update the local hedge relationship with the latest state
        HedgeRelationship = response.HedgeRelationship;
        
        await AlertService.ShowToast("Hedge Relationship successfully designated.", AlertKind.Success, "Success", showButton: true);
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await AlertService.ShowToast($"Error during designation: {ex.Message}", AlertKind.Error, "Error", showButton: true);
    }
}
```

## Key Changes

### 1. HedgeRelationshipDetails.razor.cs
- **Before:** Always saved before designation
- **After:** Only saves if document template exists
- **Reason:** Matches legacy behavior and prevents unnecessary PUT operations on Draft entities

### 2. DesignateHedgeRelationship.cs
- **Before:** Checked for document template in handler
- **After:** Document template check moved to page-level
- **Reason:** Better separation of concerns; page controls workflow, handler executes designation

## Why This Works

1. **Conditional Save:** Only saves when document template exists, avoiding unnecessary PUT operations on Draft entities
2. **State Transition:** The actual state transition from Draft to Designated happens in the backend during the `GenerateInceptionPackage` API call
3. **Proper Flow:** Matches the legacy implementation exactly:
   - If template: Save → Reload → Designate
   - If no template: Designate directly

## Verification

- ✅ `HandleReDesignateAsync()` already follows the correct conditional save pattern
- ✅ `HandleRedraftAsync()` correctly calls API without pre-saving
- ✅ `DesignationRequirementsValidator` matches all legacy validation rules
- ✅ No similar issues found in other workflow actions (De-Designate, Redraft)

## Testing Notes

This is a **lift-and-shift migration**, so the behavior must exactly match the legacy system. The fix ensures:

1. Draft hedge relationships are not unnecessarily updated before designation
2. Document template existence determines whether to save before designation
3. The API backend handles the actual state transition from Draft to Designated during inception package generation
4. All validation rules from the legacy system are preserved

## Related Files

- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Commands/DesignateHedgeRelationship.cs`
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Validation/DesignationRequirementsValidator.cs`
- `old/hr_hedgeRelationshipAddEditCtrl.js` (legacy reference)
