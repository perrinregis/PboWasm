using System;
using System.Threading.Tasks;
using Azure;
using Azure.Communication.Email;

namespace PboWasm.Services.Email
{
    public class AzureEmailService : IEmailService
    {
        private readonly string _connectionString;
        private readonly string _senderAddress;

        public AzureEmailService(string connectionString, string senderAddress)
        {
            _connectionString = connectionString;
            _senderAddress = senderAddress;
        }

        public async Task SendValidationCodeAsync(string email, string code)
        {
            try
            {
                var emailClient = new EmailClient(_connectionString);

                var subject = "Votre code de validation PboWasm";
                var htmlContent = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; text-align: center;'>
                        <h2 style='color: #333;'>Bienvenue !</h2>
                        <p>Voici votre code de vérification pour valider votre compte :</p>
                        <div style='background-color: #f3f4f6; padding: 15px; margin: 20px auto; border-radius: 8px; max-width: 200px;'>
                            <h1 style='color: #4F46E5; letter-spacing: 5px; margin: 0;'>{code}</h1>
                        </div>
                        <p style='color: #666; font-size: 12px;'>Ce code est valable pendant 15 minutes.</p>
                    </div>";

                var emailMessage = new EmailMessage(
                    senderAddress: _senderAddress,
                    recipientAddress: email,
                    content: new EmailContent(subject)
                    {
                        Html = htmlContent
                    });

                // Envoi de l'email
                EmailSendOperation emailSendOperation = await emailClient.SendAsync(
                    WaitUntil.Completed,
                    emailMessage);
                    
                Console.WriteLine($"Email envoyé avec succès à {email} ! ID: {emailSendOperation.Id}");
            }
            catch (RequestFailedException ex)
            {
                Console.WriteLine($"Erreur critique lors de l'envoi de l'email : {ex.Message}");
            }
        }
    }
}
