using MasrLab.Domain.Common.Enums;
using MasrLab.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;

namespace MasrLab.Infrastructure.Tests;

[Collection("LocalDb")]
public class Slice8VisitTestGroupSnapshotIntegrationTests
{
    [LocalDbFact]
    public async Task OQ7_DeleteGroup_DoesNotAffectVisitTestPriceOrSnapshot()
    {
        var databaseName = LocalDbTestDatabase.NewDatabaseName("MasrLabDb_Slice8_OQ7");
        int groupId = 0;
        try
        {
            // --- Arrange: seed a patient, visit, test, group, group item, and visit test ---
            await using (var setup = LocalDbTestDatabase.CreateMigratedContext(databaseName))
            {
                var patient = new Patient
                {
                    Name = "Test Patient", Gender = Gender.Male
                };
                setup.Patients.Add(patient);
                await setup.SaveChangesAsync(CancellationToken.None);

                var visit = PatientVisit.Create(patient.Id, 1, "20260820-0001", null, null);
                setup.PatientVisits.Add(visit);
                await setup.SaveChangesAsync(CancellationToken.None);

                var test = new Test
                {
                    Name = "CBC", ReportName = "CBC Report", ReceiptName = "CBC Receipt",
                    Group = "Blood", TurnaroundTime = "1 day", Unit = "count",
                    Price = 200m
                };
                setup.Tests.Add(test);
                await setup.SaveChangesAsync(CancellationToken.None);

                var group = new TestGroup { GroupName = "RealLab" };
                setup.TestGroups.Add(group);
                await setup.SaveChangesAsync(CancellationToken.None);
                groupId = group.Id;

                setup.TestGroupItems.Add(new TestGroupItem
                {
                    TestGroupId = group.Id, TestId = test.Id, Price = 50m, DisplayOrder = 1
                });
                await setup.SaveChangesAsync(CancellationToken.None);

                // --- Act: create a VisitTest with group snapshot data ---
                var visitTest = new VisitTest(visit.Id, test.Id, 50m, false)
                {
                    TestNameSnapshot = "CBC",
                    ReportNameSnapshot = "CBC Report",
                    ReceiptNameSnapshot = "CBC Receipt",
                    SourceTestGroupId = group.Id,
                    TestGroupNameSnapshot = "RealLab"
                };
                setup.VisitTests.Add(visitTest);
                await setup.SaveChangesAsync(CancellationToken.None);

                // --- Act: soft-delete the group ---
                group.IsDeleted = true;
                await setup.SaveChangesAsync(CancellationToken.None);
            }

            // --- Assert: verify visit test is intact after group deletion ---
            await using (var verify = LocalDbTestDatabase.CreateContext(databaseName))
            {
                var vt = await verify.VisitTests.SingleAsync();

                Assert.Equal(50m, vt.Price);
                Assert.Equal(groupId, vt.SourceTestGroupId);
                Assert.Equal("RealLab", vt.TestGroupNameSnapshot);
                Assert.Equal("CBC", vt.TestNameSnapshot);
            }
        }
        finally
        {
            await using var cleanup = LocalDbTestDatabase.CreateContext(databaseName);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }
}
