# Test Results Tab Actions Implementation

## Overview
This document describes the implementation of Download Excel and Delete actions for test results (regression batches) in the HedgeRelationshipDetails page's Test Results tab.

## Legacy Reference
- **File**: `old/hedgetestResultsView.cshtml` (lines 90-98)
- **Controller**: `old/hr_hedgeRelationshipAddEditCtrl.js` (lines 1553-1580)
- **API Endpoints**:
  - Download: `POST v1/HedgeRegressionBatch/Export/{ft}` (FileType.Xlsx)
  - Delete: `POST v1/HedgeRelationship/DeleteBatch/{batchid}`

## Implementation Details

### 1. MediatR Handlers

#### DownloadTestResultExcelService
**Location**: `Features/HedgeRelationships/Handlers/Queries/DownloadTestResultExcelService.cs`

**Purpose**: Downloads a test result (regression batch) as an Excel file.

**Legacy Logic**:
```javascript
// old/hr_hedgeRelationshipAddEditCtrl.js (lines 1557-1563)
var model = $scope.Model;
model.HedgeRegressionForExport = obj.getSelectedRecords()[0].ID;
$haService
    .setUrl('HedgeRegressionBatch/Export/Xlsx')
    .download($scope, undefined, 'HedgeRegressionBatch');
```

**New Implementation**:
- Takes `BatchId` and `HedgeRelationship` as parameters
- Maps HedgeRelationship to API entity
- Sets `HedgeRegressionForExport` property with the batch ID
- Calls `ExportAsync` with `FileType.Xlsx`
- Extracts filename from Content-Disposition header
- Returns stream and filename for client-side download

**Key Features**:
- Proper error handling with logging
- Content-Disposition header parsing (RFC 5987 support)
- Fallback filename generation: `HedgeRegressionBatch_{batchId}_{timestamp}.xlsx`

#### DeleteTestBatchService
**Location**: `Features/HedgeRelationships/Handlers/Commands/DeleteTestBatchService.cs`

**Purpose**: Deletes a test batch and returns the updated hedge relationship.

**Legacy Logic**:
```javascript
// old/hr_hedgeRelationshipAddEditCtrl.js (lines 1565-1579)
if (selectedRow !== undefined && confirm('Are you sure to delete the selected test?')) {
    $haService
        .setUrl('HedgeRelationship/DeleteBatch')
        .setId(selectedItem.ID)
        .post($scope)
        .then(function (response) {
            setModelData(response.data);
        });
}
```

**New Implementation**:
- Takes `BatchId` and `HedgeRelationship` as parameters
- Maps HedgeRelationship to API entity
- Calls `DeleteBatchAsync` API
- Returns success status and updated hedge relationship
- Parent component updates state with the returned data

### 2. UI Components

#### TestResultsTab Updates

**New Parameters**:
```csharp
[Parameter] public DerivativeEDGEHAApiViewModelsHedgeRelationshipVM HedgeRelationship { get; set; }
[Parameter] public EventCallback<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM> HedgeRelationshipChanged { get; set; }
```

**New Services**:
- `IJSRuntime` - For file download
- `IAlertService` - For user notifications
- `IUserAuthData` - For role-based permission checks

**State Management**:
```csharp
private bool _isDeleting = false;
private bool _isDownloading = false;
private bool _showDeleteConfirmation = false;
private DerivativeEDGEHAApiViewModelsHedgeRegressionBatchVM _batchToDelete = null;
```

#### Action Dropdown

**Template with Conditional Delete**:
```razor
<SfDropDownButton CssClass="dp-menu-style">
    <DropDownButtonEvents ItemSelected="@((args) => OnItemSelectedMatrix(args, model))" />
    <DropDownMenuItems>
        <DropDownMenuItem Text="Download Excel" />
        @if (CanShowDeleteOption())
        {
            <DropDownMenuItem Text="Delete" />
        }
    </DropDownMenuItems>
</SfDropDownButton>
```

**Permission Logic** (matching legacy):
```csharp
// Legacy: data-ng-show="Model.HedgeState === 'Draft' || checkUserRole('24') || checkUserRole('17') || checkUserRole('5')"
private bool CanShowDeleteOption()
{
    if (HedgeRelationship == null)
        return false;
        
    return HedgeRelationship.HedgeState == DerivativeEDGEHAEntityEnumHedgeState.Draft || HasRequiredRole();
}
```

#### Confirmation Modal

**Component Usage**:
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

### 3. Event Flow

