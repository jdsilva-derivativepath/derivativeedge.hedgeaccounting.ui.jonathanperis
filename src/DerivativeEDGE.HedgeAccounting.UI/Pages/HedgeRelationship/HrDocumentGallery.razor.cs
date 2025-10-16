namespace DerivativeEDGE.HedgeAccounting.UI.Pages.HedgeRelationship;

public partial class HrDocumentGallery
{
    private PreviewTemplateDialog _previewTemplateDialogComponent;

    private bool NoCardSelected { get; set; } = false;

    private CardItem SelectedCardItem { get; set; }

    [SupplyParameterFromQuery]
    public long? ClientId { get; set; }

    [SupplyParameterFromQuery]
    public long? HedgeRelationshipId { get; set; }

    [SupplyParameterFromQuery]
    public bool? IsChangingTemplate { get; set; }

    [Inject]
    public IMediator Mediator { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; }

    private List<CardItem<Models.DocumentTemplate>> CardItemsListScratch { get; set; } = [];
    private List<CardItem<Models.DocumentTemplate>> CardItemsListDp { get; set; } = [];
    private List<CardItem<Models.DocumentTemplate>> CardItemsListClient { get; set; } = [];

    private List<Models.DocumentTemplate> RetrievedDpTemplates { get; set; } = [];
    private List<Models.DocumentTemplate> RetrievedClientTemplates { get; set; } = [];

    private bool IsPreviewDialogVisible { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await GetClientTemplates();
        await GetDpTemplates();
        RefreshDPScratchCard();
        RefreshClientCards();
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

    private void RefreshClientCards()
    {
        CardItemsListClient = RetrievedClientTemplates.Count != 0 ? [.. RetrievedClientTemplates.Select(t => new CardItem<Models.DocumentTemplate>
        {
            CardIconCss = "fa-regular fa-file",
            HeaderText = $"{t.Name[..Math.Min(25, t.Name.Length)]}{(t.Name.Length > 25 ? "..." : string.Empty)}",
            Description = $"{t.Description[..Math.Min(50, t.Description.Length)]}{(t.Description.Length > 50 ? "..." : string.Empty)}",
            FirstSubText = $"Updated by: {t.ModifiedBy}",
            SecondarySubText = $"Last Updated: {t.ModifiedOn.GetValueOrDefault():yyyy/MM/dd HH:mm:ss}",
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

    private async Task GetClientTemplates()
    {
        var response = await Mediator.Send(new ListDocumentTemplates.Query(ClientId.GetValueOrDefault(0)));
        if (response.DocumentTemplates.Count > 0)
        {
            RetrievedClientTemplates = [.. response.DocumentTemplates];
        }
    }

    private async Task OnButtonClick(string action)
    {
        if (action == "continue")
        {
            NoCardSelected = SelectedCardItem == null;
            if (!NoCardSelected) 
            {
                if (IsChangingTemplate.GetValueOrDefault())
                {
                    _navigation.NavigateTo($"edit-hrdocument/{GetSelectedCardId()}?ClientId={ClientId}&HedgeRelationshipId={HedgeRelationshipId}&IsChangingTemplate={IsChangingTemplate}");
                }
                else
                {
                    await LocalStorage!.SetItemAsync(StringConstants.HedgeDocumentSelectedClientId, ClientId.GetValueOrDefault(0));
                    _navigation.NavigateTo($"create-hrdocument/{GetSelectedCardId()}?HedgeRelationshipId={HedgeRelationshipId}");
                }
            }
        }
        else
        {
            var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId.GetValueOrDefault(), string.Empty, string.Empty, "HAUI", 0);
            await Mediator.Send(updateHRCachedData);

            _navigation.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId.GetValueOrDefault()}");
        }
    }

    private void OnButtonClickHandler(CardItem card)
    {
        bool isSelected = card.IsSelected;

        SelectedCardItem = isSelected ? card : null;
        NoCardSelected = false;

        CardItemsListDp.ForEach(card => card.IsSelected = false);
        CardItemsListScratch.ForEach(card => card.IsSelected = false);
        CardItemsListClient.ForEach(card => card.IsSelected = false);

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
        else if (CardItemsListClient.Any(card => card.IsSelected))
        {
            return CardItemsListClient.Where(card => card.IsSelected).FirstOrDefault().DataSource.Id;
        }

        return null;
    }
}
