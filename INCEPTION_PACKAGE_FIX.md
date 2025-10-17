# Inception Package Zip File Corruption Fix

## Issue Description
When clicking on the "Preview Inception Package" button on the HedgeRelationshipDetails page, the zip file was being downloaded but was corrupt and could not be opened.

## Root Cause
The issue was in the `HedgeRelationshipDetails.razor.cs` file where binary file downloads (zip files) were being handled incorrectly:

1. The stream was being converted to a byte array
2. The byte array was then base64 encoded
3. The base64 string was passed to a JavaScript `downloadFile` function that uses `data:` URLs
4. This approach corrupts binary files, especially zip archives, due to:
   - Unnecessary memory copying
   - Base64 encoding overhead
   - Size limitations and potential issues with `data:` URLs for large files

## Solution
Replaced the flawed base64 download approach with `DotNetStreamReference` and the `downloadFileFromStream` JavaScript function, which is the correct pattern for binary file downloads in Blazor.

### Changes Made

#### File: `HedgeRelationshipDetails.razor.cs`

**Before (Incorrect Approach):**
```csharp
private async Task GenerateInceptionPackageAsync()
{
    // ... validation code ...
    
    var query = new InceptionPackageService.Query(HedgeRelationship);
    var result = await Mediator.Send(query);

    // Convert stream to byte array for download
    using var memoryStream = new MemoryStream();
    await result.ExcelStream.CopyToAsync(memoryStream);
    var fileBytes = memoryStream.ToArray();

    // Trigger file download
    await JSRuntime.InvokeVoidAsync("downloadFile", result.FileName, Convert.ToBase64String(fileBytes));
    
    // ... success handling ...
}
```

**After (Correct Approach):**
```csharp
private async Task GenerateInceptionPackageAsync()
{
    // ... validation code ...
    
    var query = new InceptionPackageService.Query(HedgeRelationship);
    var result = await Mediator.Send(query);

    // Use DotNetStreamReference for proper binary file download
    using var streamRef = new DotNetStreamReference(stream: result.ExcelStream);
    await JSRuntime.InvokeVoidAsync("downloadFileFromStream", result.FileName, streamRef);
    
    // ... success handling ...
}
```

The same fix was applied to `DownloadSpecsAndChecksAsync()` which had the same issue.

## Technical Details

### Why This Works
1. **DotNetStreamReference**: A special wrapper that allows .NET streams to be passed to JavaScript efficiently
2. **downloadFileFromStream**: A JavaScript function already implemented in `_Host.cshtml` that properly handles binary streams:
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
3. **Blob URLs**: Uses proper Blob URLs instead of `data:` URLs, which handle large binary files correctly

### Benefits
- ✅ Zip files are no longer corrupted
- ✅ Eliminates unnecessary memory copying
- ✅ Removes base64 encoding overhead
- ✅ Handles large files efficiently
- ✅ Consistent with the pattern already used successfully in `HedgeRelationshipRecords.razor.cs`

## Verification
To verify the fix:
1. Navigate to a Hedge Relationship details page
2. Click the "Preview Inception Package" button
3. The zip file should download successfully
4. Open the downloaded zip file - it should extract without errors

## Related Files
- `/src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs` - Fixed file
- `/src/DerivativeEDGE.HedgeAccounting.UI/Pages/_Host.cshtml` - Contains the JavaScript download functions
- `/src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipRecords.razor.cs` - Reference implementation (correct pattern)
- `/src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Queries/InceptionPackageService.cs` - Handler that returns the stream
- `/src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Queries/DownloadSpecsAndChecksService.cs` - Handler that returns the stream

## Notes
- The fix is a "lift and shift" - it maintains the same functionality but uses the correct download mechanism
- No changes were made to the API or business logic
- The `downloadFile` function with base64 is still available in `_Host.cshtml` but should not be used for binary files