#### Download Excel Flow
1. User clicks "Download Excel" from dropdown
2. `OnItemSelectedMatrix` -> `HandleExcelDownload`
3. Set `_isDownloading = true`
4. Send MediatR query with batch ID and hedge relationship
5. Receive file stream and filename
6. Use `DotNetStreamReference` to download file via JS interop
7. Show success toast
8. Set `_isDownloading = false`

#### Delete Flow
1. User clicks "Delete" from dropdown (if permitted)
2. `OnItemSelectedMatrix` -> `HandleDeleteRequest`
3. Set `_batchToDelete` and show confirmation modal
4. User confirms deletion
5. `HandleDeleteConfirmed` executes
6. Set `_isDeleting = true`
7. Send MediatR command with batch ID and hedge relationship
8. Receive updated hedge relationship
9. Invoke `HedgeRelationshipChanged` to update parent state
10. Show success toast
11. Set `_isDeleting = false`, reset state

### 4. Permission Model

**Roles with Delete Permission** (from legacy):
- Role 24
- Role 17
- Role 5
- OR HedgeState is Draft (anyone can delete draft batches)

**Implementation**:
```csharp
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

### 5. File Download Mechanism

**JavaScript Interop** (in `_Host.cshtml`):
```javascript
window.downloadFileFromStream = async function (fileName, contentStreamReference) {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);

    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? 'download.xlsx';
    anchorElement.click();
    anchorElement.remove();

    URL.revokeObjectURL(url);
};
```

**C# Usage**:
```csharp
using var streamRef = new DotNetStreamReference(stream: result.ExcelStream);
await JSRuntime.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);
```

## Testing Checklist

### Manual Testing
- [ ] Download Excel for a test batch
  - Verify file downloads correctly
  - Verify filename is meaningful
  - Verify file content is correct Excel data
- [ ] Delete action permissions
  - [ ] Test with Draft hedge relationship (any user can delete)
  - [ ] Test with Designated hedge relationship (only roles 24, 17, 5 can delete)
  - [ ] Test with Dedesignated hedge relationship (only roles 24, 17, 5 can delete)
  - [ ] Verify Delete option is hidden for users without permission
- [ ] Delete confirmation
  - [ ] Verify confirmation modal appears
  - [ ] Test Cancel - verify no deletion occurs
  - [ ] Test Confirm - verify batch is deleted
  - [ ] Verify grid updates after deletion
- [ ] Error handling
  - [ ] Test download with invalid batch ID
  - [ ] Test delete with invalid batch ID
  - [ ] Verify error toasts appear
  - [ ] Verify application remains stable after errors

### Edge Cases
- [ ] Multiple rapid clicks on Download Excel
- [ ] Multiple rapid clicks on Delete
- [ ] Network timeout during download
- [ ] Network timeout during delete
- [ ] Large Excel files (performance)
- [ ] Deleting the only test batch
- [ ] Deleting the latest test batch

## Migration Notes

### Business Logic Preserved
1. **Exact Permission Model**: Draft state OR specific roles (24, 17, 5)
2. **Confirmation Required**: Always confirm before delete
3. **State Update**: Parent component receives updated hedge relationship after delete
4. **File Format**: Excel (.xlsx) format for downloads
5. **Batch ID Tracking**: `HedgeRegressionForExport` property set before export

### Differences from Legacy
1. **Confirmation UI**: Using Syncfusion modal instead of browser `confirm()`
2. **File Download**: Using Blazor JSRuntime instead of AngularJS $http
3. **State Management**: EventCallback pattern instead of scope binding
4. **Error Handling**: Structured try-catch with logging instead of promise chains

### API Compatibility
Both endpoints maintain exact compatibility with legacy implementation:
- Export uses same endpoint and parameters
- DeleteBatch uses same endpoint and returns same structure
- No new API contracts required

## Files Modified

1. **New Handlers**:
   - `Features/HedgeRelationships/Handlers/Queries/DownloadTestResultExcelService.cs`
   - `Features/HedgeRelationships/Handlers/Commands/DeleteTestBatchService.cs`

2. **Updated Components**:
   - `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/TestResultsTab.razor`
   - `Features/HedgeRelationships/Pages/HedgeRelationshipTabs/TestResultsTab.razor.cs`
   - `Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor`

## References

- **Legacy Template**: `old/hedgetestResultsView.cshtml` (lines 90-98)
- **Legacy Controller**: `old/hr_hedgeRelationshipAddEditCtrl.js` (lines 1553-1580)
- **Similar Handler Pattern**: `DownloadSpecsAndChecksService.cs`
- **Modal Pattern**: Used in `HedgeRelationshipRecords.razor`
- **Permission Pattern**: Used in `HedgeRelationshipDetails.razor.cs`
