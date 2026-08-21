namespace Korp.Invoice.Billing.Application.DTOs.Dashboard;

public sealed record DashboardSummaryResponse(int OpenInvoices, int ClosedInvoices, int ProcessedItems);
