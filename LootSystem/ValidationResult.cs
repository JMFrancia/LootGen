public class ValidationResult
{
  public static ValidationResult Valid()
  {
    return new ValidationResult(true, "");
  }

  public static ValidationResult Error(string errorMessage)
  {
    return new ValidationResult(false, errorMessage);
  }

  private ValidationResult(bool isValid, string errorMessage)
  {
    this.IsValid = isValid;
    this.ErrorMessage = errorMessage;
  }

  public bool IsValid { get; }
  public string ErrorMessage { get; }
}
