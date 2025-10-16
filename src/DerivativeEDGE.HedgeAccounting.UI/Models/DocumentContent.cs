namespace DerivativeEDGE.HedgeAccounting.UI.Models;

public class DocumentContent
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Enter the Document Content Name.")]
    public string Name { get; set; }

    public int Order { get; set; }

    public bool Required { get; set; }
}
