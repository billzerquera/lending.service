using LendingChallenge.Api.Entities;
using LendingChallenge.Api.Repositories;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;
using static LendingChallenge.Api.DTOs;

namespace LendingChallenge.Api.Endpoints;

public static class LendingEndpoints
{
    public static RouteGroupBuilder MapLendingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/");

        // GET /status
        group.MapGet("status", () => "The service is ready to receive requests.");

        // POST /offers 
        group.MapPost("/offers", async (ILendingRepository _repo, HttpRequest request, HttpResponse response) =>
        {
            // Validar encabezado Content-Type
            if (!request.ContentType?.Equals("application/json", StringComparison.OrdinalIgnoreCase) ?? true)
            {
                response.Headers.Append("X-Validation-Errors", "InvalidContentType");
                return Results.BadRequest(new { message = "Invalid Content-Type. Expected 'application/json'." });
            }

            // Leer y validar el cuerpo de la solicitud
            string requestBody;
            using (var reader = new StreamReader(request.Body))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            JsonElement jsonPayload;
            try
            {
                jsonPayload = JsonSerializer.Deserialize<JsonElement>(requestBody);
            }
            catch (JsonException)
            {
                response.Headers.Append("X-Validation-Errors", "InvalidJsonFormat");
                return Results.BadRequest(new { message = "Invalid JSON format." });
            }

            // Validate that the body is an array
            if (jsonPayload.ValueKind != JsonValueKind.Array)
            {
                response.Headers.Append("X-Validation-Errors", "PayloadMustBeJSONArray");
                return Results.BadRequest(new { message = "The payload must be a JSON array." });
            }

            var validationErrors = new List<string>();
            var listLoanOffer = new List<LoanOffer>();

            foreach (var element in jsonPayload.EnumerateArray())
            {
                if (!element.TryGetProperty("id", out var idElement) || idElement.ValueKind != JsonValueKind.Number)
                {
                    response.Headers.Append("X-Validation-Errors", "MustBeValidID (integer)");
                    validationErrors.Add("Each offer must include a valid 'id' (integer).");
                    continue;
                }

                if (!element.TryGetProperty("Balance", out var balanceElement) || balanceElement.ValueKind != JsonValueKind.Number)
                {
                    response.Headers.Append("X-Validation-Errors", "MustIncludeBalance (integer or decimal)");
                    validationErrors.Add($"Offer with Id {idElement} must include a valid 'Balance' (integer or decimal).");
                    continue;
                }

                if (!element.TryGetProperty("Taxes", out var taxesElement) || taxesElement.ValueKind != JsonValueKind.Number)
                {
                    response.Headers.Append("X-Validation-Errors", "MustBeValidTaxes (decimal)");
                    validationErrors.Add($"Offer with Id {idElement} must include a valid 'Taxes' (decimal).");
                    continue;
                }

                var loanOffer = new LoanOffer
                {
                    Id = idElement.GetInt32(),
                    Balance = balanceElement.GetDecimal(),
                    Taxes = taxesElement.GetDecimal(),
                    //DueDate = new DateTimeOffset().DateTime
                    DueDate = DateTimeOffset.UtcNow

                };

                // Validating Data Annotations
                var validationResults = new List<ValidationResult>();
                var validationContext = new ValidationContext(loanOffer);

                if (!Validator.TryValidateObject(loanOffer, validationContext, validationResults, true))
                {
                    validationErrors.AddRange(validationResults.Select(vr => vr.ErrorMessage ?? string.Empty));
                    continue;
                }

                listLoanOffer.Add(loanOffer);
            }

            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { errors = validationErrors });
            }


