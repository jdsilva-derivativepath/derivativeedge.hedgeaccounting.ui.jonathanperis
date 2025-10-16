using DerivativeEDGE.HedgeAccounting.UI.Shared.Layout;

namespace DerivativeEDGE.HedgeAccounting.UI.Pages.HedgeRelationship;

public partial class PreviewHrDocument
{
    [CascadingParameter]
    public EmptyLayout Layout { get; set; }

    [SupplyParameterFromQuery]
    public long HedgeRelationshipId { get; set; }

    [SupplyParameterFromQuery]
    public long? ClientId { get; set; }

    private PreviewHrDocumentDialog _previewHrDocumentDialogComponent;

    [Inject]
    private IMediator MediatorService { get; set; } = default!;

    [Inject]
    private IAlertService AlertService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ILocalStorageService LocalStorage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var request = new GetHrDocument.Query(HedgeRelationshipId, Guid.Empty);
            var result = await MediatorService.Send(request);

            if (result is null)
            {
                await AlertService.ShowToast("HR document template does not exist will redirect back to HR screen.",
                    AlertKind.Error, "Error", showButton: true);

                await Task.Delay(5000);

                var updateHRCachedData = new UpdateHRCachedData.Command(HedgeRelationshipId, string.Empty, string.Empty, "HAUI", 0);
                await MediatorService.Send(updateHRCachedData);


                NavigationManager.NavigateTo($"/HedgeAccounting/HedgeRelationship?id={HedgeRelationshipId}");
                return;
            }

            _previewHrDocumentDialogComponent.FormatContent(ClientId.GetValueOrDefault(),
                result.RelationshipDocumentContents.DocumentName,
                [.. result.RelationshipDocumentContents.HedgeRelationshipDocumentContents
                .Select(v => new DocumentTemplateDetail()
                {
                    Hidden = v.Hidden,
                    HedgeDocumentContentId = Guid.Empty,
                    HtmlBody = v.HtmlBody,
                    Id = Guid.Empty,
                    Name = v.ContentName,
                    Order = v.Order
                })],
                result.RelationshipDocumentContents.HRDeDesignated);

            await LocalStorage!.SetItemAsync(StringConstants.HedgeDocumentSelectedClientId, ClientId.GetValueOrDefault(0));
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast("There was a problem retrieving hr template.", AlertKind.Error,
                                "Error", showButton: true);
        }

        await base.OnInitializedAsync();
    }
}
