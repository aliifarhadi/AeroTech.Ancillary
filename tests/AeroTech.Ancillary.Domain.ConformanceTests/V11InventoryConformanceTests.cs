using AeroTech.Ancillary.Domain.ConformanceTests.Fixtures;
using AeroTech.Messages.Ancillary.Enums;
using Xunit;
using static AeroTech.Ancillary.Domain.ConformanceTests.Fixtures.P2Fixtures;

namespace AeroTech.Ancillary.Domain.ConformanceTests;

public class V11InventoryConformanceTests
{
    [Fact]
    public void V11_I15_a_count_weight_or_slot_source_with_active_usage_cannot_be_retired()
    {
        BusinessAssert.Throws(16621, 409, () => CountSource().Retire(1, 1m, Now));
        BusinessAssert.Throws(16621, 409, () => WeightSource().Retire(1, 0.001m, Now));
        BusinessAssert.Throws(16621, 409, () => Slot(10, 11).Retire(1, 2m, Now));

        var count = CountSource();
        var weight = WeightSource();
        var slot = Slot(10, 11);

        count.Retire(1, 0m, Now);
        weight.Retire(1, 0m, Now);
        slot.Retire(1, 0m, Now);

        Assert.Equal(
            (InventoryRecordStatus.Retired, InventoryRecordStatus.Retired, InventoryRecordStatus.Retired),
            (count.Status, weight.Status, slot.Status));
    }
}
