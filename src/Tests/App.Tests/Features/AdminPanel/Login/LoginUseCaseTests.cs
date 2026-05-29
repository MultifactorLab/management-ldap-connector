using App.Features.AdminPanel.AdminAccount;
using App.Features.AdminPanel.Login;
using App.SharedPorts;
using App.SharedPorts.VerifyPassword;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.AdminPanel.Login;

public class LoginUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly ILoginUseCase _useCase;

    public LoginUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<LoginUseCase>();
    }

    [Fact]
    public void Execute_WhenCredentialsAreValid_ReturnSuccess()
    {
        SetupAccount(mustChangePassword: false);
        SetupVerify(isValid: true);

        var result = _useCase.Execute(new LoginRequest("correct_password"));

        Assert.True(result.IsSuccess);
        Assert.False(result.MustChangePassword);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Execute_WhenCredentialsAreValidAndMustChangePassword_ReturnsSuccessWithFlag()
    {
        SetupAccount(mustChangePassword: true);
        SetupVerify(isValid: true);

        var result = _useCase.Execute(new LoginRequest("correct_password"));

        Assert.True(result.IsSuccess);
        Assert.True(result.MustChangePassword);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Execute_WhenPasswordIsInvalid_ReturnInvalidCredentials()
    {
        SetupAccount(mustChangePassword: false);
        SetupVerify(isValid: false);

        var result = _useCase.Execute(new LoginRequest("wrong_password"));

        Assert.False(result.IsSuccess);
        Assert.Equal(LoginError.InvalidCredentials, result.Error);
    }

    private void SetupAccount(bool mustChangePassword) =>
        _mocker.GetMock<IGetAdminAccount>()
            .Setup(x => x.Execute())
            .Returns(new GetAdminAccountModel
            {
                Username = "admin",
                PasswordHash = "$2a$12$hash",
                MustChangePassword = mustChangePassword
            });

    private void SetupVerify(bool isValid) =>
        _mocker.GetMock<IVerifyPassword>()
            .Setup(x => x.Execute(It.IsAny<VerifyPasswordRequest>()))
            .Returns(new VerifyPasswordResult(isValid));
}