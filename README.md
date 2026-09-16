# Hutko C# SDK

<p align="center">
	<a href="https://www.nuget.org/packages/HutkoSDK/"><img src="https://img.shields.io/nuget/v/HutkoSDK.svg" alt="NuGet version" /></a>
	<a href="https://www.nuget.org/packages/HutkoSDK/"><img src="https://img.shields.io/nuget/dt/HutkoSDK.svg" alt="NuGet downloads" /></a>
</p>

Official C# / .NET SDK for the [**Hutko**](https://hutko.org) payment gateway. It wraps the Hutko
HTTP API — hosted checkout, direct card payments with 3-D Secure, refunds, captures, recurring
payments, P2P credit and webhook verification — behind small, strongly-typed classes and handles
request signing and response-signature verification for you.

- API host: `https://pay.hutko.org`
- Merchant portal & reports: `https://portal.hutko.org`
- Full API documentation: <https://docs.hutko.org/>

## Contents

- [Supported platforms](#supported-platforms)
- [Installation](#installation)
- [Configuration](#configuration)
- [Quick start](#quick-start)
- [Content types & protocol](#content-types--protocol)
- [Operations](#operations)
  - [Hosted checkout — redirect](#hosted-checkout--redirect)
  - [Hosted checkout — token](#hosted-checkout--token)
  - [Direct card payment (PCI-DSS) with 3-D Secure](#direct-card-payment-pci-dss-with-3-d-secure)
  - [Card verification](#card-verification)
  - [Order status](#order-status)
  - [Refund / reversal](#refund--reversal)
  - [Capture a pre-authorised amount](#capture-a-pre-authorised-amount)
  - [Recurring payment (saved-card token)](#recurring-payment-saved-card-token)
  - [Calendar subscription](#calendar-subscription)
  - [P2P credit](#p2p-credit)
  - [Transaction list & reports](#transaction-list--reports)
- [Handling responses & errors](#handling-responses--errors)
- [Verifying callbacks (webhooks)](#verifying-callbacks-webhooks)
- [Testing (sandbox)](#testing-sandbox)
- [Changelog](#changelog)
- [License](#license)

## Supported platforms

The package multi-targets:

| Target | Runs on |
| --- | --- |
| `netstandard2.0` | .NET Framework 4.6.1+, .NET Core 2.0+, Mono, Xamarin, Unity |
| `netstandard2.1` | .NET Core 3.x and later |
| `net8.0` | .NET 8 (LTS) |
| `net10.0` | .NET 10 (LTS) |

The only runtime dependency is [Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/).

## Installation

```bash
dotnet add package HutkoSDK
```

Or add it to your project file:

```xml
<PackageReference Include="HutkoSDK" Version="1.0.0" />
```

## Configuration

Set your merchant credentials once at start-up via the static `Config` class:

```csharp
using HutkoSDK;

Config.MerchantId = 1700002;      // your merchant id
Config.SecretKey  = "test";       // payment key, used to sign requests
Config.CreditKey  = "testcredit"; // P2P credit key (only needed for P2P credit)

// Optional (these are the defaults):
Config.ContentType = "json";      // "json", "xml" or "form"
Config.Protocol    = "1.0.1";     // API protocol version
Config.ApiHost     = "pay.hutko.org";
```

> **Keep secrets out of source control.** Load `SecretKey` / `CreditKey` from environment
> variables, user-secrets or a secrets manager — never commit real keys. The values above are the
> public sandbox credentials (see [Testing](#testing-sandbox)).

`Config` values are read per request in a thread-safe way, so a single configured process can serve
concurrent requests.

## Quick start

Create a hosted checkout and redirect the customer to the returned URL:

```csharp
using System;
using HutkoSDK;
using HutkoSDK.Checkout;

Config.MerchantId = 1700002;
Config.SecretKey  = "test";

var req = new CheckoutRequest
{
    order_id   = Guid.NewGuid().ToString("N"),
    amount     = 100000,          // amount in minor units — 100000 = 1000.00
    currency   = "UAH",
    order_desc = "Order #123",
    server_callback_url = "https://example.com/hutko/callback"
};

var resp = new Url().Post(req);

if (resp.Error == null)
{
    // Redirect the customer's browser to resp.checkout_url
    string checkoutUrl = resp.checkout_url;
    int paymentId = resp.payment_id;
}
else
{
    Console.WriteLine($"{resp.Error.ErrorCode}: {resp.Error.ErrorMessage}");
}
```

> **Amounts** are always integers in the currency's minor units (kopiykas / cents). `100000` means
> `1000.00`.

## Content types & protocol

- **Content type** — `Config.ContentType` selects how requests and responses are encoded: `"json"`
  (default), `"xml"` or `"form"`. It has no effect on the result, only on the wire format.
- **Protocol** — `Config.Protocol` defaults to `"1.0.1"`, which adds `external_ref` to successful
  responses. Set it to `"2.0"` only for [calendar subscriptions](#calendar-subscription); the SDK
  switches to `2.0` automatically for those calls.

## Operations

Every operation returns a strongly-typed response whose `Error` property is `null` on success and
populated with the gateway error on failure. See [Handling responses & errors](#handling-responses--errors).

### Hosted checkout — redirect

```csharp
using HutkoSDK.Checkout;

var resp = new Url().Post(new CheckoutRequest
{
    order_id   = orderId,
    amount     = 100000,
    currency   = "UAH",
    order_desc = "Order #123",
    server_callback_url = "https://example.com/hutko/callback",
    response_url = "https://example.com/return"   // where the customer returns
});

// resp.checkout_url, resp.payment_id
```

### Hosted checkout — token

Use a token to render an embedded/native payment form instead of redirecting:

```csharp
using HutkoSDK.Checkout;

var resp = new Token().Post(new TokenRequest
{
    order_id   = orderId,
    amount     = 100000,
    currency   = "UAH",
    order_desc = "Order #123"
});

// resp.token
```

### Direct card payment (PCI-DSS) with 3-D Secure

> Direct card handling requires PCI-DSS compliance. If you are not PCI-certified, use hosted
> checkout above.

```csharp
using HutkoSDK.Payment;

var step1 = new Pcidss().StepOne(new StepOneRequest
{
    order_id    = orderId,
    amount      = 100000,
    currency    = "UAH",
    order_desc  = "Order #123",
    card_number = "4444555511116666",
    cvv2        = "111",
    expiry_date = "1230"          // MMYY
});

if (step1.Error != null)
{
    // handle gateway error
}
else if (step1.order_status == "approved")
{
    // Paid — no 3-D Secure required.
}
else if (step1.md != null)
{
    // 3-D Secure required. Redirect the cardholder's browser (HTTP POST) to step1.acs_url
    // with fields: PaReq = step1.pareq, MD = step1.md, TermUrl = <your return URL>.
    // The ACS posts a PaRes back to your TermUrl; then complete the payment:
    var step2 = new Pcidss().StepTwo(new StepTwoRequest
    {
        md    = step1.md,
        pares = pares            // value received on your TermUrl
    });

    // step2.order_status, step2.order_id
}
```

### Card verification

Validate a card without charging it:

```csharp
using HutkoSDK.Checkout;

var resp = new Verification().Post(new VerificationRequest
{
    order_id   = orderId,
    amount     = 0,
    currency   = "UAH",
    order_desc = "Card verification",
    verification_type = "amount"   // default
});
```

### Order status

```csharp
using HutkoSDK.Order;

var byOrder = new Status().StatusByOrderId(new StatusByOrderRequest { order_id = orderId });
// or
var byPayment = new Status().StatusByPaymentId(new StatusByPaymentRequest { payment_id = 123456 });

// resp.order_status, resp.amount, resp.currency, ...
```

### Refund / reversal

Full or partial. Pass the amount to refund (in minor units):

```csharp
using HutkoSDK.Order;

var reverse = new Reverse();

// by order id
var r1 = reverse.ByOrderID(new ReverseByOrder   { order_id = orderId, amount = 100000, currency = "UAH" });
// by payment id
var r2 = reverse.ByPaymentID(new ReverseByPayment { payment_id = 123456, amount = 100000, currency = "UAH" });
// by transaction id
var r3 = reverse.ByTransactionID(new ReverseByTransaction { transaction_id = 654321, amount = "100000" });

// r1.reverse_status, r1.reversal_amount
```

### Capture a pre-authorised amount

Create the payment with `preauth = "Y"` to hold funds, then capture later:

```csharp
using HutkoSDK.Order;

var resp = new Capture().Post(new CaptureRequest
{
    order_id = orderId,
    amount   = 100000,      // amount to capture (<= authorised amount)
    currency = "UAH"
});

// resp.capture_status
```

### Recurring payment (saved-card token)

Charge a previously stored `rectoken` without cardholder involvement:

```csharp
using HutkoSDK.Payment;

var resp = new Rectoken().Post(new RectokenRequest
{
    order_id   = orderId,
    amount     = 100000,
    currency   = "UAH",
    order_desc = "Subscription renewal",
    rectoken   = savedRectoken
});
```

### Calendar subscription

Schedule recurring charges on a calendar. This uses protocol `2.0` automatically:

```csharp
using HutkoSDK.Checkout;
using HutkoSDK.Models;

var resp = new Subscription().Post(new SubscriptionRequest
{
    order_id   = orderId,
    amount     = 100000,
    currency   = "UAH",
    order_desc = "Monthly plan",
    recurring_data = new ReccuringData
    {
        start_time = "2026-10-01",
        amount     = 100000,
        every      = 1,
        period     = "month"
    }
});

// resp.payment_id, resp.checkout_url
```

### P2P credit

Send funds to a card. Requires `Config.CreditKey`:

```csharp
using HutkoSDK.P2pcredit;

Config.CreditKey  = "testcredit";
Config.ContentType = "form";

var resp = new P2Pcredit().Post(new P2PcreditRequest
{
    order_id   = orderId,
    amount     = 100000,
    currency   = "UAH",
    order_desc = "Payout",
    receiver_card_number = "4444555511116666"
});

// resp.order_id, resp.order_status
```

### Transaction list & reports

```csharp
using HutkoSDK.Order;
using HutkoSDK.Payment;

// all transactions for an order
var list = new TransactionList().Post(new TransactionListRequest { order_id = orderId });

// settled transactions for a date range
var reports = new Reports().Post(new ReportsRequest
{
    date_from = "01.09.2026 00:00:00",
    date_to   = "17.09.2026 23:59:59"
});
```

## Handling responses & errors

Operations do not throw on gateway errors; instead each response exposes an `Error` property:

```csharp
var resp = new Url().Post(req);

if (resp.Error != null)
{
    // gateway/business error — e.g. 1006 "Merchant is not configured correctly"
    string code    = resp.Error.ErrorCode;
    string message = resp.Error.ErrorMessage;
    string reqId   = resp.Error.RequestId;   // quote this to Hutko support
    return;
}

// success
```

- `resp.Error` (a `ClientException`) is set for gateway/business errors and network failures.
- For flows that verify a signature (see below), `SignatureError` is set when verification fails.
- See the documented [error and decline codes](https://docs.hutko.org/) for the meaning of each code.

## Verifying callbacks (webhooks)

Hutko sends a server-to-server callback to your `server_callback_url` when a payment completes.
Allow inbound requests from Hutko's IP `52.49.13.27`, then verify the signature before trusting the
payload:

```csharp
using HutkoSDK.Response;

// rawBody: the exact request body Hutko POSTed; contentType: "json", "form" or "xml"
var result = new Response().GetResponse(rawBody, "json");

if (result.SignatureError != null)
{
    // Signature mismatch — reject the callback.
    return;
}

// Trusted: act on the result.
// result.order_id, result.order_status, result.amount, result.currency, ...
```

Respond with HTTP `200 OK` so Hutko stops retrying.

## Testing (sandbox)

Use the public sandbox merchant (documented at <https://docs.hutko.org/>):

| Setting | Value |
| --- | --- |
| `MerchantId` | `1700002` |
| `SecretKey` | `test` |
| `CreditKey` | `testcredit` |

Common test cards (expiry — any future date, e.g. `12/30`; CVV — any, e.g. `123`):

| Card number | 3-D Secure | Result |
| --- | --- | --- |
| `4444555511116666` | no | approved |
| `4444555566661111` | yes | approved |
| `4444111166665555` | yes | declined |

## Changelog

See [CHANGELOG.md](CHANGELOG.md).

## License

Distributed under the terms of the [GNU GPL v3](LICENSE).
