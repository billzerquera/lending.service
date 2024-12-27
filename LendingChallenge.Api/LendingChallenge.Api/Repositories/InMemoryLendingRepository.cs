using LendingChallenge.Api.Entities;

namespace LendingChallenge.Api.Repositories;

public class InMemoryLendingRepository : ILendingRepository
{
    //private readonly List<LoanOffer> _loanOffers = new List<LoanOffer>();
    //private readonly List<LoanOffer> _loanOffers = [];
    private readonly List<LoanOffer> _loanOffers = new()
    {
        new LoanOffer()
        {
            Id= 1,
            Balance= 7,
            Taxes= 0.2m,
            DueDate = DateTimeOffset.UtcNow
        },
        new LoanOffer()
        {
            Id= 2,
            Balance= 10,
            Taxes= 0.7m,
            DueDate = DateTimeOffset.UtcNow
        }
    };

    //private readonly List<Customer> _customers = [];
    private readonly List<Customer> _customers = new()
    {
        new Customer()
        {
            Id= 1,
            LoanOfferId = 1,
            PhoneNumber = "688777333"

        },
        new Customer()
        {
            Id= 2,
            PhoneNumber = "688777334"
        }
    };


    public async Task AddLoanOfferAsync(List<LoanOffer> listLoanOffer)
    {
        foreach (var item in listLoanOffer)
        {
            var loanOffer = _loanOffers.FirstOrDefault(lO => lO.Id == item.Id);
            if (loanOffer is null)
            {
                _loanOffers.Add(item);
            }
            else
            {
                loanOffer.Balance = item.Balance;
                loanOffer.Taxes = item.Taxes;
            }
        }
        await Task.CompletedTask;
    }
    public async Task<List<LoanOffer>> GetAllOffersAsync()
    {
        return await Task.FromResult(_loanOffers);
    }

    public async Task<LoanOffer?> GetLoanOfferByIdAsync(int id)
    {
        return await Task.FromResult(_loanOffers.Find(lO => lO.Id == id));
    }

    public async Task<Customer?> GetCustomerByPhoneBumberAsync(string phoneNumber)
    {
        return await Task.FromResult(_customers.Find(cust => cust.PhoneNumber == phoneNumber));
    }
}
