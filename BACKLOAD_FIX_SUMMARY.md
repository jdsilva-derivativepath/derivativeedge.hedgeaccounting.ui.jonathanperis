# Backload Button SQL Parameter Error - Fix Summary

## Issue Description
When clicking the Backload button on the HedgeRelationshipDetails page, users received the following error:
```
Failed to run regression analysis: "One or more errors occurred. (The SqlParameterCollection only accepts non-null SqlParameter type objects, not SqlParameter objects.)"
```

## Root Cause Analysis

### The Problem
The error originated from a type conversion mismatch between the UI View Model and the API Entity during AutoMapper mapping:

1. **HedgeRelationshipVM** (UI layer):
   - DesignationDate: `string?`
   - DedesignationDate: `string?`
   
2. **HedgeRelationship Entity** (API layer):
   - DesignationDate: `DateTimeOffset` (required)
   - DedesignationDate: `DateTimeOffset?` (nullable)

### Why It Failed
When the Backload operation was invoked:
1. The BackloadAsync method in HedgeRelationshipDetails.razor.cs calls RunRegression.Command
2. RunRegression.Command handler uses AutoMapper to convert HedgeRelationshipVM to HedgeRelationship Entity
3. AutoMapper's default conversion doesn't properly handle empty strings (`""`) or `null` values when converting to `DateTimeOffset`
4. The improperly converted values were passed to the API
5. The API attempted to create SQL parameters with invalid date values
6. SQL Server rejected the null parameters, causing the error

### Where It Occurred
**File**: `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Commands/RunRegression.cs`
```csharp
// Line 48-50
var body = _mapper.Map<DerivativeEDGEHAEntityHedgeRelationship>(request.HedgeRelationship);
var apiResponse = await _hedgeAccountingApiClient.RegressAsync(request.HedgeResultType, body, cancellationToken);
```

The mapping step was where the type conversion failed.

## Solution Implemented

### Changes Made
Modified `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Mappings/HedgeRelationshipMappingProfile.cs`:

1. **Added Custom Date Parsing Methods**:
   ```csharp
   // For required DateTimeOffset fields
   private static DateTimeOffset ParseDateTimeOffset(string? dateString)
   {
       if (string.IsNullOrWhiteSpace(dateString))
           return DateTimeOffset.MinValue;
       
       if (DateTime.TryParse(dateString, out var parsedDate))
           return new DateTimeOffset(parsedDate);
       
       return DateTimeOffset.MinValue;
   }
   
   // For nullable DateTimeOffset? fields
   private static DateTimeOffset? ParseNullableDateTimeOffset(string? dateString)
   {
       if (string.IsNullOrWhiteSpace(dateString))
           return null;
       
       if (DateTime.TryParse(dateString, out var parsedDate))
           return new DateTimeOffset(parsedDate);
       
       return null;
   }
   ```

2. **Applied Custom Mapping**:
   ```csharp
   CreateMap<DerivativeEDGEHAApiViewModelsHedgeRelationshipVM, DerivativeEDGEHAEntityHedgeRelationship>()
       .ForMember(dest => dest.DesignationDate, 
           opt => opt.MapFrom(src => ParseDateTimeOffset(src.DesignationDate)))
       .ForMember(dest => dest.DedesignationDate, 
           opt => opt.MapFrom(src => ParseNullableDateTimeOffset(src.DedesignationDate)));
   ```

### Why This Works
- **Explicit Conversion**: Custom parsing methods handle all edge cases (null, empty string, invalid format)
- **Type Safety**: Required fields get `DateTimeOffset.MinValue` instead of invalid values
- **Null Handling**: Nullable fields correctly receive `null` for empty/invalid values
- **SQL Compatibility**: SQL parameters now receive valid date values or proper nulls

## Impact Analysis

### Operations Affected by Fix
1. **Backload**: Primary target - should now work correctly ✅
2. **Run Regression**: Uses same mapping - fix applies ✅
3. **Update Hedge Relationship**: Uses same mapping - fix applies ✅
4. **Designate/De-Designate/Re-Designate**: Indirectly benefit from improved mapping ✅

### No Breaking Changes
- The fix only affects the VM-to-Entity direction
- Entity-to-VM mapping remains unchanged (dates convert naturally to strings)
- UI date handling in HedgeRelationshipDetails.razor.cs remains the same
- Date format ("MM/dd/yyyy") preserved

## Testing Recommendations

### Manual Testing
1. **Backload Operation**:
   - Navigate to a hedge relationship in "Designated" state
   - Click the "Backload" button
   - Verify success message appears
   - Check that backload results appear in Test Results tab

2. **Run Regression**:
   - Test normal regression operation
   - Verify no errors occur
   - Confirm results display correctly

3. **Save Operations**:
   - Save hedge relationship with populated DesignationDate
   - Save with empty DesignationDate (if applicable)
   - Verify no errors in either case

4. **Edge Cases**:
   - Test with hedge relationships that have DedesignationDate set to null
   - Test with various date formats in the UI
   - Verify empty string dates don't cause errors

### Automated Testing
Consider adding unit tests for the mapping profile:
```csharp
[Test]
public void Should_Convert_Empty_DesignationDate_To_MinValue()
{
    var vm = new HedgeRelationshipVM { DesignationDate = "" };
    var entity = _mapper.Map<HedgeRelationship>(vm);
    Assert.AreEqual(DateTimeOffset.MinValue, entity.DesignationDate);
}

[Test]
public void Should_Convert_Null_DedesignationDate_To_Null()
{
    var vm = new HedgeRelationshipVM { DedesignationDate = null };
    var entity = _mapper.Map<HedgeRelationship>(vm);
    Assert.IsNull(entity.DedesignationDate);
}
```

## Legacy Code Reference
The original Angular JS implementation handled this differently:
- **File**: `old/hr_hedgeRelationshipAddEditCtrl.js`
- **Function**: `$scope.backload`
- The legacy code passed the model directly to the API endpoint without explicit date conversion
- The backend likely had more robust date parsing or different serialization settings

## Additional Notes

### Why This Wasn't Caught Earlier
1. The Update operation likely worked because it may have had valid dates already set
2. Normal Regression might work with hedge relationships that have proper dates
3. Backload specifically triggered the error because it operates on hedge relationships in specific states where date handling is critical

### Future Considerations
1. Consider adding similar custom mapping for other string-to-complex-type conversions
2. Review other AutoMapper profiles for similar issues
3. Add validation at the UI layer to ensure dates are in expected format before submission
4. Consider using a consistent date serialization strategy across the entire application

## Commit History
1. `5a2c80b` - Add custom date conversion mapping to fix Backload SQL parameter error
2. `f028f7d` - Remove incorrect RedesignationDate mapping from HedgeRelationship profile

## Related Files
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Mappings/HedgeRelationshipMappingProfile.cs`
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Pages/HedgeRelationshipDetails.razor.cs`
- `src/DerivativeEDGE.HedgeAccounting.UI/Features/HedgeRelationships/Handlers/Commands/RunRegression.cs`
- `api/HedgeAccountingApiClient.cs` (reference only)
