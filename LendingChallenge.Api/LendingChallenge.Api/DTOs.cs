namespace LendingChallenge.Api;

public class DTOs
{
    public record OfferByPhoneNumberDto
    (
        int Id,
        decimal BalanceLeft,
        DateTimeOffset DueDate
    ); 
    
    public record RepaidOfferByPhoneNumberDto
    (
        decimal Repaid
    );

    public record OfferDto
    (
        decimal Balance,
        decimal Taxes
    );

    public record Customer_OfferByPhoneNumberDto
    (
        decimal BalanceLeft,
        DateTimeOffset DueDate,
        OfferDto Offer
    );
}
