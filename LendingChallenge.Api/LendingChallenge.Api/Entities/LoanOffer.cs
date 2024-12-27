namespace LendingChallenge.Api.Entities;

public class LoanOffer
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public decimal Taxes { get; set; }
    public DateTimeOffset DueDate { get; set; }
}