            await _repo.AddLoanOfferAsync(listLoanOffer);
            return Results.Ok();
        });

        // GET /offers
        group.MapGet("/get_offers", async (ILendingRepository _repo) => await _repo.GetAllOffersAsync());

        //POST /customers/{msisdn}/loans
        group.MapPost("/customers/{msisdn}/loans", async (ILendingRepository _repo, HttpRequest request, string msisdn) =>
        {
            var form = await request.ReadFormAsync();
            var offerId = form["ID"];

            if (string.IsNullOrEmpty(offerId))
            {
                return Results.BadRequest("Offer ID required.");
            }

            var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
            if (!phoneRegex.IsMatch(msisdn))
            {
                return Results.BadRequest(new { message = "'msisdn' must be a valid phone number." });
            }

            var offer = await _repo.GetLoanOfferByIdAsync(int.Parse(offerId!));
            if (offer is null)
            {
                return Results.BadRequest();
            }
            var customer = await _repo.GetCustomerByPhoneBumberAsync(msisdn);

            if (customer?.LoanOfferId is null)
            {
                var loanOffer = new LoanOffer
                {
                    Id = offer!.Id,
                    Balance = offer.Balance,
                    Taxes = offer.Taxes,
                    DueDate = offer.DueDate
                };

                var responseDto = new OfferByPhoneNumberDto
                (
                    loanOffer.Id,
                    loanOffer.Balance,
                    loanOffer.DueDate
                );

                return Results.Ok(responseDto);
            }

            else
            {
                return Results.Conflict();
            }

        });

        //PUT /customers/{msisdn}/loans
        group.MapPut("/customers/{msisdn}/loans", async (ILendingRepository _repo, HttpRequest request, string msisdn) =>
        {
            var form = await request.ReadFormAsync();
            var topUpAmount = form["TopUp"];

            if (string.IsNullOrEmpty(topUpAmount))
            {
                return Results.BadRequest("Top Up amount required.");
            }

            var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
            if (!phoneRegex.IsMatch(msisdn))
            {
                return Results.BadRequest(new { message = "'msisdn' must be a valid phone number." });
            }

            var customer = await _repo.GetCustomerByPhoneBumberAsync(msisdn);
            if (customer is null)
                return Results.NotFound();
            var offer = await _repo.GetLoanOfferByIdAsync(customer.LoanOfferId);


            if (customer is not null && offer is not null)
            {
                var loanOffer = new LoanOffer
                {
                    Id = offer.Id,
                    Balance = offer.Balance,
                    Taxes = offer.Taxes,
                    DueDate = offer.DueDate
                };

                decimal sumBalanceTaxes = loanOffer.Balance + loanOffer.Taxes;

                if (decimal.Parse(topUpAmount!) > sumBalanceTaxes)
                {
                    var responseDto = new RepaidOfferByPhoneNumberDto
                    (
                        sumBalanceTaxes
                    );
                    return Results.Ok(responseDto);
                }
                else
                {
                    var responseDto = new RepaidOfferByPhoneNumberDto
                    (
                        decimal.Parse(topUpAmount!)
                    );
                    return Results.Ok(responseDto);
                }
            }

            else
            {
                return Results.NoContent();
            }

        });

        //GET /customers/{msisdn}/loans
        group.MapGet("/customers/{msisdn}/loans", async (ILendingRepository _repo, HttpRequest request, string msisdn) =>
        {
            var phoneRegex = new Regex(@"^\+?[1-9]\d{1,14}$");
            if (!phoneRegex.IsMatch(msisdn))
            {
                return Results.BadRequest(new { message = "'msisdn' must be a valid phone number." });
            }

            var customer = await _repo.GetCustomerByPhoneBumberAsync(msisdn);
            if (customer is null)
                return Results.NotFound();

            var offer = await _repo.GetLoanOfferByIdAsync(customer.LoanOfferId);

            if (customer is not null && offer is not null)
            {
                var loanOffer = new LoanOffer
                {
                    Id = offer.Id,
                    Balance = offer.Balance,
                    Taxes = offer.Taxes,
                    DueDate = offer.DueDate
                };

                decimal sumBalanceTaxes = loanOffer.Balance + loanOffer.Taxes;
                var responseDto = new Customer_OfferByPhoneNumberDto
                (
                    sumBalanceTaxes,
                    offer.DueDate,
                    new OfferDto
                    (
                        offer.Balance,
                        offer.Taxes
                    )
                );

                return Results.Ok(responseDto);
            }

            else
            {
                return Results.NoContent();
            }

        });

        return group;
    }
}
