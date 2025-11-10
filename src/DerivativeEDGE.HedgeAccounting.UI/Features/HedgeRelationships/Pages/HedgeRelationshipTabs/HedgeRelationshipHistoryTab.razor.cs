namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class HedgeRelationshipHistoryTab
{
    #region Parameters
    [Parameter] public ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM> HedgeRelationshipActivities { get; set; }
    [Parameter] public EventCallback<ICollection<DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM>> HedgeRelationshipActivitiesChanged { get; set; }
    #endregion

    public List<DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM> HistoryActivities { get; set; } = [];

    protected override async Task OnParametersSetAsync()
    {
        LoadHistoryData();
        await InvokeAsync(StateHasChanged);
    }

    private void LoadHistoryData()
    {
        if (HedgeRelationshipActivities == null || HedgeRelationshipActivities.Count == 0)
        {
            HistoryActivities = [];
            return;
        }

        // Legacy: data-ng-repeat="a in Model.HedgeRelationshipActivities"
        // Filter by Enabled and order by CreatedOn descending to match legacy behavior
        HistoryActivities = [.. HedgeRelationshipActivities
            .Where(activity => activity.Enabled)
            .OrderByDescending(activity => activity.CreatedOn)];
    }

    private string GetActivityTypeDisplayText(DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM activity)
    {
        // First try to use ActivityTypeText if it's available and not null/empty
        if (!string.IsNullOrEmpty(activity.ActivityTypeText))
        {
            return activity.ActivityTypeText;
        }

        // Fallback to enum conversion with proper formatting based on your actual enum values
        return activity.ActivityType switch
        {
            DerivativeEDGEHAEntityEnumActivityType.None => "None",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipCreated => "Relationship Created",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipDrafted => "Relationship Drafted",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipRedrafted => "Relationship Re-drafted",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipDesignated => "Relationship Designated",
            DerivativeEDGEHAEntityEnumActivityType.UserRegression => "User Regression",
            DerivativeEDGEHAEntityEnumActivityType.GeneratedInceptionPackage => "Generated Inception Package",
            DerivativeEDGEHAEntityEnumActivityType.BackloadRegression => "Backload Regression",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipDedesignated => "Relationship De-designated",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipUpdated => "Relationship Updated",
            DerivativeEDGEHAEntityEnumActivityType.PeriodicRegression => "Periodic Regression",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipRedesignated => "Relationship Re-designated",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipLinkTrade => "Relationship Link Trade",
            _ => activity.ActivityType.ToString()
        };
    }

    private string GetDisplayDate(DerivativeEDGEHAApiViewModelsHedgeRelationshipActivityVM activity)
    {
        // Legacy: {{a.CreatedOn | date: 'MMMM dd, yyyy'}} at {{a.CreatedOn | date: 'h:mm a'}}
        if (activity.CreatedOn != DateTimeOffset.MinValue)
        {
            return activity.CreatedOn.ToString("MMMM dd, yyyy 'at' h:mm tt");
        }

        // Fallback to current date
        return DateTimeOffset.Now.ToString("MMMM dd, yyyy 'at' h:mm tt");
    }

    private string GetActivityIcon(DerivativeEDGEHAEntityEnumActivityType activityType)
    {
        // Legacy: data-ng-class="{'fa-briefcase':a.ActivityTypeEnum==='BackloadRegression' || a.ActivityTypeEnum==='RelationshipDesignated',
        //                         'fa-line-chart':a.ActivityTypeEnum==='UserRegression'||a.ActivityTypeEnum==='PeriodicRegression',
        //                         'fa-check':a.ActivityTypeEnum==='RelationshipCreated'||a.ActivityTypeEnum==='RelationshipUpdated'}"
        return activityType switch
        {
            DerivativeEDGEHAEntityEnumActivityType.BackloadRegression => "fa-briefcase",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipDesignated => "fa-briefcase",
            DerivativeEDGEHAEntityEnumActivityType.UserRegression => "fa-line-chart",
            DerivativeEDGEHAEntityEnumActivityType.PeriodicRegression => "fa-line-chart",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipCreated => "fa-check",
            DerivativeEDGEHAEntityEnumActivityType.RelationshipUpdated => "fa-check",
            _ => "fa-circle-info" // Default icon for other types
        };
    }
}
