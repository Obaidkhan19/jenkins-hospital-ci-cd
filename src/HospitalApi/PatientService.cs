namespace HospitalApi;

public record Patient(int Id, string Name, string Department);

public class PatientService
{
    private static readonly List<Patient> Patients = new()
    {
        new Patient(1, "Anna Schmidt", "Cardiology"),
        new Patient(2, "Omar Ali", "Neurology"),
        new Patient(3, "Lisa Meyer", "Pediatrics")
    };

    public List<Patient> GetAll() => Patients;

    public Patient? GetById(int id) => Patients.FirstOrDefault(p => p.Id == id);
}
