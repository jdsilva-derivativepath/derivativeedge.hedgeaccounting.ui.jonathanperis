# Syncfusion Components Instructions for GitHub Copilot

## Project Context
This is a Blazor Server application that uses Syncfusion Blazor components for UI elements. Syncfusion components should ONLY use their native CSS classes and styling system, never Tailwind CSS classes.

## Core Principle
**CRITICAL RULE: Syncfusion components must ONLY use Syncfusion CSS classes. Never apply Tailwind CSS classes to Syncfusion components.**

## Available Syncfusion Components & Their CSS Classes

### Form Input Components

#### SfTextBox
```razor
<!-- Correct Usage -->
<SfTextBox CssClass="input-textbox" 
           Placeholder="Enter text" 
           @bind-Value="@Model.Property" />

<!-- NEVER use Tailwind classes -->
<SfTextBox CssClass="w-full px-3 py-2 border" /> <!-- ❌ WRONG -->
```

**Available CSS Classes:**
- `input-textbox` - Standard text input styling
- `e-outline` - Outlined text input
- `e-filled` - Filled text input
- `e-float-input` - Floating label input

#### SfButton
```razor
<!-- Correct Usage -->
<SfButton CssClass="e-primary" Content="Save" />
<SfButton CssClass="e-secondary" Content="Cancel" />
<SfButton CssClass="e-outline" Content="Edit" />
```

**Available CSS Classes:**
- `e-primary` - Primary button styling
- `e-secondary` - Secondary button styling  
- `e-outline` - Outlined button
- `e-flat` - Flat button without background
- `e-success` - Success/green button
- `e-warning` - Warning/yellow button
- `e-danger` - Danger/red button

#### SfRadioButton
```razor
<!-- Correct Usage -->
<SfRadioButton CssClass="input-radiobutton" 
               Name="optionGroup" 
               Value="option1" 
               TChecked="string" 
               @bind-Checked="@SelectedOption" />
```

**Available CSS Classes:**
- `input-radiobutton` - Standard radio button styling
- `e-small` - Small radio button
- `e-radio-wrapper` - Wrapper styling

#### SfCheckBox
```razor
<!-- Correct Usage -->
<SfCheckBox CssClass="input-checkbox" 
            Label="Accept Terms" 
            @bind-Checked="@IsAccepted" />
```

**Available CSS Classes:**
- `input-checkbox` - Standard checkbox styling
- `e-small` - Small checkbox
- `e-checkbox-wrapper` - Wrapper styling

#### SfComboBox
```razor
<!-- Correct Usage -->
<SfComboBox CssClass="dropdown-input" 
            TValue="int" 
            TItem="ItemModel" 
            DataSource="@Items" 
            @bind-Value="@SelectedValue">
    <ComboBoxFieldSettings Text="Name" Value="Id" />
</SfComboBox>
```

**Available CSS Classes:**
- `dropdown-input` - Standard dropdown styling
- `e-outline` - Outlined dropdown
- `e-filled` - Filled dropdown

### Navigation Components

#### SfTab
```razor
<!-- Correct Usage -->
<SfTab CssClass="custom-tab">
    <TabItems>
        <TabItem>
            <ChildContent>
                <TabHeader Text="Tab 1"></TabHeader>
            </ChildContent>
            <ContentTemplate>
                <!-- Tab content -->
            </ContentTemplate>
        </TabItem>
    </TabItems>
</SfTab>
```

**Available CSS Classes:**
- `custom-tab` - Custom tab styling
- `e-tab-header` - Tab header styling
- `e-vertical` - Vertical tabs
- `e-background` - Background tabs

#### SfAccordion
```razor
<!-- Correct Usage -->
<SfAccordion CssClass="custom-accordion">
    <AccordionItems>
        <AccordionItem>
            <HeaderTemplate>
                <div>Header Content</div>
            </HeaderTemplate>
            <ContentTemplate>
                <div>Content</div>
            </ContentTemplate>
        </AccordionItem>
    </AccordionItems>
</SfAccordion>
```

**Available CSS Classes:**
- `custom-accordion` - Custom accordion styling
- `e-expand-mode` - Expand mode styling

### Data Components

#### SfListBox
```razor
<!-- Correct Usage -->
<SfListBox CssClass="listbox-container" 
           TValue="List<int>" 
           TItem="ItemModel" 
           DataSource="@Items" 
           @bind-Value="@SelectedItems">
    <ListBoxFieldSettings Text="Name" Value="Id" />
    <ListBoxSelectionSettings ShowCheckbox="true" Mode="SelectionMode.Multiple" />
</SfListBox>
```

