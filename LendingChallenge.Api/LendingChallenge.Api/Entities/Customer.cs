namespace LendingChallenge.Api.Entities;

public class Customer
{
    public int Id { get; set; }
    public int LoanOfferId { get; set; }
    public LoanOffer? LoanOffer { get; set; }
    public required string PhoneNumber { get; set; }

}
