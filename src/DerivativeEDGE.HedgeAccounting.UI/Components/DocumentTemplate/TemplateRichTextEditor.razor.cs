namespace DerivativeEDGE.HedgeAccounting.UI.Components.DocumentTemplate;

public partial class TemplateRichTextEditor
{
    private Dictionary<string, SfRichTextEditor> _refRichTextEditor;
    [Parameter]
    public List<HedgeDocumentContentViewModel> HedgeDocumentContents { get; set; } = [];

    [Parameter]
    public ErrorMessage ErrorMsg { get; set; }

    [Parameter]
    public bool IsErrorMessageVisible { get; set; }

    [Inject]
    private IMediator MediatorService { get; set; } = default!;

    private static readonly List<SanitizeAttribute> additionalSanitizeAttributes =
    [
        new SanitizeAttribute {  Selector = "br", Attribute = "class"  },
        new SanitizeAttribute {  Selector = "br", Attribute = "style"  },
		new SanitizeAttribute {  Selector = "li", Attribute = "style"  },
		new SanitizeAttribute {  Selector = "ul", Attribute = "style"  },
		new SanitizeAttribute {  Selector = "ol", Attribute = "style"  },
        new SanitizeAttribute {  Selector = "font"  , Attribute="color"},
        new SanitizeAttribute {  Selector = "font" },
        new SanitizeAttribute {  Selector = "undefined" }
    ];

    private static ErrorMessage ContentEmptyValidationMsg => new()
    {
        Title = "Document content cannot be blank",                                        
        Message = "Fill in the required Hedge Document Content."
    };

    private List<EnumType> MergeData;

    private List<DropDownMenuItem> Items;

    private readonly char mentionChar = '{';

    protected override async Task OnInitializedAsync()
    {
        _refRichTextEditor = [];
        foreach (var item in HedgeDocumentContents)
        {
            _refRichTextEditor.Add(item.Id.ToString(), new SfRichTextEditor());
        }

        var result = await MediatorService.Send(new GetKeywordTypes.Query());
        if (result is not null)
        {
            MergeData = result;
            Items = [.. result.OrderBy(x=> x.Type).Select(x => new DropDownMenuItem() { Text = x.Description })];

        }

        await base.OnInitializedAsync();
    }

    public void OnItemSelect(MenuEventArgs args, SfRichTextEditor editor)
    {
        if (args.Item.Text != null)
        {
            var value = MergeData.FirstOrDefault(md => md.Description == args.Item.Text)?.Type;
            string htmlContent = $"<span contenteditable=\"false\" class=\"dp-mention-chip\"><span>{{{{{value}}}}}</span></span>";
            var undoOption = new ExecuteCommandOption { Undo = true };
            editor.ExecuteCommandAsync(CommandName.InsertHTML, htmlContent, undoOption);
        }
    }

    private readonly List<ToolbarItemModel> Tools =
     [
        new() { Command = ToolbarCommand.Bold },
        new() { Command = ToolbarCommand.Italic },
        new() { Command = ToolbarCommand.Underline },
        new() { Command = ToolbarCommand.Separator },
        new() { Command = ToolbarCommand.Alignments },
        new() { Command = ToolbarCommand.Separator },
        new() { Command = ToolbarCommand.BulletFormatList },
        new() { Command = ToolbarCommand.NumberFormatList },
        new() { Command = ToolbarCommand.Separator },
        new() { Command = ToolbarCommand.Indent },
        new() { Command = ToolbarCommand.Outdent },
        new ToolbarItemModel() { Name = "InsertField", TooltipText = "Insert Field" },
    ];
}