**Available CSS Classes:**
- `listbox-container` - Standard listbox styling
- `e-list-box` - List box wrapper
- `selected-roles-list-container` - Custom selected items container

#### SfGrid
```razor
<!-- Correct Usage -->
<SfGrid CssClass="custom-grid" 
        TValue="DataModel" 
        DataSource="@DataList">
    <GridColumns>
        <GridColumn Field="@nameof(DataModel.Name)" HeaderText="Name" />
    </GridColumns>
</SfGrid>
```

**Available CSS Classes:**
- `custom-grid` - Custom grid styling
- `e-grid` - Standard grid styling

### Message Components

#### SfMessage
```razor
<!-- Correct Usage -->
<SfMessage CssClass="alert-message" 
           Severity="MessageSeverityType.Error" 
           Visible="@ShowMessage">
    <h1>Error Title</h1>
    <p>Error message content</p>
</SfMessage>
```

**Available CSS Classes:**
- `alert-message` - Custom alert styling
- `e-message` - Standard message styling

## Best Practices

### DO ✅
1. **Always use Syncfusion CSS classes** for Syncfusion components
2. **Use consistent naming** following the existing pattern (`input-textbox`, `dropdown-input`, etc.)
3. **Apply layout styling to container divs** using Tailwind, not the Syncfusion component itself
4. **Follow existing component patterns** from the codebase

### DON'T ❌
1. **Never mix Tailwind with Syncfusion** component CssClass
2. **Don't use inline styles** on Syncfusion components unless absolutely necessary
3. **Don't override Syncfusion styles** with custom CSS without good reason
4. **Don't use generic class names** that might conflict with Syncfusion's internal styling

## Layout Pattern
```razor
<!-- Correct Pattern -->
<div class="grid grid-cols-3 gap-6 mb-6"> <!-- Tailwind for layout -->
    <div class="flex flex-col"> <!-- Tailwind for container -->
        <label class="block text-sm font-normal text-gray-700 mb-2">Label</label> <!-- Tailwind for label -->
        <SfTextBox CssClass="input-textbox" @bind-Value="@Model.Property" /> <!-- Syncfusion CSS only -->
        <ValidationMessage For="@(() => Model.Property)" class="text-xs text-red-600 mt-1" /> <!-- Tailwind for validation -->
    </div>
</div>
```

## Component-Specific CSS Classes Reference

### Input Components
| Component | CSS Class | Purpose |
|-----------|-----------|---------|
| SfTextBox | `input-textbox` | Standard text input |
| SfButton | `e-primary` | Primary action button |
| SfButton | `e-secondary` | Secondary action button |
| SfRadioButton | `input-radiobutton` | Radio button styling |
| SfCheckBox | `input-checkbox` | Checkbox styling |
| SfComboBox | `dropdown-input` | Dropdown/select styling |

### Navigation Components  
| Component | CSS Class | Purpose |
|-----------|-----------|---------|
| SfTab | `custom-tab` | Tab navigation |
| SfAccordion | `custom-accordion` | Collapsible sections |

### Data Components
| Component | CSS Class | Purpose |
|-----------|-----------|---------|
| SfListBox | `listbox-container` | List selection |
| SfGrid | `custom-grid` | Data grid |

### Message Components
| Component | CSS Class | Purpose |
|-----------|-----------|---------|
| SfMessage | `alert-message` | Alert/notification |

## Integration with Tailwind
- **Container elements**: Use Tailwind classes for layout, spacing, positioning
- **Syncfusion components**: Use only Syncfusion CSS classes
- **Labels and text**: Use Tailwind typography classes
- **Validation messages**: Use Tailwind utility classes

## Error Prevention
- Always use the `CssClass` property, never `class` on Syncfusion components
- If you need custom styling, extend the Syncfusion CSS class definitions rather than mixing frameworks
- Test component styling after changes to ensure Syncfusion theming remains intact

## Common Mistakes to Avoid
```razor
<!-- ❌ WRONG - Don't mix Tailwind with Syncfusion -->
<SfTextBox CssClass="input-textbox w-full px-3 border-gray-300" />

<!-- ❌ WRONG - Don't use class instead of CssClass -->
<SfTextBox class="input-textbox" />

<!-- ❌ WRONG - Don't use Tailwind utilities on Syncfusion components -->
<SfButton CssClass="bg-blue-500 text-white px-4 py-2" />

<!-- ✅ CORRECT - Use Syncfusion CSS classes only -->
<SfTextBox CssClass="input-textbox" />
<SfButton CssClass="e-primary" />
```

Remember: **Separation of concerns** - Tailwind for layout and containers, Syncfusion CSS for component styling.
