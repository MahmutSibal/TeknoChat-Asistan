namespace TeknofestAsistan.Application.Common;

/// <summary>Thrown when a caller's reCAPTCHA token fails verification — distinct from
/// InvalidOperationException so controllers can map it to its own 400 response.</summary>
public class RecaptchaValidationException() : Exception("Robot doğrulaması başarısız oldu. Lütfen tekrar deneyin.");
