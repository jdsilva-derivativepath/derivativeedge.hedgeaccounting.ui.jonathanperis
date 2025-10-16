namespace DerivativeEDGE.HedgeAccounting.UI.Constants;

public static class StringConstants
{
    public static readonly List<string> ClientContractTypes = ["Saas", "SwaS"];

    //Hedge Accounting Messages
    public static readonly string APICallFailed = "Call to Hedge Accounting API failed! URL={0}";

    //Cache key names
    public static readonly string HedgeDocumentSelectedClientId = "HedgeDocumentSelectedClientId";

    public static readonly string HedgeRelationshipSelectedClientId = "HedgeRelationshipSelectedClientId";

    //Hedge Document Template active view grid
    public static readonly string HedgeDocumentTemplateActiveView = "HedgeDocumentTemplateActiveView";

    public static readonly string HedgeDocumentTemplateMode = "HedgeDocumentTemplateMode";

    public static readonly string HrDocumentMode = "HrDocumentMode";
}
