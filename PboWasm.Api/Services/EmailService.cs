namespace PboWasm.Api.Services;

public interface IEmailService
{
    Task SendValidationEmailAsync(string email, string code);
}

// DevEmailService simule l'envoi en affichant simplement le code dans la console noire de l'API.
// Plus tard, on créera un 'SendGridEmailService' qui implémentera la même interface !
public class DevEmailService : IEmailService
{
    public Task SendValidationEmailAsync(string email, string code)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n=======================================================");
        Console.WriteLine($"[SIMULATION EMAIL] Destinataire : {email}");
        Console.WriteLine($"[SIMULATION EMAIL] Message : Votre code de validation est le : {code}");
        Console.WriteLine("=======================================================\n");
        Console.ResetColor();

        return Task.CompletedTask;
    }
}
