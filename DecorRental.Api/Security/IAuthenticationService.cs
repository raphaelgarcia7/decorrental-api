namespace DecorRental.Api.Security;

public interface IAuthenticationService
{
    AuthenticationResult Authenticate(string username, string password);
}
