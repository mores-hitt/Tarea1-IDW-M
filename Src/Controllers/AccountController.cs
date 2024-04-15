using courses_dotnet_api.Src.DTOs.Account;
using courses_dotnet_api.Src.Interfaces;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace courses_dotnet_api.Src.Controllers;

public class AccountController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;

    public AccountController(IUserRepository userRepository, IAccountRepository accountRepository)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
    }

    [HttpPost("register")]
    public async Task<IResult> Register(RegisterDto registerDto)
    {
        if (
            await _userRepository.UserExistsByEmailAsync(registerDto.Email)
            || await _userRepository.UserExistsByRutAsync(registerDto.Rut)
        )
        {
            return TypedResults.BadRequest("User already exists");
        }

        await _accountRepository.AddAccountAsync(registerDto);

        if (!await _accountRepository.SaveChangesAsync())
        {
            return TypedResults.BadRequest("Failed to save user");
        }

        AccountDto? accountDto = await _accountRepository.GetAccountAsync(registerDto.Email);

        return TypedResults.Ok(accountDto);
    }

    [HttpPost("login")]

    public async Task<IResult> Login(LoginDto logindto)
    {

        AccountDto? accountDto = await _accountRepository.GetAccountAsync(logindto.Email);

        if(accountDto == null)
        {
            return TypedResults.BadRequest("Incorrect credentials");
        }

        AccountPasswordDto? accountPasswordDto = await _accountRepository.GetLoginCredentialsAsync(logindto.Email);

        if(accountPasswordDto == null)
        {
            return TypedResults.BadRequest("Error. Try again please c:");
        }

        using var hmac = new HMACSHA512(accountPasswordDto.PasswordSalt);

        string encryptedPassword = BitConverter.ToString(accountPasswordDto.EncryptedPassword);
        string encryptedInputPassword = BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(logindto.Password)));

        if(encryptedPassword != encryptedInputPassword)
        {
            return TypedResults.BadRequest("Incorrect credentials");
        }

        return TypedResults.Ok(accountDto);

    }
}
