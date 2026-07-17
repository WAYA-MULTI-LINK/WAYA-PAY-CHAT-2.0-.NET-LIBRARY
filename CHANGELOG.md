# Changelog

All notable changes to this project will be documented in this file.

## [2.0.1] - 2026-07-17

First version published to [NuGet](https://www.nuget.org/packages/WayaQuick.Integration) via the automated release pipeline. No functional changes.

### Changed

- NuGet package ID is now `WayaQuick.Integration` (renamed from `WayaPay`), matching the Java SDK's `wayaquick-integration` artifactId
- Renamed WayaPay → WayaQuick throughout: root namespace `WayaPay` → `WayaQuick` and classes `WayaPayClient` → `WayaQuickClient`, `WayaPayOptions` → `WayaQuickOptions`, `WayaPayWebhook` → `WayaQuickWebhook`, `WayaPayWebhookException` → `WayaQuickWebhookException`, `WayaPayResponse` → `WayaQuickResponse`, `WayapayErrorResponse` → `WayaQuickErrorResponse`
- Live-test environment variables renamed: `WAYAPAY_MERCHANT_ID` → `WAYAQUICK_MERCHANT_ID`, `WAYAPAY_SECRET_KEY` → `WAYAQUICK_SECRET_KEY`
- Publish workflow now pushes to nuget.org and creates a GitHub Release on every `v*.*.*` tag

## [2.0.0] - 2026-06-06

### Breaking changes from 1.x

- `client.Banks` renamed to `client.Payouts.ListBanksAsync()`
- `client.Accounts.VerifyAsync()` moved to `client.Payouts.VerifyAccountAsync()` — input type is now `PayoutVerifyRequestModel`
- `client.Collect` renamed to `client.Collection`; `CreateAsync` renamed to `InitiateAsync` — input type is now `CollectionRequestModel`
- `client.Identity.VerifyBvnAsync()` now takes `BvnIdentityRequestModel` instead of a raw string
- `client.Accounts.CreateDynamicAsync()` removed — dynamic virtual accounts are not part of the v2 API
- `client.Transactions` removed — transaction verification and history streaming are not part of the v2 API
- `WayaQuickException` removed — errors now throw `HttpRequestException` or `InvalidOperationException`
- `Environment` option removed from `WayaQuickOptions` — the production base URL is a compile-time constant; override with `BaseUrl` is no longer supported
- `PayoutInput`, `CollectInput`, `VerifyAccountInput` removed — replaced by the `*Model` types above

### Added

- `Payouts.ListBanksAsync()` — returns all supported banks and CBN codes
- `Payouts.VerifyAccountAsync(PayoutVerifyRequestModel)` — resolves an account number to its registered name; validates that `BankCode` is present when `EnquiryType` is `"OTHERS"`
- `Payouts.InitiateAsync(PayoutRequestModel)` — initiates a bank transfer; `PROCESSING` means accepted, not settled
- `Collection.InitiateAsync(CollectionRequestModel)` — starts a payment collection and returns a checkout URL
- `Identity.VerifyBvnAsync(BvnIdentityRequestModel)` — verifies a BVN with local 11-digit format check before the network call
- `WayaQuickClient.GenerateReference(prefix)` — generates a timestamped, collision-resistant idempotency key
- Automatic retry with exponential backoff on GET requests (timeouts, 429, 5xx); writes never auto-retry
- Full `CancellationToken` support on every async method
- `HttpClient` injection via `WayaQuickOptions.HttpClient` for DI, handler chains, and test fakes
