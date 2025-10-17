using DerivativeEdge.HedgeAccounting.Api.Client;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Commands;
using DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Handlers.Queries;
using Microsoft.JSInterop;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages;

public partial class HedgeRelationshipCreate
{
    public bool ShowNewProcessModal { get; set; }
    private List<Client> AvailableClients { get; set; } = new();
    private List<Entity> AvailableEntities { get; set; } = new();
    public DerivativeEDGEHAEntityHedgeRelationship HedgeRelationship { get; set; } = new();

    #region Loading States
    public bool IsLoadingClients { get; set; }
    public bool IsLoadingEntities { get; set; }
    #endregion

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(LoadClientsAsync());
        StateHasChanged();
    }

    private void OnClickCancelHandler()
    {
        HideFormMessage();
        ShowNewProcessModal = false;
    }

    private async Task OnTriggerSubmitFromHeader()
    {
        await IJSRuntime.InvokeVoidAsync("triggerHiddenSubmit");
    }

    private async Task RequestNewHedgeRelationship(EditContext context)
    {
        HideFormMessage();

        // Manual validation: Entity selection
        var selectedEntity = AvailableEntities
            .FirstOrDefault(e => e.EntityId == HedgeRelationship.BankEntityID);

        if (HedgeRelationship.BankEntityID == 0 ||
            string.Equals(selectedEntity?.EntityLongName, "None", StringComparison.OrdinalIgnoreCase))
        {
            ShowCustomMessage(
                "Please fix the following errors:",
                "Please select a valid entity.",
                MapFormMessageTypeToSeverity(FormMessageType.ValidationError)
            );
            return; // Prevent submit
        }

        // Manual validation: DedesignationDate > DesignationDate
        if (HedgeRelationship.DedesignationDate <= HedgeRelationship.DesignationDate)
        {
            ShowCustomMessage(
                "Please fix the following errors:",
                "Dedesignation Date must be later than Designation Date",
                MapFormMessageTypeToSeverity(FormMessageType.ValidationError)
            );
            return; // Prevent submit
        }

        // Proceed with creating the relationship
        var result = await Mediator.Send(new CreateHedgeRelationship.Command(HedgeRelationship));
        if (!result.HasError)
        {
            ShowNewProcessModal = false;
            NavManager.NavigateTo($"{NavManager.BaseUri}hedgerelationship?id={result.Data.ID}");
        }
        else
        {
            await AlertService.ShowToast(result.Message, AlertKind.Error, "Failed", showButton: true);
        }
    }

    private void OnInvalidSubmit(EditContext context)
    {
        ShowFormMessage(FormMessageType.ValidationError);
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            IsLoadingClients = true;
            StateHasChanged();

            var query = new GetClients.Query();
            var response = await Mediator.Send(query, CancellationToken.None);
            response.Clients.Insert(0, new Client { ClientId = 0, ClientName = "None" });
            AvailableClients = response.Clients;

            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"There was a problem retrieving the Client List: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsLoadingClients = false;
        }
    }

    public async Task LoadClientEntitiesAsync(long? clientId)
    {
        try
        {
            IsLoadingEntities = true;

            // Clear entities if no client is selected (for DPI users)
            if (clientId == null || clientId == 0)
            {
                AvailableEntities = new List<Entity>();
                HedgeRelationship.BankEntityID = 0; // reset selection
                return;
            }

            var query = new GetClientEntities.Query(clientId);
            AvailableEntities = new List<Entity>();
            var response = await Mediator.Send(query, CancellationToken.None);

            response.Entities.Insert(0, new Entity { EntityId = 0, EntityLongName = "None" }); // Insert "None" option
            AvailableEntities = response.Entities;

            // If ClientID corresponds to "None" (EntityId = 0), reset BankEntityID
            if (clientId == 0 || string.Equals(
                    AvailableEntities.FirstOrDefault(e => e.EntityId == clientId)?.EntityLongName,
                    "None", StringComparison.OrdinalIgnoreCase))
            {
                HedgeRelationship.BankEntityID = 0;
            }
            else
            {
                var firstEntity = AvailableEntities.FirstOrDefault(e => e.EntityId != 0);
                HedgeRelationship.BankEntityID = firstEntity.EntityId;
            }

            SelectDefaultComboBoxValues();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await AlertService.ShowToast($"There was a problem retrieving the Entities List: {ex.Message}", AlertKind.Error, "Error", showButton: true);
        }
        finally
        {
            IsLoadingEntities = false;
        }
    }


    private void SelectDefaultComboBoxValues()
    {
        // Set default values for dropdowns using their respective enum types
        HedgeRelationship.HedgeRiskType = (DerivativeEDGEHAEntityEnumHedgeRiskType)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHedgeRiskType)).GetValue(0);
        HedgeRelationship.HedgeType = (DerivativeEDGEHAEntityEnumHRHedgeType)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHRHedgeType)).GetValue(0);
        HedgeRelationship.HedgedItemType = (DerivativeEDGEHAEntityEnumHedgedItemType)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHedgedItemType)).GetValue(0);
        HedgeRelationship.AssetLiability = (DerivativeEDGEHAEntityEnumAssetLiability)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumAssetLiability)).GetValue(0);
        HedgeRelationship.Standard = (DerivativeEDGEHAEntityEnumStandard)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumStandard)).GetValue(0);
        HedgeRelationship.HedgingInstrumentStructure = (DerivativeEDGEHAEntityEnumHedgingInstrumentStructure)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHedgingInstrumentStructure)).GetValue(0);
        HedgeRelationship.AmortizationMethod = (DerivativeEDGEHAEntityEnumAmortizationMethod)Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumAmortizationMethod)).GetValue(0);
        HedgeRelationship.OptionPremium = 0;
    }

    public IEnumerable<DropdownModel<TEnum>> GetDropdownDatasource<TEnum>() where TEnum : struct, Enum
    {
        return Enum.GetValues(typeof(TEnum))
            .Cast<TEnum>()
            .Select(value => new DropdownModel<TEnum>
            {
                Value = value,
                Text = GetEnumDescription(value)
            });
    }

    public IEnumerable<DropdownModel> GetDropdownDatasource(string dataSet = "hedgedrisk")
    {
        switch (dataSet.ToLower())
        {
            case "hedgerisktype":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHedgeRiskType))
                    .Cast<DerivativeEDGEHAEntityEnumHedgeRiskType>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            case "hedgetype":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHRHedgeType))
                    .Cast<DerivativeEDGEHAEntityEnumHRHedgeType>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            case "hedgeditemtype":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHedgedItemType))
                    .Cast<DerivativeEDGEHAEntityEnumHedgedItemType>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            case "assetliability":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumAssetLiability))
                    .Cast<DerivativeEDGEHAEntityEnumAssetLiability>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            case "standard":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumStandard))
                    .Cast<DerivativeEDGEHAEntityEnumStandard>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            case "hedginginstrumentstructure":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumHedgingInstrumentStructure))
                    .Cast<DerivativeEDGEHAEntityEnumHedgingInstrumentStructure>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            case "amortizationmethod":
                return Enum.GetValues(typeof(DerivativeEDGEHAEntityEnumAmortizationMethod))
                    .Cast<DerivativeEDGEHAEntityEnumAmortizationMethod>()
                    .Select(value => new DropdownModel
                    {
                        Value = value.ToString(),
                        Text = GetEnumDescription(value)
                    });

            default:
                throw new ArgumentException("Invalid data set");
        }
    }

    private string GetEnumDescription<TEnum>(TEnum value) where TEnum : Enum
    {
        // Get attribute description if available, otherwise use the enum value name
        var field = value.GetType().GetField(value.ToString());
        var attributes = (EnumDescriptionAttribute[])field.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);

        return attributes.Length > 0
            ? attributes[0].Description
            : value.ToString();
    }

    public class DropdownModel
    {
        public string Text { get; set; }
        public string Value { get; set; }
    }

    public class DropdownModel<TEnum> where TEnum : struct, Enum
    {
        public string Text { get; set; }
        public TEnum Value { get; set; }
    }

    private async Task HandleClientValueChangeAsync()
    {
        AvailableEntities = new List<Entity>();
        await LoadClientEntitiesAsync(HedgeRelationship.ClientID);
    }
}
