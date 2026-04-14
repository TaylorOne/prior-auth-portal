
namespace PriorAuthApi.Validators
{
    public class FormDefinition
    {
        public List<FieldDefinition> Fields { get; set; } = [];
        public List<MedicationFieldDefinition> MedicationFields { get; set; } = [];
    }

    public class FieldDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public FieldValidation Validation { get; set; } = new();
    }

    public class FieldValidation
    {
        public bool Required { get; set; }
        public List<string>? AllowedValues { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public bool? Integer { get; set; }
        public int? MaxLength { get; set; }
    }

    public class MedicationFieldDefinition : FieldDefinition
    {
        public string MedicationProperty { get; set; } = string.Empty;
    }
}
