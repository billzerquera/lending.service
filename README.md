# Lending Challenge API (DOCUMENTATION)

## Description

This API is part of the Lending Challenge. It provides endpoints to manage loan offers and customers.

## Requisitos

- .NET > 6.0 SDK 
- InMemoryLendingRepository

## Instalation

1. Clona el repositorio:
    ```bash
    git clone <URL_DEL_REPOSITORIO>
    ```
2. Navega al directorio del proyecto:
    ```bash
    cd LendingChallenge.Api
    ```
3. Restaura los paquetes NuGet:
    ```bash
    dotnet restore
	
	<PackageReference Include="MinimalApis.Extensions" Version="0.11.0" />
    ```
4. Ejecuta la aplicación:
    ```bash
    dotnet run
    ```

## Endpoints

** All endpoints in this API are implemented using asynchronous methods.
** This is done to improve the efficiency and responsiveness of the service, especially when handling multiple simultaneous requests - [$`10^4`$ / $`10^5`$ customers]
** Asynchronous programming allows the server to handle other tasks while waiting for the completion of I/O operations (databases or external services).

## Dependecy Injections in Program.cs

```chsarp
/*
In this project, we use dependency injection to register and resolve our dependencies. 
Specifically, was registered `ILendingRepository` with a concrete implementation `InMemoryLendingRepository` as a singleton service.

--> builder.Services.AddSingleton<InMemoryLendingRepository>();

Guaranteeing:

1. Decoupling
2. Facilitates Testing
3. Life Cycle Control
4. Flexibility
5. Centralized Configuration
*/
 
Also was registered the method 

--> app.MapLendingEndpoints(); 

Guaranteeing:

1.Code Organization
2.Reusability and Modularity
3.Maintainability
4.Flexibility
5.Facilitates Dependency Injection into any method of the API endpoints

*/



### GET /status

Developed an endpoint that retrieve a 200 response with a successfull message body 

```csharp

group.MapGet("status", () => "The service is ready to receive requests.");

```

### POST /offers

Developed an asynchronous endpoint  

```csharp

group.MapPost("/offers", async (InMemoryLendingRepository _repo, HttpRequest request, HttpResponse response) =>{})

```

*Different in this endpoint: for each error validation of both the body and the json parameters I included them in an "X-Validation-Errors" header in the expected Headers*


### POST /customers/{msisdn}/loans

```csharp

group.MapPost("/customers/{msisdn}/loans", async (InMemoryLendingRepository _repo, HttpRequest request, string msisdn) =>{})

```

*Different in this endpoint: Using the dto approach to display the required response in the body, in this case the OfferByPhoneNumberDto record

### PUT /customers/{msisdn}/loans

```csharp

group.MapPut("/customers/{msisdn}/loans", async (InMemoryLendingRepository _repo, HttpRequest request, string msisdn) =>{})

```
*Different in this endpoint: Using the dto approach to display the required response in the body, in this case the RepaidOfferByPhoneNumberDto record

### GET /customers/{msisdn}/loans

```csharp

group.MapGet("/customers/{msisdn}/loans", async (InMemoryLendingRepository _repo, HttpRequest request, string msisdn) =>{})

```

*Different in this endpoint: Using the dto approach to display the required response in the body, in this case the Customer_OfferByPhoneNumberDto record



## Repository in Memory

For this project, an `InMemoryLendingRepository` class is used which implements the ILendingRepository interface that stores all data in-memory. 
This fulfills the requirement of not using a persistent database and allows for fast and efficient operations.

### Implementation Example

```csharp

public class InMemoryLendingRepositories
{
    private readonly List<LoanOffer> loanOffers = new();
    private readonly List<Customer> customers = new();

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

--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

# Lending Service challenge
Design/implement a system to manage loans.

You have been assigned to build the lending service that will be used to disburse and reimburse loans to a customer's balance.

Loans are related to a loan offer that specifies the balance disbursed to the customerand the taxes applied.

Customers request a loan related to an offer. It's not possible a customer have more than one active (pending to be repaid) loan.

Once the loan has been approvisioned, an Online Charging System (OCS) can notify the service customer's balance has received a top up
so repayment (fully or partial) should be challenged.

# API
This service must provide a REST API which will be used to interact with it.

This API must comply with the following contract:

## GET /status
Indicate the service has stated up correctly and is ready to accept requests.

Responses:
- **200 OK** When the service is ready to receive requests.

## POST /offers
Load offers to the offers set. If the offer matches any existing *Id*, it should be updated.
This method may be called more than once during the life cycle of the service.

**Body** *required* The list of offers to add.

**Content Type** ```application/json```
Sample:
```
[
  {
    "id": 1,
    "Balance": 7,
    "Taxes": 0.2
  },
  {
    "id": 2,
    "Balance": 10,
    "Taxes": 0.7
  }
]
```
Responses:
- **200 OK** When the offers are added properly.
- **400 Bad Request** When there is a failure in the request format, exepcted headers, or the payload cannot be unmarshaled.

## POST /customers/{msisdn}/loans
Given an subscriber number (phone number) such that ```688777333``` and an offer ID such that ```ID=X```, return the balance left and the due date.

**Parameters** *required* Phone number used to identify the customer.

**Body** *required* An url encoded form with the offer ID such that ```ID=X```.

**Content Type** ```application/x-www-form-urlencoded```

**Accept** ```application/json```

Responses:
- **200 OK** With the data of the loan if the customer has not any active loan. See below for the expected loan representation
```
{
  "id": 1,
  "balanceLeft": 7.2,
  "dueDate": "2024-06-01T10:05:00+02:00"
}
```
- **400 Bad Request** When the specified offer does not exist.
- **409 Conflict** When the customer already has an active loan.

## PUT /customers/{msisdn}/loans
Given an subscriber number (phone number) such that ```688777333``` and a top up amount, repay total or partially the active loan if any.

**Parameters** *required* Phone number used to identify the customer.

**Body** *required* An url encoded form with the amount of the top up such that ```TopUp=10```.

**Content Type** ```application/x-www-form-urlencoded```

**Accept** ```application/json```

Responses:
- **200 OK** With the amount used to repay the active loan lile ```Repaid=7.2```.
- **204 No Content** When the customer has not an active loan.
- **404 Not Found** The customer is not registered on the system.

## GET /customers/{msisdn}/loans
Given an subscriber number (phone number) such that ```688777333```, return the customer's active loan if any.

**Parameters** *required* Phone number used to identify the customer.

**Accept** ```application/json```

Responses:
- **200 OK** With the data of the active loan. See below for the expected loan representation
```
{
  "balanceLeft": 7.2,
  "dueDate": "2024-06-01T10:05:00+02:00",
  "offer": {
    "balance": 7,
    "taxes: 0.2
  }
}
```
- **204 No Content** When the customer has not an active loan.
- **404 Not Found** The customer is not registered on the system.

# Tooling
At MyneHub, we use Github for our backend development work. Anyways, please fork this repository and use the environment you feel more comfortable,
but please bear in ming we want to see your best!

# Requirements
- The service should be as efficient as possible. It should be able to work reasonably well with at least $`10^4`$ / $`10^5`$ customers.
  Explain how you did achieve this requirement.
- Document your decissions using PRs or in this very README adding sections to it,
  the same way you would be generating documentation for any other deliverable.
- It is supposed the service will run in-memory. In case you need any repository, please justify it.




