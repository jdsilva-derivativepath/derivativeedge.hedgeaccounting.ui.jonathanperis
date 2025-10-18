using System.ComponentModel.DataAnnotations;
using DerivativeEdge.HedgeAccounting.Api.Client;

namespace DerivativeEDGE.HedgeAccounting.UI.Features.HedgeRelationships.Models;

/// <summary>
/// Validation model for Amortization dialog form.
/// Wraps the API model to add validation attributes matching legacy system behavior.
/// </summary>
public class AmortizationFormModel
{
    public long ID { get; set; }
    
    [Required(ErrorMessage = "The GL Account field is required.")]
    [Range(1, long.MaxValue, ErrorMessage = "The GL Account field is required.")]
    public long GLAccountID { get; set; }
    
    [Required(ErrorMessage = "The Contra Account field is required.")]
    [Range(1, long.MaxValue, ErrorMessage = "The Contra Account field is required.")]
    public long ContraAccountID { get; set; }
    
    [Required(ErrorMessage = "Start Date is required.")]
    public DateTime? StartDate { get; set; }
    
    [Required(ErrorMessage = "End Date is required.")]
    public DateTime? EndDate { get; set; }
    
    [Required(ErrorMessage = "Front Roll Date is required.")]
    public DateTime? FrontRollDate { get; set; }
    
    [Required(ErrorMessage = "Back Roll Date is required.")]
    public DateTime? BackRollDate { get; set; }
    
    // Non-required fields
    public List<string>? FinancialCenters { get; set; }
    public DerivativeEDGEDomainEntitiesEnumsPaymentFrequency? PaymentFrequency { get; set; }
    public DerivativeEDGEDomainEntitiesEnumsDayCountConv? DayCountConv { get; set; }
    public DerivativeEDGEDomainEntitiesEnumsPayBusDayConv? PayBusDayConv { get; set; }
    public double? TotalAmount { get; set; }
    public bool AdjDates { get; set; }
    public bool Straightline { get; set; }
    public bool IncludeInRegression { get; set; }
    
    /// <summary>
    /// Converts this validation model to the API model for submission.
    /// </summary>
    public DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM ToApiModel()
    {
        return new DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM
        {
            ID = this.ID,
            GLAccountID = this.GLAccountID,
            ContraAccountID = this.ContraAccountID,
            StartDate = this.StartDate?.ToString("MM/dd/yyyy"),
            EndDate = this.EndDate?.ToString("MM/dd/yyyy"),
            FrontRollDate = this.FrontRollDate?.ToString("MM/dd/yyyy"),
            BackRollDate = this.BackRollDate?.ToString("MM/dd/yyyy"),
            PaymentFrequency = this.PaymentFrequency ?? DerivativeEDGEDomainEntitiesEnumsPaymentFrequency.Monthly,
            DayCountConv = this.DayCountConv ?? DerivativeEDGEDomainEntitiesEnumsDayCountConv.Actual360,
            PayBusDayConv = this.PayBusDayConv ?? DerivativeEDGEDomainEntitiesEnumsPayBusDayConv.Following,
            TotalAmount = this.TotalAmount ?? 0,
            AdjDates = this.AdjDates,
            Straightline = this.Straightline,
            IncludeInRegression = this.IncludeInRegression
        };
    }
    
    /// <summary>
    /// Creates a validation model from the API model.
    /// </summary>
    public static AmortizationFormModel FromApiModel(DerivativeEDGEHAApiViewModelsHedgeRelationshipOptionTimeValueAmortVM apiModel)
    {
        return new AmortizationFormModel
        {
            ID = apiModel.ID,
            GLAccountID = apiModel.GLAccountID,
            ContraAccountID = apiModel.ContraAccountID,
            StartDate = !string.IsNullOrEmpty(apiModel.StartDate) ? DateTime.Parse(apiModel.StartDate) : null,
            EndDate = !string.IsNullOrEmpty(apiModel.EndDate) ? DateTime.Parse(apiModel.EndDate) : null,
            FrontRollDate = !string.IsNullOrEmpty(apiModel.FrontRollDate) ? DateTime.Parse(apiModel.FrontRollDate) : null,
            BackRollDate = !string.IsNullOrEmpty(apiModel.BackRollDate) ? DateTime.Parse(apiModel.BackRollDate) : null,
            PaymentFrequency = apiModel.PaymentFrequency,
            DayCountConv = apiModel.DayCountConv,
            PayBusDayConv = apiModel.PayBusDayConv,
            TotalAmount = apiModel.TotalAmount,
            AdjDates = apiModel.AdjDates,
            Straightline = apiModel.Straightline,
            IncludeInRegression = apiModel.IncludeInRegression
        };
    }
}
