# Hedge Relationship Migration - Business Rules Analysis

## Executive Summary

This document provides a comprehensive analysis comparing the legacy Angular JS-based Hedge Relationship system with the new Blazor Server implementation. The analysis focuses on identifying business rules, validation logic, workflow behaviors, and feature parity to ensure a successful lift-and-shift migration.

**Analysis Date**: October 17, 2025  
**Scope**: Hedge Relationship Details Page and All Related Tabs  
**Status**: In Progress - Detailed Analysis Phase

---

## Table of Contents

1. [Overview](#overview)
2. [File Mapping](#file-mapping)
3. [Main Page & Workflow Analysis](#main-page--workflow-analysis)
4. [Initial View / Setup Section](#initial-view--setup-section)
5. [Details View / Edit Section](#details-view--edit-section)
6. [Accounting Details Section](#accounting-details-section)
7. [Instrument Analysis Tab](#instrument-analysis-tab)
8. [Test Results Tab](#test-results-tab)
9. [Amortization Tab](#amortization-tab)
10. [Option Amortization Tab](#option-amortization-tab)
11. [History Tab](#history-tab)
12. [Logs Tab](#logs-tab)
13. [Workflow Actions Analysis](#workflow-actions-analysis)
14. [Business Rules Summary](#business-rules-summary)
15. [Gap Analysis](#gap-analysis)
16. [Action Items](#action-items)

---

## Overview

### Legacy System Architecture
- **Framework**: ASP.NET Framework with Angular JS
- **Main Controller**: `hr_hedgeRelationshipAddEditCtrl.js` (3,513 lines)
- **Main View**: `HedgeRelationship.cshtml`
- **Partial Views**: Multiple `.cshtml` files for different sections
- **State Management**: Angular JS scope-based
- **API Communication**: Custom Angular service (`$haService`)

### New System Architecture
- **Framework**: Blazor Server (.NET)
- **Main Component**: `HedgeRelationshipDetails.razor` / `.razor.cs` (682 + 1,294 lines)
- **Tab Components**: Separate `.razor` files for each tab
- **State Management**: Component-based with parameters
- **API Communication**: MediatR pattern with CQRS

---

## File Mapping

| Legacy File | New File | Status |
|-------------|----------|--------|
| `HedgeRelationship.cshtml` | `HedgeRelationshipDetails.razor` | ✅ Implemented |
| `initialView.cshtml` | `HedgeRelationshipDetails.razor` (Initial Section) | ✅ Implemented |
| `detailsView.cshtml` | `HedgeRelationshipDetails.razor` (Details Section) | ✅ Implemented |
| `accountingView.cshtml` | `AccountingDetailsTab.razor` | ✅ Implemented |
| `instrumentsAnalysisView.cshtml` | `InstrumentAnalysisTab.razor` | ✅ Implemented |
| `hedgetestResultsView.cshtml` | `TestResultsTab.razor` | ✅ Implemented |
| `amortizationView.cshtml` | `AmortizationTab.razor` | ✅ Implemented |
| `optionTimeValue.cshtml` | `OptionAmortizationTab.razor` | ✅ Implemented |
| `historyView.cshtml` | `HedgeRelationshipHistoryTab.razor` | ✅ Implemented |
| `details.cshtml` (logs) | `HedgeRelationshipLogsTab.razor` | ✅ Implemented |
| `hr_hedgeRelationshipAddEditCtrl.js` | `HedgeRelationshipDetails.razor.cs` | ⚠️ Needs Review |

---

## Main Page & Workflow Analysis

### Legacy Implementation (HedgeRelationship.cshtml + hr_hedgeRelationshipAddEditCtrl.js)

#### Key Features:
1. **Breadcrumb Navigation** (Lines 29-55 in .cshtml)
   - Displays trail based on `Model.BreadcrumbTrail`
   - Dynamic link generation with custom HTML attributes

2. **Workflow Dropdown** (Lines 58-72 in .cshtml)
   - Status displayed: `{{Model.HedgeStateText}}`
   - Action dropdown: `{{onActionChangeValue}}`
   - Actions: Designate, De-Designate, Re-Designate, Redraft
   - Disabled when `InProgress` is true

3. **User Message Display** (Lines 74-83 in .cshtml)
   - Success/error message shown conditionally
   - Icon and message text display
   - Can be dismissed by user

4. **Tab Structure**
   - Uses partial views rendered via `Html.RenderPartial`
   - Initial view shown when `!openDetailsTab`
   - Details view shown when `openDetailsTab`

#### Business Rules in Legacy Controller:


**1. Workflow Action Availability (Lines 458-476)**
```javascript
setWorkFlow = function () {
    // Draft state: Remove "De-Designate" option
    if ($scope.Model.HedgeState === 'Draft') {
        $scope.DropDownList.ActionList.splice(1, 1);
    }
    
    // Show "Re-Designate" only for Designated CashFlow hedges
    if ($scope.Model.HedgeState !== 'Designated' || $scope.Model.HedgeType !== "CashFlow") {
        $scope.DropDownList.ActionList.splice(2, 1);
    }
    
    // Designated or Dedesignated: Replace "Designate" with "Redraft"
    if ($scope.Model.HedgeState === 'Designated' || $scope.Model.HedgeState === "Dedesignated") {
        $scope.DropDownList.ActionList.splice(0, 1);
        $scope.DropDownList.ActionList.splice(0, 0, { "Value": "Redraft", "Disabled": false });
    }
    
    // Disable actions based on user roles (24, 17, 5)
    $scope.DropDownList.ActionList.map(function (v) {
        v.Disabled = !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'));
    });
};
```

**Business Rule**: Workflow actions are conditional based on:
- Hedge State (Draft, Designated, Dedesignated)
- Hedge Type (CashFlow for Re-Designate)
- User Roles (24, 17, 5 required for actions)

**2. Button Disable Logic**

**Save Button** (Lines 3110-3112):
```javascript
$scope.disableSave = function () {
    return $scope.InProgress || 
           (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) 
            && $scope.Model.HedgeState !== 'Draft');
};
```
**Business Rule**: Save is disabled when:
- Operation is in progress, OR
- User doesn't have roles 24/17/5 AND hedge state is not Draft

**Preview Inception Package** (Lines 3114-3119):
```javascript
$scope.disablePrevInceptionPackage = function () {
    return $scope.InProgress ||
           $scope.Model.LatestHedgeRegressionBatch === null ||
           $scope.Model.LatestHedgeRegressionBatch === undefined ||
           (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) 
            && $scope.Model.HedgeState !== 'Draft');
};
```
**Business Rule**: Preview is disabled when:
- Operation is in progress, OR
- No regression batch exists, OR
- User lacks required roles and hedge is not Draft

**Run Regression** (Lines 3121-3125):
```javascript
$scope.disableRunRegression = function () {
    return $scope.InProgress ||
           ($scope.Model.Benchmark === 'None' && $scope.Model.HedgeRiskType !== 'ForeignExchange') ||
           (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) 
            && $scope.Model.HedgeState !== 'Draft');
};
```
**Business Rule**: Run Regression is disabled when:
- Operation is in progress, OR
- Benchmark is 'None' AND hedge risk is not Foreign Exchange, OR
- User lacks required roles and hedge is not Draft

**Backload** (Lines 3127-3131):
```javascript
$scope.disableBackload = function () {
    return $scope.InProgress ||
           !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) ||
           $scope.Model.HedgeState == 'Draft';
};
```
**Business Rule**: Backload is disabled when:
- Operation is in progress, OR
- User lacks required roles (24/17/5), OR
- Hedge state is Draft

**3. Benchmark Filtering (Lines 35-58)**
```javascript
function setDropDownListBenchmark() {
    var benchmarks = [];
    if ($scope.Model.HedgeType === "CashFlow") {
        var notCFBenchmarks = ["FFUTFDTR", "FHLBTopeka", "USDTBILL4WH15"];
        // Filter out these benchmarks for CashFlow
    } else {
        var notFVBenchmarks = ["FFUTFDTR", "FHLBTopeka", "USDTBILL4WH15", "Other", "Prime"];
        // Filter out these benchmarks for FairValue
    }
}
```
**Business Rule**: Benchmark options are filtered based on Hedge Type:
- CashFlow: Excludes FFUTFDTR, FHLBTopeka, USDTBILL4WH15
- FairValue: Excludes FFUTFDTR, FHLBTopeka, USDTBILL4WH15, Other, Prime

### New Implementation (HedgeRelationshipDetails.razor/.cs)

#### Key Features:
1. **Breadcrumb Navigation** (Lines 42-64 in .razor)
   - Simplified breadcrumb structure
   - Home → Hedge Relationships → HR ID

2. **Workflow Dropdown** (Lines 68-76 in .razor)
   - Uses SfDropDownButton component
   - Status button shows `HedgeRelationship.HedgeStateText`
   - Workflow dropdown with `WorkflowItems` collection
   - Calls `HandleWorkflowAction` method

3. **Action Buttons** (Lines 79-103 in .razor)
   - Save button with loading state
   - Cancel button
   - Preview Inception Package button with loading state
   - Run Regression button
   - Backload button

4. **Validation Messages** (Lines 23-36 in .razor)
   - SfMessage component for errors
   - List of validation errors displayed

#### Business Rules in New Implementation:

**1. Workflow Action Handler** (Lines 838-857 in .razor.cs):
```csharp
private async Task HandleWorkflowAction(MenuEventArgs args)
{
    var selected = args.Item.Text;
    switch (selected)
    {
        case "Designate":
            await HandleDesignateAsync();
            break;
        case "De-Designate":
            await HandleDeDesignateAsync();
            break;
        case "Redraft":
            await HandleRedraftAsync();
            break;
        case "Re-Designate":
            await HandleReDesignateAsync();
            break;
    }
}
```

**2. Button Disable Logic**:

**Save Button** (Method: `IsSaveDisabled()`):
```csharp
// Implementation needs to be checked
```

**Preview Inception Package** (Method: `IsPreviewInceptionPackageDisabled()`):
```csharp
// Implementation needs to be checked
```

### Comparison & Gap Analysis

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Breadcrumb Navigation | ✅ Dynamic from Model | ✅ Simplified static | ⚠️ Needs verification |
| Workflow Dropdown | ✅ Conditional actions | ✅ Implemented | ⚠️ Logic needs verification |
| User Messages | ✅ Angular binding | ✅ SfMessage component | ⚠️ Dismiss functionality? |
| Save Disable Logic | ✅ Role + State based | ❓ Needs verification | 🔍 TO VERIFY |
| Preview Disable Logic | ✅ Regression + Role based | ❓ Needs verification | 🔍 TO VERIFY |
| Regression Disable Logic | ✅ Benchmark + Role based | ❓ Needs verification | 🔍 TO VERIFY |
| Backload Disable Logic | ✅ State + Role based | ❓ Needs verification | 🔍 TO VERIFY |
| Workflow Action Rules | ✅ State + Type based | ❓ Needs verification | 🔍 TO VERIFY |
| Benchmark Filtering | ✅ Type-based filtering | ❓ Needs verification | 🔍 TO VERIFY |

---

## Initial View / Setup Section

### Legacy Implementation (initialView.cshtml)

#### Key Features:
1. **Continue/Cancel Buttons** (Lines 2-7)
   - Continue button: `data-ng-click="continue();"`
   - Cancel button: `data-ng-click="cancel();"` with `InProgress` disable

2. **Relationship Information Section** (Lines 11-133)
   - Client selection (DPI users only via directive)
   - Bank Entity selection
   - Description text input
   - Hedged Risk Type dropdown (required)
   - Hedge Direction dropdown (hidden)
   - Hedge Type dropdown (required)
   - Fair Value Method (shown when HedgeType === 'FairValue')
   - Designation Date (required)
   - Dedesignation Date
   - Benchmark dropdown (shown when HedgeRiskType === 'InterestRate')
   - Hedge Exposure dropdown (FX + non-NetInvestment)
   - Exposure Currency (FX + NetInvestment)
   - Hedged Item Type dropdown
   - Hedged Item Type Description textarea (when Type === 'SHOW')
   - Shortcut checkbox (FairValue only)
   - Asset/Liability dropdown
   - Accounting Treatment (NetInvestment only)
   - Available For Sale checkbox (FairValue + Asset)

3. **Hedging Objective Section** (Lines 135-166)
   - Template dropdown for inception memo
   - Objective textarea
   - Rich text editor support

4. **Accounting Section** (Lines 168-175)
   - Rendered via partial view `accountingView.cshtml`

#### Business Rules:

**1. Hedge Type Filtering** (Lines 308-326 in controller):
```javascript
function setDropDownListHedgeType() {
    var hedgetype = [];
    if ($scope.Model.HedgeRiskType === 'InterestRate') {
        $scope.DropDownList.HedgeTypeList.map(function (v) {
            if (v.Value !== 'NetInvestment') {
                hedgetype.push(v);
            }
        });
        $scope.DropDownList.HedgeTypeList = hedgetype;
    } else {
        $scope.DropDownList.HedgeTypeList = $scope.enums['HRHedgeType'];
    }
}
```
**Business Rule**: NetInvestment hedge type is NOT available for Interest Rate risks.

**2. Benchmark/Exposure/Treatment Setting** (Lines 271-295 in controller):
```javascript
function setBenchmarkContractualRateExposure() {
    if (($scope.Model.HedgeRiskType === 'ForeignExchange') && ($scope.Model.HedgeType === 'CashFlow')) {
        $scope.Model.Benchmark = '0';
        $scope.Model.ExposureCurrency = null;
        $scope.Model.HedgeAccountingTreatment = '0';
    }
    else if (($scope.Model.HedgeRiskType === 'ForeignExchange') && ($scope.Model.HedgeType === 'FairValue')) {
        $scope.Model.Benchmark = '0';
        $scope.Model.ExposureCurrency = null;
        $scope.Model.HedgeAccountingTreatment = '0';
    }
    else if (($scope.Model.HedgeRiskType === 'ForeignExchange') && ($scope.Model.HedgeType === 'NetInvestment')) {
        $scope.Model.Benchmark = '0';
        $scope.Model.HedgeExposure = '0';
    }
    else if ($scope.Model.HedgeRiskType === 'InterestRate') {
        $scope.Model.HedgeExposure = '0';
        $scope.Model.ExposureCurrency = null;
        $scope.Model.HedgeAccountingTreatment = '0';
    }
}
```
**Business Rule**: Field auto-reset based on HedgeRiskType and HedgeType combinations.

**3. Benchmark Label** (Lines 254-268 in controller):
```javascript
function setBenchmarkLabel() {
    if (($scope.Model.HedgeRiskType === 'InterestRate') && ($scope.Model.HedgeType === 'CashFlow')) {
        $scope.Model.BenchMarkLabel = 'Contractual Rate';
    }
    else if (($scope.Model.HedgeRiskType === 'InterestRate') && ($scope.Model.HedgeType === 'FairValue')) {
        $scope.Model.BenchMarkLabel = 'Benchmark';
    }
    else {
        $scope.Model.BenchMarkLabel = 'Benchmark';
    }
}
```
**Business Rule**: Label changes to "Contractual Rate" for Interest Rate + Cash Flow hedges.

### New Implementation (HedgeRelationshipDetails.razor - Initial Section)

#### Key Features:
- Initial section appears to be inline in the main Details.razor file
- Uses Blazor EditForm with DataAnnotationsValidator
- Fields organized in grid layout

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Client Selection | ✅ DPI directive | ❓ | 🔍 TO VERIFY |
| Entity Selection | ✅ Dynamic load | ❓ | 🔍 TO VERIFY |
| Hedge Type Filtering | ✅ By Risk Type | ❓ | 🔍 TO VERIFY |
| Benchmark Filtering | ✅ By Hedge Type | ❓ | 🔍 TO VERIFY |
| Field Auto-Reset | ✅ Multiple rules | ❓ | 🔍 TO VERIFY |
| Benchmark Label Change | ✅ Contractual Rate | ❓ | 🔍 TO VERIFY |
| Conditional Field Display | ✅ Many conditions | ❓ | 🔍 TO VERIFY |
| Rich Text Editor | ✅ Objective field | ❓ | 🔍 TO VERIFY |

---

## Details View / Edit Section

### Legacy Implementation (detailsView.cshtml)

#### Key Features:
1. **Action Bar** (Lines 2-17)
   - Save button: Disabled via `disableSave()`
   - Cancel button: Disabled when `InProgress`
   - Preview Inception Package: Disabled via `disablePrevInceptionPackage()`
   - Run Regression: Disabled via `disableRunRegression()`
   - Backload: Disabled via `disableBackload()`
   - Curve Date input with datepicker
   - Download Specs & Checks button (conditional display)

2. **Edit Information Section** (Lines 29-130)
   - Toggle between view and edit modes via `editInfoValue`
   - Client dropdown (DPI users)
   - Entity dropdown
   - Description input
   - Hedging Objective section with template dropdown
   - Display modes differ based on `DraftDesignatedIsDPIUser`

3. **Permission-Based Display** (Lines 598-600 in controller):
```javascript
$scope.DraftDesignatedIsDPIUser = $scope.Model.HedgeState === 'Draft' || 
    ($scope.Model.HedgeState === 'Designated' && 
     ($scope.IsDPIUser || $scope.Model.ContractType === 'SaaS' || $scope.Model.ContractType === 'SwaS'));
$scope.DraftIsDPIUser = $scope.Model.HedgeState === 'Draft' && $scope.IsDPIUser;
$scope.DesignatedIsDPIUser = $scope.Model.HedgeState === 'Designated' && 
    !($scope.IsDPIUser || $scope.Model.ContractType === 'SaaS' || $scope.Model.ContractType === 'SwaS');
```

**Business Rules:**
- `DraftDesignatedIsDPIUser`: True if Draft OR (Designated AND (DPI user OR SaaS/SwaS contract))
- `DraftIsDPIUser`: True if Draft AND DPI user
- `DesignatedIsDPIUser`: True if Designated AND NOT (DPI user OR SaaS/SwaS)

These affect field editability and visibility throughout the UI.

### New Implementation

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Edit Mode Toggle | ✅ editInfoValue | ❓ | 🔍 TO VERIFY |
| DPI User Permissions | ✅ Complex logic | ❓ | 🔍 TO VERIFY |
| Contract Type Logic | ✅ SaaS/SwaS | ❓ | 🔍 TO VERIFY |
| Field Disable Logic | ✅ Per permission | ❓ | 🔍 TO VERIFY |
| Curve Date Input | ✅ With validation | ❓ | 🔍 TO VERIFY |

---

## Accounting Details Section

### Legacy Implementation (accountingView.cshtml)

#### Key Features:
1. **Standard Selection** (Lines 5-9)
   - Dropdown for accounting standard
   - Disabled when `Model.HedgeState !== 'Draft'`

2. **Tax Purposes Checkbox** (Lines 11-16)
   - "Hedge for Tax Purposes"

3. **Option-Related Fields** (Lines 18-57, only when `!openDetailsTab`)
   - "Hedge is an Option" checkbox
   - Hedging Instrument Structure dropdown
   - Delta Match Option checkbox (when IsAnOptionHedge)
   - Amortize Option Premium checkbox (when IsAnOptionHedge)
   - Amortization Method dropdown (when IsAnOptionHedge)
   - Option Premium input (when IsAnOptionHedge)

4. **FX After Tax Basis** (Lines 58-64, hidden)
   - Hidden checkbox

#### Business Rules from Controller:

**1. IsAnOptionHedge Watch** (Lines 424-435):
```javascript
$scope.$watch('Model.IsAnOptionHedge', function (new_) {
    if (new_ !== undefined) {
        if (!new_) {
            $scope.Model.AmortizeOptionPremimum = false;
            $scope.Model.IsDeltaMatchOption = false;
            $scope.Model.ExcludeIntrinsicValue = false;
        }
        if (!$scope.Model.OffMarket) {
            $scope.Model.OffMarket = false;
        }
    }
});
```
**Business Rule**: When option hedge is unchecked, reset related fields to false.

**2. ExcludeIntrinsicValue Watch** (Lines 437-445):
```javascript
$scope.$watch('Model.ExcludeIntrinsicValue', function (new_) {
    if (new_ !== undefined) {
        if (!new_) {
            $scope.Model.IntrinsicMethod = "None";
            $scope.Model.AmortizeOptionPremimum = false;
            $scope.Model.IsDeltaMatchOption = false;
        }
    }
});
```
**Business Rule**: When exclude intrinsic value is unchecked, reset method and related checkboxes.

**3. Hedging Instrument Structure Filtering** (Lines 407-422):
```javascript
function setDropDownListHedgingInstrumentStructure() {
    if (!$scope.Model.HedgingInstrumentStructureText) {
        $scope.Model.HedgingInstrumentStructureText = 'Single Instrument';
        $scope.Model.HedgingInstrumentStructure = "SingleInstrument";
    }
    
    if ($scope.IsDPIUser) {
        return;
    }
    
    $scope.DropDownList.HedgingInstrumentStructureList = 
        $scope.DropDownList.HedgingInstrumentStructureList
            .filter(function (e) { return e.Value === 'SingleInstrument' });
};
```
**Business Rule**: Non-DPI users can only select "Single Instrument" structure.

**4. Amortization Method Filtering** (Lines 395-405):
```javascript
function setDropDownListAmortizationMethod() {
    var amortization = [];
    $scope.DropDownList.AmortizationMethodList.map(function (v) {
        if (v.Value != 'IntrinsicValueMethod') {
            amortization.push(v);
        }
    });
    $scope.DropDownList.AmortizationMethodList = amortization;
};
```
**Business Rule**: "IntrinsicValueMethod" is filtered out from amortization methods.

**5. Hedge State Change** (Lines 447-455):
```javascript
$scope.$watch('Model.HedgeState', function (new_, old_) {
    if (new_ !== undefined) {
        setWorkFlow();
        
        if ($scope.Model.HedgeState === "Dedesignated") {
            $scope.Model.IsAnOptionHedge = false;
        }
    }
});
```
**Business Rule**: When hedge is Dedesignated, IsAnOptionHedge is forced to false.

### New Implementation (AccountingDetailsTab.razor)

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Standard Dropdown | ✅ Disabled in non-Draft | ❓ | 🔍 TO VERIFY |
| Tax Purposes Checkbox | ✅ | ❓ | 🔍 TO VERIFY |
| Option Hedge Controls | ✅ Complex visibility | ❓ | 🔍 TO VERIFY |
| IsAnOptionHedge Watch | ✅ Resets related fields | ❓ | 🔍 TO VERIFY |
| Instrument Structure Filter | ✅ Non-DPI limited | ❓ | 🔍 TO VERIFY |
| Amortization Method Filter | ✅ No Intrinsic | ❓ | 🔍 TO VERIFY |
| Dedesignated State Rules | ✅ Force option off | ❓ | 🔍 TO VERIFY |

---

## Instrument Analysis Tab

### Legacy Implementation (instrumentsAnalysisView.cshtml)

#### Key Features:
1. **Hedged Items Section** (Lines 1-48)
   - Grid display of hedged items (Syncfusion ejGrid)
   - "Select existing trade" button
   - "New trade" dropdown with multiple instrument types:
     - Callable Debt
     - Cancelable
     - Cap Floor
     - Collar
     - Debt
     - Debt Option
     - Swap
     - Swap With Cap/Floor
     - Swaption
     - Corridor
     - FX Forward

2. **Hedging Items Section** (Lines 51-99)
   - Similar grid and controls as Hedged Items
   - Same trade type options

3. **Effectiveness Settings** (Lines 101-201)
   - Prospective Assessment Method dropdown
   - Retrospective Assessment Method dropdown
   - Ineffectiveness Measurement (hidden)
   - Report Currency dropdown
   - Report Frequency dropdown
   - **Regression Settings**:
     - Period Count input
     - Period Size dropdown
     - Regression type: Cumulative Changes vs Periodic Changes (radio)
     - End Of Month checkbox (enabled only if Period Size is Month)

#### Business Rules:

**1. Hide Select/Remove Trade** (Lines 3133-3143 in controller):
```javascript
$scope.hideSelectNewOrRemoveTrade = function () {
    if ($scope.Model === undefined) {
        setTimeout(function () { $scope.hideSelectNewOrRemoveTrade(); }, 1000);
        return false;
    }
    else {
        return $scope.DesignatedIsDPIUser || 
               (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) 
                && $scope.Model.HedgeState !== 'Draft');
    }
};
```
**Business Rule**: Trade selection buttons hidden when:
- User is Designated DPI user, OR
- User lacks roles 24/17/5 AND hedge is not Draft

**2. EOM Checkbox Behavior** (Line 189):
```html
<i class="fa fa-square-o" data-type="Model.EOM" data-ng-hide="Model.EOM" 
   data-ng-click="!IsPeriodSizeMonth() || checkboxClickEvent($event);"></i>
```
**Business Rule**: EOM checkbox only clickable when Period Size is Month.

**3. Effectiveness Methods Filtering** (Lines 149-181 in controller):
```javascript
function setDropDownListEffectivenessMethods() {
    if ($scope.Model.HedgeType === 'FairValue') {
        $scope.Model.FairValueMethod = 'None';
    }
    
    $scope.DropDownList.EffectivenessMethods = [];
    $scope.EffectivenessMethods.map(function (v) {
        if ((($scope.Model.HedgeType === 'FairValue' && v.IsForFairValue) ||
             ($scope.Model.HedgeType === 'CashFlow' && v.IsForCashFlow) ||
             ($scope.Model.HedgeType === 'NetInvestment' && v.IsForNetInvestment)))
        {
            if ($scope.Model.IsAnOptionHedge) {
                if (v.IsForOption) {
                    $scope.DropDownList.EffectivenessMethods.push(v);
                }
            } else {
                $scope.DropDownList.EffectivenessMethods.push(v);
            }
        }
    });
    
    // Filter out 'Regression - Change in Intrinsic Value' if not option hedge
    if (!$scope.Model.IsAnOptionHedge && v.Name === 'Regression - Change in Intrinsic Value') {
        return;
    }
}
```
**Business Rule**: Effectiveness methods filtered by:
- Hedge Type (FairValue, CashFlow, NetInvestment)
- Whether it's an option hedge
- Specific exclusions for non-option hedges

### New Implementation (InstrumentAnalysisTab.razor)

#### Key Features:
- Hedged Items grid with action buttons
- Hedging Items grid with action buttons
- Similar dropdown for new trade types
- Select existing trade buttons

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Hedged Items Grid | ✅ Syncfusion | ✅ Custom Grid | ✅ IMPLEMENTED |
| Hedging Items Grid | ✅ Syncfusion | ✅ Custom Grid | ✅ IMPLEMENTED |
| New Trade Dropdown | ✅ All types | ✅ | ⚠️ Verify all types |
| Effectiveness Section | ✅ Full section | ❓ | 🔍 TO VERIFY |
| Prospective Method | ✅ Filtered dropdown | ❓ | 🔍 TO VERIFY |
| Retrospective Method | ✅ Filtered dropdown | ❓ | 🔍 TO VERIFY |
| Regression Settings | ✅ All fields | ❓ | 🔍 TO VERIFY |
| EOM Checkbox Logic | ✅ Month-dependent | ❓ | 🔍 TO VERIFY |
| Method Filtering | ✅ Complex logic | ❓ | 🔍 TO VERIFY |
| Hide Trade Buttons | ✅ Permission-based | ❓ | 🔍 TO VERIFY |

---

## Test Results Tab

### Legacy Implementation (hedgetestResultsView.cshtml)

#### Key Features:
1. **Most Recent Test Results Section** (Lines 1-76)
   - Statistical metrics displayed in three columns:
     - **Column 1**: Slope, R, R²
     - **Column 2**: Std Error, Obs, y-intercept
     - **Column 3**: t-Test, Significance, F-Stat
   - **Optional Column 4** (shown when `Model.CumulativeChanges`):
     - d (Durbin-Watson stat)
     - Positive Autocorrelation
     - Negative Autocorrelation
   - Regression chart container
   - Results table

2. **All Tests Section** (Lines 78-87)
   - Grid showing all historical test runs
   - Action dropdown per row:
     - "Download Excel"
     - "Delete" (conditional)

#### Business Rules:

**1. Delete Action Visibility** (Line 96):
```html
<option value="Delete" data-ng-show="Model.HedgeState === 'Draft' || 
        checkUserRole('24') || checkUserRole('17') || checkUserRole('5')">Delete</option>
```
**Business Rule**: Delete test option available when:
- Hedge state is Draft, OR
- User has role 24, 17, or 5

**2. Autocorrelation Display** (Lines 52-65):
```html
<div class="col-xs-4 testresultsSlope" data-ng-show="Model.CumulativeChanges">
```
**Business Rule**: Autocorrelation stats (d, positive/negative) shown only for Cumulative Changes regression.

### New Implementation (TestResultsTab.razor)

#### Key Features:
- Statistics display in custom layout
- Regression chart using SfChart
- Trendline visualization

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Statistics Display | ✅ 3-4 columns | ✅ Custom layout | ⚠️ Verify all stats |
| Autocorrelation Stats | ✅ Conditional | ❓ | 🔍 TO VERIFY |
| Regression Chart | ✅ Highcharts | ✅ Syncfusion | ⚠️ Verify parity |
| All Tests Grid | ✅ With actions | ❓ | 🔍 TO VERIFY |
| Delete Action Logic | ✅ State + Role | ❓ | 🔍 TO VERIFY |
| Download Excel | ✅ | ❓ | 🔍 TO VERIFY |

---

## Amortization Tab

### Legacy Implementation (amortizationView.cshtml)

#### Key Features:
1. **Amortization Schedules Grid** (Lines 16-28)
   - Display of schedules

2. **Amortization Values Grid** (Lines 30-34)
   - Display of amortization entries

3. **Amortization Dialog Template** (Lines 39-135)
   - **GL Account dropdown** (required, validated)
   - **Contra Account dropdown** (required, validated)
   - **Financial Centers** multi-select listbox
   - **Payment Frequency dropdown**
   - **Day Count Convention dropdown** (continued...)

#### Business Rules:

**1. GL Account Validation** (Lines 42-57):
```html
<select data-ng-model="HedgeRelationshipOptionTimeValueAmort.GLAccountID"
        name="GLAccountID"
        data-ng-change="validateGlAccount(...,'GLAccountID')"
        ng-style="{'border-color': (amortizationForm.GLAccountID.$touched && 
                                    amortizationForm.GLAccountID.$invalid) ? 'red' : ''}" 
        required >
```
**Business Rule**: GL Account is required and validated on change.

**2. Contra Account Validation** (Lines 59-75):
```html
<select data-ng-model="HedgeRelationshipOptionTimeValueAmort.ContraAccountID"
        name="ContraAccountID"
        data-ng-change="validateGlAccount(...,'ContraAccountID')"
        required>
```
**Business Rule**: Contra Account is required and validated on change.

**3. Multi-Select Financial Centers** (Lines 77-89):
```html
@Html.ListBox("selectFinancialCenters", ...,
    new Dictionary<string, object>
    {
        {"class" ,"multiselect-holiday-calendar-hr hide"},
        ...
    })
```
**Business Rule**: Multiple financial centers can be selected (holiday calendars).

### New Implementation (AmortizationTab.razor)

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Schedules Grid | ✅ | ❓ | �� TO VERIFY |
| Values Grid | ✅ | ❓ | 🔍 TO VERIFY |
| GL Account Validation | ✅ Required | ❓ | 🔍 TO VERIFY |
| Contra Validation | ✅ Required | ❓ | 🔍 TO VERIFY |
| Financial Centers | ✅ Multi-select | ❓ | 🔍 TO VERIFY |
| Form Validation | ✅ Angular forms | ❓ | 🔍 TO VERIFY |

---

## Option Amortization Tab

### Legacy Implementation (optionTimeValue.cshtml)

Similar structure to Amortization Tab with option-specific fields.

### New Implementation (OptionAmortizationTab.razor)

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Option-specific fields | ✅ | ❓ | 🔍 TO VERIFY |
| GL Account setup | ✅ | ❓ | 🔍 TO VERIFY |
| Payment settings | ✅ | ❓ | 🔍 TO VERIFY |

---

## History Tab

### Legacy Implementation (historyView.cshtml)

#### Key Features:
1. **Activity Timeline** (Lines 1-21)
   - Icon-based activity display
   - Different icons per activity type:
     - Briefcase: BackloadRegression, RelationshipDesignated
     - Line Chart: UserRegression, PeriodicRegression
     - Check: RelationshipCreated, RelationshipUpdated
   - Activity text with conditional link for Designated
   - Timestamp and user name display

#### Business Rules:

**1. Activity Icons** (Line 5):
```html
<i class="fa pull-left annotationNotesDisplay" 
   data-ng-class="{'fa-briefcase':a.ActivityTypeEnum==='BackloadRegression' || 
                                   a.ActivityTypeEnum==='RelationshipDesignated',
                   'fa-line-chart':a.ActivityTypeEnum==='UserRegression' || 
                                   a.ActivityTypeEnum==='PeriodicRegression',
                   'fa-check':a.ActivityTypeEnum==='RelationshipCreated' || 
                              a.ActivityTypeEnum==='RelationshipUpdated'}" 
   aria-hidden="true"></i>
```
**Business Rule**: Specific icons mapped to activity types.

**2. Inception Package Link** (Line 9):
```html
<a data-ng-show="a.ActivityTypeEnum==='Designated'" href="javascript:void(0);" 
   data-ng-click="generateInceptionPackage();">View inception package</a>
```
**Business Rule**: "View inception package" link only shown for Designated activity.

### New Implementation (HedgeRelationshipHistoryTab.razor)

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Activity Timeline | ✅ Icon + text | ❓ | 🔍 TO VERIFY |
| Activity Icons | ✅ Type-specific | ❓ | 🔍 TO VERIFY |
| Inception Link | ✅ Designated only | ❓ | 🔍 TO VERIFY |
| Timestamp Display | ✅ Date + time | ❓ | 🔍 TO VERIFY |
| User Name Display | ✅ Full name | ❓ | 🔍 TO VERIFY |

---

## Logs Tab

### Legacy Implementation

The details.cshtml appears to be embedded in the main view rather than a separate partial view for this tab.

### New Implementation (HedgeRelationshipLogsTab.razor)

#### Comparison & Gaps:

| Feature | Legacy | New | Status |
|---------|--------|-----|--------|
| Logs Display | ✅ | ❓ | 🔍 TO VERIFY |
| Log Filtering | ❓ | ❓ | 🔍 TO VERIFY |
| Log Details | ❓ | ❓ | 🔍 TO VERIFY |

---

## Workflow Actions Analysis

### 1. DESIGNATE Action

#### Legacy Implementation (Lines 2716-2741 in controller):
```javascript
initiateDesignation = function () {
    $scope.ha_errors = [];
    $haService
        .setUrl("HedgeRelationship/FindDocumentTemplate/" + id)
        .get()
        .then(function (response) {
            if (response.data) {
                // Document template found - save first, then designate
                $scope.submit(undefined, function () {
                    $scope.init(id, function () {
                        $timeout(function () {
                            designate(checkDocumentTemplateKeywordsOnDesignated);
                        }, 1000);
                    });
                }, "InitiateDesignation");
            }
            else {
                // No template - proceed with designation and generate package
                designate(function () {
                    $scope.generatePackage(false);
                });
            }
        });
}
```

**Business Rules:**
1. Clear errors before starting
2. Check if document template exists
3. If template exists: Save → Reinit → Designate → Check keywords
4. If no template: Designate → Generate package
5. Designation includes regression run (Inception type)

#### New Implementation (Lines 859-893 in .razor.cs):
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
        // Save current state before designation
        await SaveHedgeRelationshipAsync();

        // Execute designation workflow
        var response = await Mediator.Send(new DesignateHedgeRelationship.Command(HedgeId));
        
        if (response.HasError)
        {
            await AlertService.ShowToast(response.ErrorMessage, ...);
            return;
        }

        // Update the local hedge relationship with the latest state
        HedgeRelationship = response.HedgeRelationship;
        
        await AlertService.ShowToast("Hedge Relationship successfully designated.", ...);
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await AlertService.ShowToast($"Error during designation: {ex.Message}", ...);
    }
}
```

**Comparison:**
| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Validation | ✅ Via errors array | ✅ DesignationRequirementsValidator | ⚠️ |
| Template Check | ✅ FindDocumentTemplate | ❓ Implicit in command? | 🔍 TO VERIFY |
| Save First | ✅ submit() | ✅ SaveHedgeRelationshipAsync() | ✅ |
| Designation Call | ✅ API call | ✅ MediatR Command | ✅ |
| Regression Run | ✅ Included | ❓ | 🔍 TO VERIFY |
| Package Generation | ✅ Conditional | ❓ | 🔍 TO VERIFY |
| Keyword Check | ✅ checkDocumentTemplateKeywords | ❓ | 🔍 TO VERIFY |

---

### 2. DE-DESIGNATE Action

#### Legacy Implementation (Lines 2797-2890 in controller):

**Step 1: Initialize Dialog** (Lines 2797-2821)
```javascript
if (value === "De-Designate") {
    $scope.DedesignateUserMessage = '';
    $scope.DedesignateDisabled = true;
    $scope.Model.DedesignationReason = 0;
    $scope.Model.Termination = false;
    $scope.Model.Ineffectiveness = false;
    $scope.Model.TimeValuesStartDate = moment().format('M/D/YYYY');
    $scope.Model.TimeValuesEndDate = moment().format('M/D/YYYY');
    $scope.Model.FullCashPayment = true;
    $scope.Model.PartialCashPayment = false;
    $scope.Model.NoCashPayment = false;
    $scope.Model.HedgedExposureNotExist = false;
    
    var dedesignationDate = moment().format("M/D/YYYY");
    $scope.Model.DeDesignation = {
        DedesignationDate: dedesignationDate,
        Payment: 0,
        Accrual: 0,
        BasisAdjustment: 0,
        BasisAdjustmentBalance: 0,
        CashPaymentType: 0,
        HedgedExposureExist: true
    };
```

**Step 2: Get Accrual from Last Hedging Item** (Lines 2822-2889)
```javascript
if ($scope.Model.HedgingItems.length > 0) {
    var hedgingItem = $scope.Model.HedgingItems[$scope.Model.HedgingItems.length - 1];
    
    // Determine URL based on security type
    if (hedgingItem.SecurityType === "Swap" || 
        hedgingItem.SecurityType === "CapFloor" || 
        hedgingItem.SecurityType === "Debt" || 
        hedgingItem.SecurityType === "Swaption") {
        url_ = hedgingItem.SecurityType + "Summary";
    }
    // ... more type checks
    
    if (url_ !== "") {
        // Get termination date
        $haService.setUrl("Trade/GetTerminationDate/" + hedgingItem.ItemID)
            .get()
            .then(function (response) {
                if (response.data !== null) {
                    var terminationDate = moment(response.data).format("M/D/YYYY");
                    
                    // Price the instrument to get accrual
                    url_ = "/" + url_ + "/Price?id=" + hedgingItem.ItemID + 
                           "&valueDate=" + terminationDate + 
                           "&instance=Last&discCurve=OIS" + userAction;
                    
                    $http({ method: "GET", url: url_, cache: false })
                        .then(function (response) {
                            $scope.Model.DeDesignation.Accrual = 
                                jQuery(jQuery.parseHTML(response.data.pricePV))
                                    .find("input[name='Accrued']").val();
                            
                            // Show dialog
                            ngDialog.open({...});
                        });
                }
            });
    }
}
```

**Step 3: Toggle Reason** (Lines 3174-3212)
```javascript
$scope.toggleReason = function (reason) {
    $scope.Model.DedesignationReason = reason;
    $scope.Model.Termination = reason == 0;
    $scope.Model.Ineffectiveness = reason == 1;
    
    $haService.setUrl('HedgeRelationship/Dedesignate/' + id + "/" + reason)
        .get()
        .then(function (response) {
            if (response.data === null) {
                $scope.DedesignateDisabled = true;
                if (reason === 0) {
                    $scope.DedesignateUserMessage = 'Status of Hedge Item is not Terminated.';
                }
            }
            else if (response.data.ErrorMessage) {
                $scope.DedesignateDisabled = true;
                $scope.DedesignateUserMessage = response.data.ErrorMessage;
                $scope.Model.DeDesignation.DedesignationDate = moment(response.data.DedesignationDate).format("M/D/YYYY");
            }
            else {
                $scope.DedesignateDisabled = false;
                $scope.DedesignateUserMessage = '';
                $scope.Model.TimeValuesStartDate = moment(response.data.DedesignationDate).format('M/D/YYYY');
                $scope.Model.TimeValuesEndDate = moment(response.data.TimeValuesEndDate).format('M/D/YYYY');
                $scope.Model.DeDesignation.Payment = response.data.Payment;
                $scope.Model.DeDesignation.ShowBasisAdjustmentBalance = response.data.ShowBasisAdjustmentBalance;
                $scope.Model.DeDesignation.BasisAdjustment = response.data.BasisAdjustment;
                $scope.Model.DeDesignation.BasisAdjustmentBalance = response.data.BasisAdjustmentBalance;
                $scope.Model.DeDesignation.CashPaymentType = 0;
                $scope.Model.DeDesignation.HedgedExposureExist = true;
            }
        });
};
```

**Business Rules:**
1. Initialize with default values (today's date, payment=0, FullCashPayment, HedgedExposureExist=true)
2. Get last hedging item's termination date and price it to get accrual
3. When reason changes (Termination vs Ineffectiveness):
   - Call API to get de-designation data
   - Handle validation (e.g., "Status of Hedge Item is not Terminated")
   - Populate dates, payment, basis adjustment fields
4. Show basis adjustment balance only for FairValue + non-Shortcut + Termination
5. Cash payment type affects field enable/disable state
6. Hedged exposure existence affects date field enable/disable

#### New Implementation (Lines 895-1029 in .razor.cs):
```csharp
private async Task HandleDeDesignateAsync()
{
    try
    {
        // Initialize de-designation model with default values
        DedesignateUserMessage = string.Empty;
        DedesignateIsError = false;
        IsDeDesignateDisabled = true;
        DedesignationReason = 0;
        DedesignateTimeValuesStartDate = DateTime.Today;
        DedesignateTimeValuesEndDate = DateTime.Today;
        CashPaymentType = 0;
        HedgedExposureExist = true;
        DedesignationDateDialog = DateTime.Today;
        DedesignatePayment = 0;
        DedesignateAccrual = 0;
        BasisAdjustment = 0;
        BasisAdjustmentBalance = 0;

        // Commented out: API Call to get termination date and price
        // Show De-Designation dialog
        OpenModal = MODAL_DEDESIGNATE;
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await AlertService.ShowToast($"Error during de-designation: {ex.Message}", ...);
    }
}

private async Task OnDeDesignateReasonChanged(int reason)
{
    DedesignationReason = reason;
    
    try
    {
        // API Call: Load de-designation data for the selected reason
        var response = await Mediator.Send(
            new GetDeDesignateData.Query(HedgeId, (DerivativeEDGEHAEntityEnumDedesignationReason)reason));
        
        if (response.HasError || !string.IsNullOrEmpty(response.ErrorMessage))
        {
            IsDeDesignateDisabled = true;
            DedesignateUserMessage = response.ErrorMessage ?? "An error occurred loading de-designation data";
            DedesignateIsError = true;
            DedesignationDateDialog = response.DedesignationDate;
        }
        else
        {
            IsDeDesignateDisabled = false;
            DedesignateUserMessage = string.Empty;
            DedesignateTimeValuesStartDate = response.TimeValuesStartDate;
            DedesignateTimeValuesEndDate = response.TimeValuesEndDate;
            DedesignationDateDialog = response.DedesignationDate;
            DedesignatePayment = response.Payment;
            ShowBasisAdjustmentBalance = response.ShowBasisAdjustmentBalance;
            BasisAdjustment = response.BasisAdjustment;
            BasisAdjustmentBalance = response.BasisAdjustmentBalance;
            CashPaymentType = 0;
            HedgedExposureExist = true;
        }
    }
    catch (Exception ex)
    {
        IsDeDesignateDisabled = true;
        DedesignateUserMessage = $"Error loading de-designation data: {ex.Message}";
        DedesignateIsError = true;
    }
    
    StateHasChanged();
}
```

**Comparison:**
| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Default Initialization | ✅ Today, 0 values | ✅ Today, 0 values | ✅ |
| Accrual Calculation | ✅ Price last item | ⚠️ Commented out | ❌ GAP |
| Reason Toggle API | ✅ GetDedesignate/reason | ✅ GetDeDesignateData | ✅ |
| Validation Messages | ✅ Multiple cases | ✅ Error handling | ⚠️ |
| Basis Adjustment Logic | ✅ Conditional | ✅ Via ShowBasisAdjustmentBalance | ✅ |
| Cash Payment Types | ✅ Three types | ✅ Implemented | ⚠️ Verify UI |
| Hedged Exposure Toggle | ✅ Affects dates | ❓ | 🔍 TO VERIFY |

**CRITICAL GAP**: Accrual calculation from pricing is commented out in new implementation!

---

### 3. RE-DESIGNATE Action

#### Legacy Implementation (Lines 2743-2791 in controller):
```javascript
reDesignate = function (isDocTemplateFound) {
    checkAnalyticsStatus(function () {
        $haService
            .setUrl('HedgeRelationship/Redesignate/' + id)
            .get()
            .then(function (response) {
                $scope.Model.RedesignationDate = moment(response.data.RedesignationDate).format('M/D/YYYY');
                $scope.Model.TimeValuesStartDate = moment(response.data.TimeValuesStartDate).format('M/D/YYYY');
                $scope.Model.TimeValuesEndDate = moment(response.data.TimeValuesEndDate).format('M/D/YYYY');
                $scope.Model.Payment = 0;
                $scope.Model.DayCountConv = response.data.DayCountConv;
                $scope.Model.PayBusDayConv = response.data.PayBusDayConv;
                $scope.Model.PaymentFrequency = response.data.PaymentFrequency;
                $scope.Model.AdjustedDates = response.data.AdjustedDates;
                $scope.Model.MarkAsAcquisition = response.data.MarkAsAcquisition;
                $scope.Model.IsDocTemplateFound = isDocTemplateFound;

                ngDialog.open({
                    template: 'redesignateDialog',
                    controller: 'reDesignateCtrl',
                    scope: $scope,
                    className: 'ngdialog-theme-default ngdialog-theme-custom',
                    title: 'Re-Designation Workflow',
                    showTitleCloseshowClose: true
                });
            });
    });
}
```

**Business Rules:**
1. Check analytics status before proceeding
2. Load re-designation data from API (dates, conventions, frequencies)
3. Show dialog with form fields:
   - Re-designation Date
   - Payment Frequency
   - Day Count Convention
   - Payment Convention
   - Start Date / End Date
   - Amount
   - Adjusted Dates checkbox
   - Mark As Acquisition checkbox

#### New Implementation (Lines 1053-1100 in .razor.cs):
```csharp
private async Task HandleReDesignateAsync()
{
    try
    {
        // Load re-designation data
        var response = await Mediator.Send(new GetReDesignateData.Query(HedgeId));
        
        if (response.HasError)
        {
            await AlertService.ShowToast(response.ErrorMessage, ...);
            return;
        }

        // Initialize dialog with API data
        RedesignationDate = response.RedesignationDate;
        RedesignateTimeValuesStartDate = response.TimeValuesStartDate;
        RedesignateTimeValuesEndDate = response.TimeValuesEndDate;
        RedesignatePayment = 0;
        RedesignateDayCountConv = response.DayCountConv;
        RedesignatePayBusDayConv = response.PayBusDayConv;
        RedesignatePaymentFrequency = response.PaymentFrequency;
        RedesignateAdjustedDates = response.AdjustedDates;
        RedesignateMarkAsAcquisition = response.MarkAsAcquisition;

        // Show dialog
        OpenModal = MODAL_REDESIGNATE;
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await AlertService.ShowToast($"Error during re-designation: {ex.Message}", ...);
    }
}
```

**Comparison:**
| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Analytics Check | ✅ checkAnalyticsStatus | ❓ | 🔍 TO VERIFY |
| Load Data API | ✅ Redesignate/id | ✅ GetReDesignateData | ✅ |
| Field Population | ✅ All fields | ✅ All fields | ✅ |
| Dialog Display | ✅ ngDialog | ✅ OpenModal | ✅ |

---

### 4. REDRAFT Action

#### Legacy Implementation (Lines 2894-2923 in controller):
```javascript
else if (value === "Redraft") {
    var gridObj = $("#amortizationDiv1").ejGrid("instance");
    var selectedRow = gridObj.selectedRowsIndexes[0];
    var selectedItem = $scope.Model.HedgeRelationshipOptionTimeValues[selectedRow];

    if (selectedItem !== undefined) {
        $scope.selectedRow1 = selectedRow;
        $haService
            .setUrl('HedgeRelationshipOptionTimeValueAmort')
            .setId(selectedItem.ID)
            .destroy($scope.selectedItem)
            .then(function () {
                $scope.init(selectedItem.HedgeRelationshipID);
                $haService
                    .setUrl('HedgeRelationship/Redraft')
                    .post($scope)
                    .then(function (response) {
                        $scope.init(id);
                    });
            });
    }
    else {
        $haService
            .setUrl('HedgeRelationship/Redraft')
            .post($scope)
            .then(function (response) {
                $scope.init(id);
            });
    }
}
```

**Business Rules:**
1. Check if option time value amortization row is selected
2. If selected: Delete the amortization entry first, then redraft
3. If not selected: Just redraft
4. Reload hedge relationship after redraft

#### New Implementation (Lines 1031-1053 in .razor.cs):
```csharp
private async Task HandleRedraftAsync()
{
    try
    {
        // Execute redraft
        var response = await Mediator.Send(new RedraftHedgeRelationship.Command(HedgeId));
        
        if (response.HasError)
        {
            await AlertService.ShowToast(response.ErrorMessage, ...);
            return;
        }

        // Update local hedge relationship
        HedgeRelationship = response.Data;
        
        await AlertService.ShowToast("Hedge Relationship successfully redrafted.", ...);
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await AlertService.ShowToast($"Error during redraft: {ex.Message}", ...);
    }
}
```

**Comparison:**
| Aspect | Legacy | New | Match? |
|--------|--------|-----|--------|
| Amortization Check | ✅ Check selected row | ❓ | 🔍 TO VERIFY |
| Delete Amortization | ✅ Conditional | ❓ | 🔍 TO VERIFY |
| Redraft API Call | ✅ POST | ✅ Command | ✅ |
| Reload Data | ✅ init(id) | ✅ Update model | ✅ |

---

## Business Rules Summary

### Critical Business Rules Identified

1. **Workflow Action Availability**
   - Draft: Only Designate
   - Designated: Redraft, De-Designate, Re-Designate (CashFlow only)
   - Dedesignated: Only Redraft
   - All actions require roles 24, 17, or 5

2. **Button Disable Logic**
   - Save: Disabled for non-Draft if user lacks required roles
   - Preview: Requires regression batch + Draft or required roles
   - Regression: Requires valid benchmark (except FX) + Draft or required roles
   - Backload: Requires required roles + non-Draft state

3. **Benchmark Filtering**
   - CashFlow: Excludes FFUTFDTR, FHLBTopeka, USDTBILL4WH15
   - FairValue: Excludes FFUTFDTR, FHLBTopeka, USDTBILL4WH15, Other, Prime

4. **Hedge Type Restrictions**
   - NetInvestment NOT available for InterestRate risk

5. **Field Auto-Reset Rules**
   - Complex logic based on HedgeRiskType and HedgeType combinations
   - Affects Benchmark, ExposureCurrency, HedgeExposure, HedgeAccountingTreatment

6. **Option Hedge Rules**
   - When unchecked: Reset AmortizeOptionPremimum, IsDeltaMatchOption, ExcludeIntrinsicValue
   - Dedesignated state forces IsAnOptionHedge to false

7. **Effectiveness Methods Filtering**
   - Filtered by HedgeType (FairValue, CashFlow, NetInvestment)
   - Further filtered if IsAnOptionHedge
   - Specific exclusions for non-option hedges

8. **Permission-Based Display**
   - DraftDesignatedIsDPIUser: Controls field editability
   - DesignatedIsDPIUser: Locks most fields
   - Contract types (SaaS/SwaS) affect permissions

9. **De-Designation Rules**
   - Requires pricing of last hedging item for accrual
   - Reason (Termination vs Ineffectiveness) changes validation
   - Basis adjustment only for FairValue + non-Shortcut + Termination
   - Cash payment type affects field states
   - Hedged exposure affects date field availability

10. **Re-Designation Rules**
    - Only for CashFlow hedge types
    - Must check analytics status first
    - Loads conventions and frequencies from API

---

## Gap Analysis

### HIGH PRIORITY GAPS

#### 1. **De-Designate: Accrual Calculation**
**Status**: ❌ CRITICAL GAP  
**Legacy**: Prices last hedging item to get accrual value  
**New**: Commented out code  
**Impact**: Incorrect accrual value in de-designation  
**Location**: HandleDeDesignateAsync() lines 916-937

#### 2. **Workflow: Analytics Status Check**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Checks analytics status before Re-Designate  
**New**: Not visible in reviewed code  
**Impact**: May proceed with stale analytics data  
**Location**: HandleReDesignateAsync()

#### 3. **Redraft: Option Amortization Check**
**Status**: �� NEEDS VERIFICATION  
**Legacy**: Checks for selected amortization row and deletes if present  
**New**: Direct redraft call without check  
**Impact**: May not clean up amortization data properly  
**Location**: HandleRedraftAsync()

#### 4. **Designate: Template Keyword Check**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Checks document template keywords after designation  
**New**: Not visible in reviewed code  
**Impact**: May not validate template completeness  
**Location**: HandleDesignateAsync()

#### 5. **Designate: Package Generation**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Conditionally generates inception package  
**New**: Not visible in workflow  
**Impact**: May not generate package automatically  
**Location**: initiateDesignation()

### MEDIUM PRIORITY GAPS

#### 6. **Benchmark Label Dynamic Change**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Changes label to "Contractual Rate" for IR+CF  
**New**: Static label?  
**Impact**: User confusion  
**Location**: setBenchmarkLabel()

#### 7. **Hedge Type Filtering by Risk Type**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Filters out NetInvestment for InterestRate  
**New**: Implementation unclear  
**Impact**: Invalid combinations possible  
**Location**: setDropDownListHedgeType()

#### 8. **Field Auto-Reset Logic**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Complex cascade of field resets  
**New**: Not visible  
**Impact**: Inconsistent state  
**Location**: setBenchmarkContractualRateExposure()

#### 9. **Effectiveness Methods Filtering**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Multi-criteria filtering  
**New**: Implementation unclear  
**Impact**: Invalid methods available  
**Location**: setDropDownListEffectivenessMethods()

#### 10. **EOM Checkbox Conditional Enable**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Only enabled when Period Size = Month  
**New**: Unclear  
**Impact**: Invalid data entry  
**Location**: InstrumentAnalysisTab

### LOW PRIORITY GAPS

#### 11. **Delete Test Permission Check**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Delete only for Draft or roles 24/17/5  
**New**: Unclear  
**Impact**: Unauthorized deletions  
**Location**: TestResultsTab

#### 12. **Autocorrelation Stats Display**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Only for Cumulative Changes  
**New**: Unclear  
**Impact**: Confusing display  
**Location**: TestResultsTab

#### 13. **Activity History Icons**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Type-specific icons  
**New**: Unclear  
**Impact**: Poor UX  
**Location**: HistoryTab

#### 14. **Inception Package Link**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Only for Designated activity  
**New**: Unclear  
**Impact**: Missing functionality  
**Location**: HistoryTab

#### 15. **Amortization GL Account Validation**
**Status**: 🔍 NEEDS VERIFICATION  
**Legacy**: Required with real-time validation  
**New**: Unclear  
**Impact**: Invalid data  
**Location**: AmortizationTab

---

## Action Items

### CRITICAL - MUST FIX BEFORE RELEASE

#### ACTION ITEM #1: Implement De-Designate Accrual Calculation
**Priority**: CRITICAL  
**File**: `HedgeRelationshipDetails.razor.cs`  
**Method**: `HandleDeDesignateAsync()`  
**Lines**: 916-937

**Detailed Prompt**:
```
Implement the de-designation accrual calculation logic in the HandleDeDesignateAsync() method.

Current State:
- The code to get termination date and price the last hedging item is commented out (lines 916-937)

Required Implementation:
1. Check if HedgeRelationship.HedgingItems has any items
2. Get the last hedging item from the array
3. Determine the API URL based on SecurityType:
   - "Swap", "CapFloor", "Debt", "Swaption" → "{SecurityType}Summary"
   - "Cancelable", "Collar", "DebtOption", "CallableDebt", "Corridor" → "{SecurityType}"
   - "SwapWithOption" → "SwapEmbeddedOption"
   - For "Collar": Add "&userAction=price_collar" parameter
4. Call GetTerminationDate API with the item's ItemID
5. If termination date is not null:
   a. Call Price API with: itemID, terminationDate, instance=Last, discCurve=OIS
   b. Parse the response to extract the Accrued value from the pricePV HTML
   c. Set DedesignateAccrual to this value
6. Show the de-designation dialog

Legacy Reference:
- Lines 2822-2889 in hr_hedgeRelationshipAddEditCtrl.js
- API: "Trade/GetTerminationDate/{itemID}"
- API: "/{url}/Price?id={itemID}&valueDate={date}&instance=Last&discCurve=OIS{userAction}"

Expected Result:
- DedesignateAccrual should be populated with the accrued value from pricing
- Dialog should display with correct accrual amount pre-filled
```

---

#### ACTION ITEM #2: Verify and Implement Workflow Analytics Check
**Priority**: CRITICAL  
**File**: `HedgeRelationshipDetails.razor.cs`  
**Method**: `HandleReDesignateAsync()`

**Detailed Prompt**:
```
Verify and implement the analytics status check before allowing Re-Designation.

Current State:
- No visible analytics status check before re-designation

Required Implementation:
1. Before showing the re-designation dialog, check analytics status
2. The legacy code calls checkAnalyticsStatus(callback) before proceeding
3. This likely checks if there are any pending or failed analytics jobs
4. If analytics are not ready, show appropriate message and block re-designation

Legacy Reference:
- Line 2744 in hr_hedgeRelationshipAddEditCtrl.js: checkAnalyticsStatus(function () {...})

Investigation Needed:
1. Check if CheckAnalyticsStatus query/command exists in the codebase
2. Determine what constitutes "ready" analytics status
3. Verify if this should also apply to other workflow actions

Expected Result:
- Re-designation blocked if analytics are not in a valid state
- User sees clear message about why they cannot re-designate
- Once analytics complete, re-designation becomes available
```

---

#### ACTION ITEM #3: Implement Redraft Option Amortization Cleanup
**Priority**: HIGH  
**File**: `HedgeRelationshipDetails.razor.cs`  
**Method**: `HandleRedraftAsync()`

**Detailed Prompt**:
```
Implement the option amortization cleanup logic before redrafting.

Current State:
- Direct redraft without checking for selected amortization entries

Required Implementation:
1. Check if there's a selected option time value amortization entry
2. If there is a selected entry:
   a. Delete the amortization entry first (HedgeRelationshipOptionTimeValueAmort API)
   b. Then proceed with redraft
3. If no entry selected:
   a. Proceed directly with redraft
4. After redraft, reload the hedge relationship data

Legacy Reference:
- Lines 2894-2923 in hr_hedgeRelationshipAddEditCtrl.js
- Checks $("#amortizationDiv1").ejGrid("instance").selectedRowsIndexes[0]
- Deletes selectedItem using .setUrl('HedgeRelationshipOptionTimeValueAmort').setId(selectedItem.ID).destroy()

Implementation Considerations:
1. How is the selected amortization row tracked in Blazor?
2. Should this be a parameter passed to the method?
3. Does the OptionAmortizationTab maintain a selected item reference?

Expected Result:
- If amortization entry selected: Entry deleted, then redraft succeeds
- If no entry: Redraft proceeds immediately
- Hedge relationship refreshes with Draft state
```

---

### HIGH PRIORITY - VERIFY IMPLEMENTATION

#### ACTION ITEM #4: Verify Designation Template and Package Logic
**Priority**: HIGH  
**File**: `HedgeRelationshipDetails.razor.cs`  
**Method**: `HandleDesignateAsync()`

**Detailed Prompt**:
```
Verify and ensure the designation workflow includes template checking and package generation.

Current State:
- Basic designation flow implemented
- Unclear if template check and package generation are included

Required Verification:
1. Does DesignateHedgeRelationship.Command check for document template?
2. If template found, does it trigger keyword validation?
3. Is inception package generated automatically after designation?
4. Does designation include running an Inception regression?

Legacy Reference:
- Lines 2716-2741 in hr_hedgeRelationshipAddEditCtrl.js
- API: "HedgeRelationship/FindDocumentTemplate/{id}"
- Calls designate(checkDocumentTemplateKeywordsOnDesignated) if template found
- Calls designate(generatePackage) if no template
- Designation includes regression: .setUrl('HedgeRelationship/Regress?hedgeResultType=Inception')

Required Implementation (if missing):
1. Check FindDocumentTemplate API before designation
2. If template exists:
   a. Save hedge relationship
   b. Designate
   c. Check document template keywords
3. If no template:
   a. Designate
   b. Generate inception package
4. Ensure Inception regression is triggered as part of designation

Expected Result:
- Template validation occurs when appropriate
- Inception package generated automatically
- User sees appropriate prompts for template keywords
- Regression results available after designation
```

---

#### ACTION ITEM #5: Verify Benchmark and Field Filtering Logic
**Priority**: HIGH  
**File**: `HedgeRelationshipDetails.razor.cs` or related

**Detailed Prompt**:
```
Verify and implement all dropdown filtering and field auto-reset business rules.

Areas to Verify:

1. **Benchmark Filtering** (setBenchmarkList in legacy):
   - CashFlow hedges: Exclude FFUTFDTR, FHLBTopeka, USDTBILL4WH15
   - FairValue hedges: Exclude FFUTFDTR, FHLBTopeka, USDTBILL4WH15, Other, Prime
   - Check if this filtering exists in AvailableBenchmarks or similar property

2. **Hedge Type Filtering** (setDropDownListHedgeType in legacy):
   - If HedgeRiskType = InterestRate: Exclude NetInvestment from options
   - Otherwise: Show all hedge types

3. **Field Auto-Reset** (setBenchmarkContractualRateExposure in legacy):
   When HedgeRiskType or HedgeType changes:
   - ForeignExchange + CashFlow: Reset Benchmark, ExposureCurrency, HedgeAccountingTreatment
   - ForeignExchange + FairValue: Reset Benchmark, ExposureCurrency, HedgeAccountingTreatment
   - ForeignExchange + NetInvestment: Reset Benchmark, HedgeExposure
   - InterestRate: Reset HedgeExposure, ExposureCurrency, HedgeAccountingTreatment

4. **Benchmark Label** (setBenchmarkLabel in legacy):
   - InterestRate + CashFlow: Label = "Contractual Rate"
   - Otherwise: Label = "Benchmark"

5. **Effectiveness Methods Filtering** (setDropDownListEffectivenessMethods in legacy):
   - Filter by HedgeType (IsForFairValue, IsForCashFlow, IsForNetInvestment)
   - Further filter by IsAnOptionHedge
   - Exclude "Regression - Change in Intrinsic Value" for non-option hedges

Legacy Reference:
- Lines 35-58: setBenchmarkList
- Lines 271-295: setBenchmarkContractualRateExposure
- Lines 254-268: setBenchmarkLabel
- Lines 308-326: setDropDownListHedgeType
- Lines 149-181: setDropDownListEffectivenessMethods

Expected Result:
- Dropdowns show only valid options based on current selections
- Fields auto-reset to prevent invalid combinations
- Label changes reflect context
- User cannot create invalid hedge configurations
```

---

### MEDIUM PRIORITY - VERIFY UX BEHAVIOR

#### ACTION ITEM #6: Verify Permission-Based Field Behavior
**Priority**: MEDIUM  
**Investigation Needed**

**Detailed Prompt**:
```
Verify that field editability matches legacy permission logic.

Permission Variables (from legacy):
1. DraftDesignatedIsDPIUser:
   - True if: HedgeState = Draft OR (HedgeState = Designated AND (IsDPIUser OR ContractType = 'SaaS' OR ContractType = 'SwaS'))
   - Controls: Most field editability in details view

2. DraftIsDPIUser:
   - True if: HedgeState = Draft AND IsDPIUser
   - Controls: Specific DPI user actions in Draft

3. DesignatedIsDPIUser:
   - True if: HedgeState = Designated AND NOT (IsDPIUser OR ContractType = 'SaaS' OR ContractType = 'SwaS')
   - Controls: Read-only mode for designated non-DPI users

Fields to Verify:
1. Client dropdown: Editable when DraftDesignatedIsDPIUser
2. Entity dropdown: Disabled when DesignatedIsDPIUser
3. Description: Editable when DraftDesignatedIsDPIUser
4. Template: Disabled when DesignatedIsDPIUser
5. All initial view fields: Editable based on permissions

Legacy Reference:
- Lines 598-600 in hr_hedgeRelationshipAddEditCtrl.js
- data-ng-disabled attributes throughout detailsView.cshtml

Expected Result:
- Non-DPI users see read-only fields when designated
- DPI users and SaaS/SwaS contracts maintain editability
- Field states match legacy behavior exactly
```

---

#### ACTION ITEM #7: Verify Trade Selection and Grid Behavior
**Priority**: MEDIUM  
**File**: `InstrumentAnalysisTab.razor`

**Detailed Prompt**:
```
Verify trade selection buttons visibility and trade grid functionality.

Business Rules to Verify:

1. **Hide/Show Trade Buttons** (hideSelectNewOrRemoveTrade in legacy):
   - Hide when: DesignatedIsDPIUser = true
   - Hide when: User lacks roles 24/17/5 AND HedgeState != Draft
   - Show otherwise

2. **Trade Types Available**:
   Verify all trade types in dropdown:
   - Callable Debt
   - Cancelable
   - Cap Floor
   - Collar
   - Debt
   - Debt Option
   - Swap
   - Swap With Cap/Floor
   - Swaption
   - Corridor
   - FX Forward

3. **Grid Functionality**:
   - Add existing trade (via dialog)
   - Add new trade (via navigation)
   - Remove trade (delete button per row)
   - Display all required columns

4. **Effectiveness Section**:
   - Prospective/Retrospective method dropdowns with filtering
   - Report currency dropdown
   - Report frequency dropdown
   - Regression settings (Period Count, Period Size)
   - Cumulative vs Periodic radio buttons
   - EOM checkbox (enabled only when Period Size = Month)

Legacy Reference:
- Lines 3133-3143: hideSelectNewOrRemoveTrade
- instrumentsAnalysisView.cshtml lines 1-201

Expected Result:
- Trade buttons hidden/shown based on permissions
- All trade types available in dropdown
- Grid allows add/remove as per permissions
- Effectiveness settings match legacy behavior
- EOM checkbox conditional enablement works
```

---

#### ACTION ITEM #8: Verify Test Results Display and Actions
**Priority**: MEDIUM  
**File**: `TestResultsTab.razor`

**Detailed Prompt**:
```
Verify test results display and action availability.

Display to Verify:

1. **Most Recent Test Statistics**:
   - Slope, Std Error, t-Test (format: value/analytics)
   - R, Obs, Significance
   - R², y-Intercept, F-Stat (format: value/analytics)
   - **Conditional**: d, Positive/Negative Autocorrelation (only if CumulativeChanges = true)

2. **Regression Chart**:
   - Scatter plot of data points
   - Trendline
   - Proper axes and labels

3. **All Tests Grid**:
   - List of all historical tests
   - Action dropdown per row

Actions to Verify:

1. **Download Excel**:
   - Always available

2. **Delete**:
   - Available when: HedgeState = 'Draft' OR user has role 24/17/5
   - Hidden otherwise

Legacy Reference:
- hedgetestResultsView.cshtml lines 1-98
- Lines 52-65: Autocorrelation display conditional
- Line 96: Delete action conditional

Expected Result:
- All statistics display correctly
- Autocorrelation stats only show for Cumulative Changes
- Chart renders with trendline
- Delete action only available per business rules
- Download Excel always works
```

---

### LOW PRIORITY - VERIFY DETAILS

#### ACTION ITEM #9: Verify Amortization Tab Validation
**Priority**: LOW  
**File**: `AmortizationTab.razor`

**Detailed Prompt**:
```
Verify amortization form validation and field requirements.

Validation Rules to Verify:

1. **GL Account**:
   - Required field
   - Validated on change
   - Red border if invalid
   - Error message displayed

2. **Contra Account**:
   - Required field
   - Validated on change
   - Red border if invalid
   - Error message displayed

3. **Financial Centers**:
   - Multi-select capability
   - Holiday calendar integration

4. **Other Fields**:
   - Payment Frequency dropdown
   - Day Count Convention dropdown
   - Payment Convention dropdown
   - Start Date / End Date
   - Premium / Amount fields

Legacy Reference:
- amortizationView.cshtml lines 39-135
- Angular form validation with required attribute
- ng-style for border color change
- Error message divs

Expected Result:
- GL Account and Contra are required
- Real-time validation feedback
- Error messages match legacy
- Multi-select for financial centers works
- Form cannot submit with invalid data
```

---

#### ACTION ITEM #10: Verify History Tab Display
**Priority**: LOW  
**File**: `HedgeRelationshipHistoryTab.razor`

**Detailed Prompt**:
```
Verify history tab activity display and formatting.

Display Requirements:

1. **Activity Icons**:
   - Briefcase icon (fa-briefcase): BackloadRegression, RelationshipDesignated
   - Line chart icon (fa-line-chart): UserRegression, PeriodicRegression
   - Check icon (fa-check): RelationshipCreated, RelationshipUpdated

2. **Activity Text**:
   - Display ActivityTypeText
   - For Designated activity: Add link "View inception package"
   - Link calls generateInceptionPackage() method

3. **Timestamp**:
   - Format: "MMMM dd, yyyy at h:mm a"
   - Example: "January 15, 2025 at 3:45 PM"

4. **User Name**:
   - Display: CreatedByUser.Person.FullName

Legacy Reference:
- historyView.cshtml lines 1-21
- Line 5: Icon mapping logic
- Line 9: Conditional link display
- Lines 13-15: Timestamp and user format

Expected Result:
- Correct icon for each activity type
- Inception package link only for Designated
- Properly formatted timestamps
- User names displayed
- Timeline sorted by date
```

---

## Conclusion

This analysis has identified **1 CRITICAL gap** (de-designation accrual calculation), **3 HIGH-priority items** requiring verification, and multiple medium/low priority items.

### Recommendation:
1. **IMMEDIATELY** address ACTION ITEM #1 (De-Designate Accrual)
2. **BEFORE RELEASE** verify and implement ACTION ITEMS #2-#5
3. **PRIOR TO UAT** address ACTION ITEMS #6-#8
4. **POST-UAT** (if time permits) verify ACTION ITEMS #9-#10

### Next Steps:
1. Create JIRA tickets for each action item
2. Assign priorities and sprint allocations
3. Schedule technical reviews for complex items
4. Plan UAT scenarios covering all business rules
5. Document any intentional deviations from legacy behavior

---

**Document Version**: 1.0  
**Last Updated**: October 17, 2025  
**Status**: Initial Analysis Complete - Awaiting Action Item Assignment

