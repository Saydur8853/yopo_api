using System.ComponentModel.DataAnnotations;

namespace YopoAPI.Validation
{
    public class EitherEmailOrPhoneRequiredAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            // This attribute should be applied to the class, not individual properties
            return true;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // Get the Email and PhoneNumber properties from the object
            var emailProperty = validationContext.ObjectType.GetProperty("Email");
            var phoneProperty = validationContext.ObjectType.GetProperty("PhoneNumber");

            if (emailProperty == null || phoneProperty == null)
            {
                return new ValidationResult("Email and PhoneNumber properties are required for this validation.");
            }

            var emailValue = emailProperty.GetValue(validationContext.ObjectInstance) as string;
            var phoneValue = phoneProperty.GetValue(validationContext.ObjectInstance) as string;

            // Check if both email and phone are null or empty
            if (string.IsNullOrWhiteSpace(emailValue) && string.IsNullOrWhiteSpace(phoneValue))
            {
                return new ValidationResult(ErrorMessage ?? "Either Email or Phone Number is required.");
            }

            return ValidationResult.Success;
        }
    }
}
