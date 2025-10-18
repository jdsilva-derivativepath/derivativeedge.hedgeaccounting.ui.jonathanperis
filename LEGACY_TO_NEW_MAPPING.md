# Legacy to New Code Mapping - Test Results Actions

## Quick Reference

### Download Excel Action

#### Legacy (AngularJS)
**File**: `old/hr_hedgeRelationshipAddEditCtrl.js` (lines 1557-1563)
```javascript
if (action === 'Download Excel') {
    var model = $scope.Model;
    model.HedgeRegressionForExport = obj.getSelectedRecords()[0].ID;
    $haService
        .setUrl('HedgeRegressionBatch/Export/Xlsx')
        .download($scope, undefined, 'HedgeRegressionBatch');
}
```

#### New (Blazor)
**File**: `TestResultsTab.razor.cs` (HandleExcelDownload method)
```csharp
private async Task HandleExcelDownload(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM data)
{
    var query = new DownloadTestResultExcelService.Query(data.ID, HedgeRelationship);
    var result = await Mediator.Send(query);
    
    using var streamRef = new DotNetStreamReference(stream: result.ExcelStream);
    await JSRuntime.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);
}
```

**Handler**: `DownloadTestResultExcelService.cs`
```csharp
var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
apiEntity.HedgeRegressionForExport = request.BatchId;

var fileResponse = await hedgeAccountingApiClient.ExportAsync(
    DerivativeEDGEHAEntityEnumFileType.Xlsx, 
    apiEntity, 
    cancellationToken);
```

---

### Delete Action

#### Legacy (AngularJS)
**File**: `old/hr_hedgeRelationshipAddEditCtrl.js` (lines 1565-1579)
```javascript
else if (action === 'Delete') {
    var gridObj = $("#allTestsDiv").ejGrid("instance");
    var selectedRow = gridObj.selectedRowsIndexes[0];
    var selectedItem = $scope.Model.HedgeRegressionBatches[selectedRow];

    if (selectedRow !== undefined && confirm('Are you sure to delete the selected test?')) {
        $haService
            .setUrl('HedgeRelationship/DeleteBatch')
            .setId(selectedItem.ID)
            .post($scope)
            .then(function (response) {
                setModelData(response.data);
            });
    }
}
```

#### New (Blazor)
**File**: `TestResultsTab.razor.cs` (HandleDeleteRequest and HandleDeleteConfirmed methods)
```csharp
// Step 1: Show confirmation
private async Task HandleDeleteRequest(DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM data)
{
    _batchToDelete = data;
    _showDeleteConfirmation = true;
    StateHasChanged();
}

// Step 2: Execute delete after confirmation
private async Task HandleDeleteConfirmed()
{
    var command = new DeleteTestBatchService.Command(_batchToDelete.ID, HedgeRelationship);
    var result = await Mediator.Send(command);

    if (result.IsSuccess)
    {
        // Update parent state (equivalent to setModelData(response.data))
        await HedgeRelationshipChanged.InvokeAsync(result.UpdatedHedgeRelationship);
    }
}
```

**Handler**: `DeleteTestBatchService.cs`
```csharp
var apiEntity = mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);

var updatedHedgeRelationship = await hedgeAccountingApiClient.DeleteBatchAsync(
    request.BatchId, 
    apiEntity, 
    cancellationToken);

return new Response(true, updatedHedgeRelationship);
```

---

### Permission Check (Delete Visibility)

#### Legacy (AngularJS)
**File**: `old/hedgetestResultsView.cshtml` (line 96)
```html
<option value="Delete" data-ng-show="Model.HedgeState === 'Draft' || checkUserRole('24') || checkUserRole('17') || checkUserRole('5')">Delete</option>
```

**Controller**: `old/hr_hedgeRelationshipAddEditCtrl.js`
```javascript
$scope.checkUserRole = function (role) {
    if (!Session.user || !Session.user.Roles)
        return false;
    return Session.userRoles.indexOf(role) > -1;
};
```

#### New (Blazor)
**File**: `TestResultsTab.razor` (grid column template)
```razor
<DropDownMenuItems>
    <DropDownMenuItem Text="Download Excel" />
    @if (CanShowDeleteOption())
    {
        <DropDownMenuItem Text="Delete" />
    }
</DropDownMenuItems>
```

**File**: `TestResultsTab.razor.cs` (permission methods)
```csharp
private bool CanShowDeleteOption()
{
    if (HedgeRelationship == null)
        return false;
        
    return HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft 
           || HasRequiredRole();
}

private bool HasRequiredRole()
{
    return CheckUserRole("24") || CheckUserRole("17") || CheckUserRole("5");
}

private bool CheckUserRole(string role)
{
    if (string.IsNullOrEmpty(role) || UserAuthData?.Roles == null)
        return false;

    if (!int.TryParse(role, out var roleId))
        return false;

    var edgeRole = (DerivativeEDGE.Authorization.AuthClaims.EdgeRole)roleId;
    return UserAuthData.Roles.Contains(edgeRole);
}
```

---

### Confirmation Dialog

#### Legacy (AngularJS)
```javascript
confirm('Are you sure to delete the selected test?')
```

