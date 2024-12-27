using LendingChallenge.Api.Entities;

namespace LendingChallenge.Api.Repositories
{
    public interface ILendingRepository
    {
        Task AddLoanOfferAsync(List<LoanOffer> listLoanOffer);
        Task<List<LoanOffer>> GetAllOffersAsync();
        Task<Customer?> GetCustomerByPhoneBumberAsync(string phoneNumber);
        Task<LoanOffer?> GetLoanOfferByIdAsync(int id);
    }
}