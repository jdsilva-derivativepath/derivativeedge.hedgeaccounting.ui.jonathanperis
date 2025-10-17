namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Pages.HedgeRelationshipTabs;

public partial class AccountingDetailsTab
{
    private bool IsChecked { get; set; } = true;
    private string SelectedStandardValue { get; set; } = "None";

    public class StandardOption
    {
        public string ID { get; set; }
        public string Text { get; set; }
    }

    private List<StandardOption> StandardOptions = new List<StandardOption>()
    {
        new(){ ID= "None", Text="None" },
        new(){ ID= "ASC815", Text="ASC815" },
    };
}