#### New (Blazor)
**File**: `TestResultsTab.razor`
```razor
<DpConfirmationModal Visible="@_showDeleteConfirmation"
                     VisibleChanged="@((bool v) => _showDeleteConfirmation = v)"
                     Title="Confirm Delete"
                     Message="Are you sure to delete the selected test?"
                     ConfirmText="Delete"
                     CancelText="Cancel"
                     OnConfirmed="@HandleDeleteConfirmed"
                     OnCancelled="@HandleDeleteCancelled" />
```

---

### State Update After Delete

#### Legacy (AngularJS)
**File**: `old/hr_hedgeRelationshipAddEditCtrl.js`
```javascript
.then(function (response) {
    setModelData(response.data); // Updates entire model with new data from API
});
```

#### New (Blazor)
**File**: `TestResultsTab.razor.cs`
```csharp
if (result.IsSuccess)
{
    // Invoke parent's callback to update HedgeRelationship state
    await HedgeRelationshipChanged.InvokeAsync(result.UpdatedHedgeRelationship);
}
```

**Parent Component**: `HedgeRelationshipDetails.razor`
```razor
<TestResultsTab @bind-HedgeRelationship="@HedgeRelationship" />
```
This two-way binding ensures the parent component's `HedgeRelationship` is updated when the event is invoked.

---

## API Endpoints

### Download Excel
- **Legacy URL**: `HedgeRegressionBatch/Export/Xlsx`
- **New API**: `POST v1/HedgeRegressionBatch/Export/{ft}`
- **Method**: `hedgeAccountingApiClient.ExportAsync(FileType.Xlsx, body, cancellationToken)`
- **Body**: HedgeRelationship with `HedgeRegressionForExport` set to batch ID

### Delete Batch
- **Legacy URL**: `HedgeRelationship/DeleteBatch` (with ID parameter)
- **New API**: `POST v1/HedgeRelationship/DeleteBatch/{batchid}`
- **Method**: `hedgeAccountingApiClient.DeleteBatchAsync(batchId, body, cancellationToken)`
- **Body**: HedgeRelationship entity
- **Response**: Updated HedgeRelationshipVM

---

## Data Flow Diagrams

### Download Excel Flow
```
User Action (Click "Download Excel")
    ↓
OnItemSelectedMatrix (Switch on "Download Excel")
    ↓
HandleExcelDownload
    ↓
DownloadTestResultExcelService.Query (MediatR)
    ↓
DownloadTestResultExcelService.Handler
    ├─ Map HedgeRelationship to API entity
    ├─ Set HedgeRegressionForExport = BatchId
    └─ Call hedgeAccountingApiClient.ExportAsync
        ↓
    Extract filename from Content-Disposition
        ↓
Return (Stream, FileName)
    ↓
DotNetStreamReference wraps stream
    ↓
JSRuntime.InvokeVoidAsync("downloadFileFromStream")
    ↓
JavaScript creates blob and triggers download
    ↓
User receives file: HedgeRegressionBatch_{id}_{timestamp}.xlsx
```

### Delete Flow
```
User Action (Click "Delete")
    ↓
OnItemSelectedMatrix (Switch on "Delete")
    ↓
HandleDeleteRequest
    ├─ Set _batchToDelete = selected batch
    └─ Set _showDeleteConfirmation = true
        ↓
DpConfirmationModal appears
    ↓
User clicks "Delete" button
    ↓
HandleDeleteConfirmed
    ↓
DeleteTestBatchService.Command (MediatR)
    ↓
DeleteTestBatchService.Handler
    ├─ Map HedgeRelationship to API entity
    └─ Call hedgeAccountingApiClient.DeleteBatchAsync
        ↓
    Receive updated HedgeRelationshipVM
        ↓
Return Response(IsSuccess=true, UpdatedHedgeRelationship)
    ↓
Invoke HedgeRelationshipChanged.InvokeAsync
    ↓
Parent HedgeRelationshipDetails updates its HedgeRelationship state
    ↓
Grid automatically refreshes with updated data
    ↓
Success toast shown to user
```

---

## Key Differences

| Aspect | Legacy (AngularJS) | New (Blazor) |
|--------|-------------------|--------------|
| **Architecture** | Controller + Service | MediatR Handler + Component |
| **Confirmation** | Browser `confirm()` | `DpConfirmationModal` component |
| **File Download** | AngularJS $http.download | JSRuntime + DotNetStreamReference |
| **State Update** | `setModelData()` updates $scope | `EventCallback` updates parent state |
| **Permission Check** | `checkUserRole()` checks session | `CheckUserRole()` checks IUserAuthData |
| **Error Handling** | Promise rejection | try-catch with ILogger |
| **User Feedback** | Implicit | Explicit toast notifications |

---

## Testing Scenarios

### Download Excel
1. ✅ Click Download Excel → file downloads with correct name
2. ✅ File contains correct data for selected batch
3. ✅ Error handling: API failure shows error toast
4. ✅ Multiple rapid clicks don't cause issues

### Delete
1. ✅ Draft state + any user → Delete option visible
2. ✅ Designated state + role 24 → Delete option visible
3. ✅ Designated state + role 17 → Delete option visible
4. ✅ Designated state + role 5 → Delete option visible
5. ✅ Designated state + no role → Delete option hidden
6. ✅ Click Delete → Confirmation modal appears
7. ✅ Cancel confirmation → No deletion occurs
8. ✅ Confirm deletion → Batch removed from grid
9. ✅ After delete → Parent state updates automatically
10. ✅ Error handling: API failure shows error toast
