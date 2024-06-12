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
