namespace PriorAuthApi.DTOs;

public record PriorAuthDetailDto(
    int Id,
    string Status,
    string PatientName,
    string PractitionerName,
    string ServiceCode,
    string ServiceCodeDisplay,
    DateTime CreatedAt,
    string ClinicalData
);
