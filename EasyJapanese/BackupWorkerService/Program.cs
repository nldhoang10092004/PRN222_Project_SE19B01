using BackupWorkerService;
using CoreLibrary.Backup;
using CoreLibrary.Data;
using CoreLibrary.Email;
using CoreLibrary.Membership;
using CoreLibrary.Payment;
using CoreLibrary.Reconciliation;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAppDbContext(builder.Configuration);
builder.Services.AddGmailEmail(builder.Configuration);
builder.Services.AddPayOS(builder.Configuration);

builder.Services.AddDatabaseBackup(builder.Configuration);
builder.Services.AddMembershipExpiry(builder.Configuration);
builder.Services.AddTransactionReconciliation(builder.Configuration);

builder.Services.AddHostedService<BackupWorker>();
builder.Services.AddHostedService<MembershipExpiryWorker>();
builder.Services.AddHostedService<ReconciliationWorker>();

var host = builder.Build();
host.Run();
