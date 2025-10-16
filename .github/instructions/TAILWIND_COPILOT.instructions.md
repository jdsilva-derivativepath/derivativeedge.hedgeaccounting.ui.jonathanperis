# Tailwind CSS Instructions for GitHub Copilot

## Project Context
This is a Blazor Server component library project that uses Tailwind CSS for styling. The Tailwind CSS is compiled from `input.css` to `output.css` using the Tailwind CLI and included in the project via `_Host.cshtml`.

## Tailwind Configuration
- **Config File**: `tailwind.config.js`
- **Input File**: `input.css`
- **Output File**: `DerivativeEdge.Blazor.ComponentLibrary.Demo/wwwroot/output.css`
- **Build Command**: `npx tailwindcss -i input.css -o DerivativeEdge.Blazor.ComponentLibrary.Demo/wwwroot/output.css --watch`

## Available Tailwind Classes in This Project

### Layout & Positioning
- `relative`, `absolute`, `fixed`, `sticky`, `static`
- `top-0`, `top-2`, `top-0.5`, `-top-24`, `bottom-0`, `left-0`, `right-0`, `-left-1`, `-left-24`
- `inset-0`, `inset-x-0`, `inset-y-0`
- `z-10`, `z-50`, `z-0`, `z-20`, `z-30`, `z-40`

### Display & Flexbox
- `block`, `inline-block`, `inline`, `flex`, `inline-flex`, `grid`, `hidden`
- `flex-row`, `flex-col`, `flex-wrap`, `flex-nowrap`
- `justify-start`, `justify-center`, `justify-between`, `justify-around`, `justify-evenly`
- `items-start`, `items-center`, `items-end`, `items-stretch`

### Spacing
- **Padding**: `p-{size}`, `px-{size}`, `py-{size}`, `pt-{size}`, `pb-{size}`, `pl-{size}`, `pr-{size}`
- **Margin**: `m-{size}`, `mx-{size}`, `my-{size}`, `mt-{size}`, `mb-{size}`, `ml-{size}`, `mr-{size}`
- **Sizes**: `0`, `0.5`, `1`, `1.5`, `2`, `2.5`, `3`, `4`, `5`, `6`, `8`, `10`, `12`, `16`, `20`, `24`

### Sizing
- **Width**: `w-full`, `w-auto`, `w-screen`, `w-5`, `w-8`, `w-12`, `w-16`, `w-20`, `w-60`, `w-80`
- **Height**: `h-full`, `h-auto`, `h-screen`, `h-0`, `h-5`, `h-7`, `h-8`, `h-9`, `h-12`

### Colors Available
- **Gray Scale**: `gray-100`, `gray-200`, `gray-300`, `gray-400`, `gray-500`, `gray-600`, `gray-700`, `gray-800`, `gray-900`
- **Text Colors**: `text-gray-{shade}`, `text-black`, `text-white`, `text-transparent`
- **Background Colors**: `bg-gray-{shade}`, `bg-white`, `bg-black`, `bg-transparent`
- **Border Colors**: `border-gray-{shade}`, `border-transparent`, `border-black`, `border-white`

### Borders & Radius
- **Border Width**: `border`, `border-0`, `border-2`, `border-4`, `border-8`
- **Border Sides**: `border-t`, `border-b`, `border-l`, `border-r`
- **Border Radius**: `rounded`, `rounded-sm`, `rounded-md`, `rounded-lg`, `rounded-xl`, `rounded-full`
- **Border Style**: `border-solid`, `border-dashed`, `border-dotted`

### Typography
- **Font Size**: `text-xs`, `text-sm`, `text-base`, `text-lg`, `text-xl`, `text-2xl`
- **Font Weight**: `font-thin`, `font-normal`, `font-medium`, `font-semibold`, `font-bold`
- **Text Align**: `text-left`, `text-center`, `text-right`
- **Text Transform**: `uppercase`, `lowercase`, `capitalize`
- **Line Height**: `leading-3`, `leading-4`, `leading-8`, `leading-none`, `leading-tight`
- **Letter Spacing**: `tracking-tighter`, `tracking-tight`, `tracking-normal`, `tracking-wide`

