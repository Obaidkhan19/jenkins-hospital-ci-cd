using HospitalApi;
using Xunit;

public class PatientServiceTests
{
    private readonly PatientService _service = new();

    [Fact]
    public void GetAll_ReturnsThreePatients()
    {
        Assert.Equal(3, _service.GetAll().Count);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsPatient()
    {
        var patient = _service.GetById(1);
        Assert.NotNull(patient);
        Assert.Equal("Anna Schmidt", patient!.Name);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        Assert.Null(_service.GetById(999));
    }
}
