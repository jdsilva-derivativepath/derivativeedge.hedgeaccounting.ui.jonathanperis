namespace DerivativeEDGE.HedgeAccounting.UI.Pages.DocumentTemplate;

public partial class TemplateGallery
{
    private PreviewTemplateDialog _previewTemplateDialogComponent;

    private bool NoCardSelected { get; set; } = false;

    private CardItem SelectedCardItem { get; set; }

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; }

    private List<CardItem<Models.DocumentTemplate>> CardItemsListScratch { get; set; } = [];
    private List<CardItem<Models.DocumentTemplate>> CardItemsListDp { get; set; } = [];

    private List<Models.DocumentTemplate> RetrievedDpTemplates { get; set; } = [];

    private bool IsPreviewDialogVisible { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await GetDpTemplates();
        RefreshDPScratchCard();
        RefreshDPCards();
    }

    private void RefreshDPScratchCard()
    {
        CardItemsListScratch =
            [
                new() {
                    CardIconCss = "fa-regular fa-file",
                    HeaderText = "Blank document",
                    Description = "Create your own document template",
                    ActionButtonText = "Select Template",
                    IsSelected = false,
                }
            ];
    }

    private void RefreshDPCards()
    {
        CardItemsListDp = RetrievedDpTemplates.Count != 0 ? [.. RetrievedDpTemplates.Select(t => new CardItem<Models.DocumentTemplate>
        {
            CardIconCss = "fa-regular fa-file",
            HeaderText = $"{t.Name[..Math.Min(25, t.Name.Length)]}{(t.Name.Length > 25 ? "..." : string.Empty)}",
            Description = $"{t.Description[..Math.Min(50, t.Description.Length)]}{(t.Description.Length > 50 ? "..." : string.Empty)}",
            ActionButtonText = "Select Template",
            DataSource = t,
            IsSelected = false,
            HyperLinkText = "Preview",
        })]
        : [];
    }

    private async Task GetDpTemplates()
    {
        var response = await Mediator.Send(new GetDpTemplates.Query());

        if (response.DocumentTemplates.Count > 0)
        {
            RetrievedDpTemplates = [.. response.DocumentTemplates];
        }
    }

    private async Task OnButtonClick(string action)
    {
        if (action == "continue")
        {
            NoCardSelected = SelectedCardItem == null;

            if (!NoCardSelected) 
            {
                _navigation.NavigateTo($"create-template/{GetSelectedCardId()}");
            }
        }
        else
        {
            _navigation.NavigateTo("hrhedgedocumenttemplate");
        }
    }

    private void OnButtonClickHandler(CardItem card)
    {
        bool isSelected = card.IsSelected;

        SelectedCardItem = isSelected ? card : null;
        NoCardSelected = false;

        CardItemsListDp.ForEach(card => card.IsSelected = false);
        CardItemsListScratch.ForEach(card => card.IsSelected = false);

        card.IsSelected = isSelected;
    }

    private async Task OnHyperLinkClickHandler(CardItem card)
    {
        var cardItem = card as CardItem<Models.DocumentTemplate>;
        var documentTemplate = cardItem.DataSource;

        await _previewTemplateDialogComponent.FormatContent(documentTemplate.ClientId, documentTemplate.Name, documentTemplate.HedgeDocumentTemplateDetails);

        IsPreviewDialogVisible = true;
    }

    private void ClosePreview()
    {
        IsPreviewDialogVisible = false;
    }

    private Guid? GetSelectedCardId()
    {
        if (CardItemsListScratch.Any(card => card.IsSelected))
        {
            return null;
        }
        else if (CardItemsListDp.Any(card => card.IsSelected))
        {
            return CardItemsListDp.Where(card => card.IsSelected).FirstOrDefault().DataSource.Id;
        }

        return null;
    }
}