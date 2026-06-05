# Changelog

All notable changes to this project will be documented in this file.

## [2.0.0] - 2025-06-05

### Added
- Complete rewrite targeting WayaPay Merchant API v2
- `Banks` — list all supported banks and CBN codes
- `Accounts` — verify bank accounts; create dynamic virtual accounts
- `Identity` — BVN verification with local 11-digit validation
- `Payouts` — initiate bank transfers with auto-generated idempotency references
- `Collect` — create one-time and subscription payment links
- `Transactions` — verify individual transactions; paginated history stream via `IAsyncEnumerable`
- Automatic retry with exponential backoff on GET requests (timeouts, 429, 5xx)
- `WayaPayException` with typed `ErrorCode`, `Status`, and `Type` for precise error handling
- Full `CancellationToken` support on every async method
- `HttpClient` injection for DI and testability
