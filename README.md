# Hutko C# SDK

<p align="center">
	<a href="https://www.nuget.org/packages/HutkoSDK/"><img src="https://img.shields.io/nuget/v/HutkoSDK.svg" /></a>
	<a href="https://www.nuget.org/packages/HutkoSDK/"><img src="https://img.shields.io/nuget/dt/HutkoSDK.svg" /></a>
</p>

## Hutko payment service provider
Hutko - Ukrainian payment service that is destroying business forward Start working with payment more efficiently - it is necessary and you can find more in one event. This is the very best assistant who already knows what you need.[read more](https://en.wikipedia.org/wiki/Payment_service_provider)

## Installation

SDK availble on [NuGet](https://www.nuget.org/packages/HutkoSDK/).

## Requirements

Hutko account - [Register here](https://portal.hutko.org/#/account/create)

Newtonsoft.json (JSON.NET)


## Simple Start
```csharp
using HutkoSDK;
using HutkoSDK.Checkout;

Config.MerchantId = 1700002;
Config.SecretKey = "test";

var req = new CheckoutRequest {
  order_id = Guid.NewGuid().ToString("N"),
  amount = 100000,
  order_desc = "checkout json demo",
  currency = "UAH"
};
var resp = new Url().Post(req);
if (resp.Error == null) {
 string url = resp.checkout_url;
}
```
# Api

See [docs](https://docs.hutko.org/)
## Examples
To check it you can use build-in ISS server
[http://localhost:7777/](http://localhost:7777/)