### Interactive States
- **Hover**: `hover:bg-{color}`, `hover:text-{color}`, `hover:border-{color}`
- **Focus**: `focus:outline-none`, `focus:ring-{size}`, `focus:ring-{color}`
- **Active**: `active:bg-{color}`, `active:text-{color}`

### Effects & Shadows
- **Opacity**: `opacity-0`, `opacity-25`, `opacity-50`, `opacity-75`, `opacity-100`
- **Shadow**: `shadow`, `shadow-sm`, `shadow-md`, `shadow-lg`, `shadow-xl`, `shadow-2xl`
- **Cursor**: `cursor-pointer`, `cursor-default`, `cursor-not-allowed`

## Conversion Guidelines

### When Converting Custom CSS to Tailwind:

1. **Replace positioning classes first**:
   - `position: relative` → `relative`
   - `position: absolute` → `absolute`
   - `top: 8px` → `top-2`

2. **Convert display and layout**:
   - `display: flex` → `flex`
   - `display: inline-block` → `inline-block`
   - `justify-content: space-between` → `justify-between`

3. **Handle colors systematically**:
   - `color: #6b7280` → `text-gray-500`
   - `background-color: #f3f4f6` → `bg-gray-100`
   - Use hover states: `hover:text-gray-900`, `hover:bg-gray-200`

4. **Convert spacing**:
   - `padding: 20px 0` → `py-5`
   - `margin-left: 12px` → `ml-3`
   - `height: 48px` → `h-12`

### Common Patterns in This Project:

```html
<!-- Menu Icon Pattern -->
<div class="relative inline-block mr-4 rounded-full w-8 h-8 text-center pt-1 hover:bg-gray-200">
  <a class="text-lg text-gray-700 hover:text-gray-900">
    <i class="fa-solid fa-grid"></i>
  </a>
</div>

<!-- Card/Modal Pattern -->
<div class="w-60 absolute top-0.5 -left-1 bg-white shadow-2xl rounded-md border border-gray-300 z-50">
  <div class="py-5 relative z-50 h-full w-full">
    <!-- Content -->
  </div>
</div>

<!-- Button/Link Pattern -->
<div class="px-5 py-1.5 relative h-12 leading-8 hover:bg-gray-100">
  <a href="#" class="text-sm text-gray-700 hover:text-gray-900">
    <!-- Content -->
  </a>
</div>
```

## File Structure Context

### Component Locations:
- `PortalLauncher/TopBar/TopBar.razor` - Main navigation bar
- `PortalLauncher/TopBar/Components/LaunchMenu.razor` - Dropdown menu
- `PortalLauncher/TopBar/Components/AppLink.razor` - App navigation links
- `PortalLauncher/TopBar/Components/TopLogo.razor` - Logo component

### CSS Files Being Replaced:
- Replace classes from `.razor.css` files with Tailwind utilities
- Keep app-specific icon classes that use gradients (e.g., `app-icon`, `commodities-icon`)
- Convert layout, spacing, typography, and basic styling to Tailwind

## DO NOT Convert:
- Complex gradient backgrounds (use existing CSS classes)
- Font-family declarations (unless using standard Tailwind fonts)
- Syncfusion component styling
- Third-party library specific classes

## Best Practices:
1. **Always check if classes exist in output.css before using**
2. **Never use inline styles** - Always use Tailwind utility classes or extend the Tailwind configuration with custom utilities instead of adding `style=""` attributes
3. **Use semantic grouping**: layout classes first, then spacing, then colors
4. **Prefer Tailwind utilities over custom CSS when possible**
5. **Maintain existing functionality and visual appearance**
6. **Use hover states consistently**: `hover:bg-gray-200`, `hover:text-gray-900`
7. **Keep responsive design in mind** (mobile-first approach)
8. **All pages should have auto-scroll** - Add `overflow-auto` to main page containers to enable automatic scrolling when content exceeds viewport height

## Build Process:
- Tailwind watches for changes automatically when using `--watch` flag
- Changes to .razor files trigger automatic rebuilds
- CSS is served from `DerivativeEdge.Blazor.ComponentLibrary.Demo/wwwroot/output.css`
- Include the CSS in `_Host.cshtml`: `<link rel="stylesheet" href="~/output.css" />`
