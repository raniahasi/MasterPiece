using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;

namespace WeCartFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        public PaymentController()
        {
            StripeConfiguration.ApiKey = "sk_test_51Q1PVHP61isbCLOjS7LMjFpcgFwAKjQnzW9K6Pd4sde7Ngkh2uR5TcR9MsfMUsADsyls7DI7RGiY3p1egXBcAHXo00y4TY6IrF";
        }

        [HttpPost("create-checkout-session")]
        public ActionResult CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            try
            {
                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.ProductName,
                        },
                        UnitAmount = request.Amount,
                    },
                    Quantity = 1,
                },
            },
                    Mode = "payment",
                    SuccessUrl = request.SuccessUrl,
                    CancelUrl = request.CancelUrl,
                };

                var service = new SessionService();
                Session session = service.Create(options);
                return Ok(new { sessionId = session.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // More detailed error to help diagnose issues
            }
        }
















        public class PaymentRequest
        {
            public string ProductName { get; set; }
            public long Amount { get; set; }
            public string SuccessUrl { get; set; }
            public string CancelUrl { get; set; }
        }

    }

}