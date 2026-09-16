# Changelog

All notable, merchant-facing changes to the **Hutko C# SDK** are documented in this file.
The format is based on [Keep a Changelog](https://keepachangelog.com/), and the project
follows [Semantic Versioning](https://semver.org/).

## [1.0.1] - 2026-09-17

### Fixed
- The package README now renders correctly on the NuGet package page (the badges
  previously used raw HTML, which NuGet displayed as text). No functional or API changes.

## [1.0.0] - 2026-09-17

Initial public release on [NuGet](https://www.nuget.org/packages/HutkoSDK/).

### Supported platforms
- .NET Standard 2.0 and 2.1 — usable from .NET Framework 4.6.1+, .NET Core 2.0+, Xamarin and Unity.
- .NET 8 and .NET 10.

### Payment operations
- **Hosted checkout** — redirect flow (`checkout/url`) and embedded/token flow (`checkout/token`).
- **Direct card payments with 3-D Secure** (PCI-DSS `3dsecure_step1` / `3dsecure_step2`).
- **Card verification** (validate a card without charging).
- **Order status** lookup by `order_id` or `payment_id`.
- **Refunds / reversals** — full or partial, by order, payment or transaction.
- **Capture** of pre-authorised (hold) amounts.
- **Recurring payments** using saved-card tokens (`rectoken`).
- **Calendar subscriptions** and **settlements**.
- **P2P credit** transfers to a card.
- **Transaction lists** and **reports**.

### Requests & responses
- JSON, XML and form-url-encoded content types.
- Protocol version **`1.0.1` by default** (adds `external_ref` to responses); version `2.0` is
  used for calendar subscriptions.
- Requests are signed automatically, and response / server-callback (webhook) signatures are
  verified for you.

### Security
- All API calls use HTTPS with a **TLS 1.2+** minimum.

[1.0.1]: https://www.nuget.org/packages/HutkoSDK/1.0.1
[1.0.0]: https://www.nuget.org/packages/HutkoSDK/1.0.0
