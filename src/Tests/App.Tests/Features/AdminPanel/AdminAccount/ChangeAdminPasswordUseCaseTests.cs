using App.Features.AdminPanel.AdminAccount;
using App.Features.AdminPanel.AdminAccount.ChangeAdminPassword;
using App.SharedPorts;
using App.SharedPorts.HashPassword;
using Moq;
using Moq.AutoMock;

namespace App.Tests.Features.AdminPanel.AdminAccount;

public class ChangeAdminPasswordUseCaseTests
{
    private readonly AutoMocker _mocker = new();
    private readonly IChangeAdminPasswordUseCase _useCase;

    public ChangeAdminPasswordUseCaseTests()
    {
        _useCase = _mocker.CreateInstance<ChangeAdminPasswordUseCase>();
    }

    [Fact]
    public void Execute_WhenCalled_HashNewPassword()
    {
        SetupAccount();
        SetupHashPassword();

        _useCase.Execute(new ChangeAdminPasswordModel("newPassword123"));

        _mocker.GetMock<IHashPassword>()
            .Verify(x => x.Execute(
                    It.Is<HashPasswordRequest>(r => r.Password == "newPassword123")),
                Times.Once);
    }

    [Fact]
    public void Execute_WhenCalled_SaveHashedPassword()
    {
        SetupAccount();
        SetupHashPassword("$2a$12$hashedpassword");

        _useCase.Execute(new ChangeAdminPasswordModel("newPassword123"));

        _mocker.GetMock<ISaveAdminAccount>()
            .Verify(x => x.Execute(
                    It.Is<SaveAdminAccountModel>(m =>
                        m.PasswordHash == "$2a$12$hashedpassword")),
                Times.Once);
    }

    [Fact]
    public void Execute_WhenCalled_SetsMustChangePasswordToFalse()
    {
        SetupAccount();
        SetupHashPassword();

        _useCase.Execute(new ChangeAdminPasswordModel("newPassword123"));

        _mocker.GetMock<ISaveAdminAccount>()
            .Verify(x => x.Execute(
                    It.Is<SaveAdminAccountModel>(m => m.MustChangePassword == false)),
                Times.Once);
    }

    private void SetupAccount() =>
        _mocker.GetMock<IGetAdminAccount>()
            .Setup(x => x.Execute())
            .Returns(new GetAdminAccountModel
            {
                Username = "admin",
                PasswordHash = "$2a$12$oldhash",
                MustChangePassword = true
            });

    private void SetupHashPassword(string hash = "$2a$12$hash") =>
        _mocker.GetMock<IHashPassword>()
            .Setup(x => x.Execute(It.IsAny<HashPasswordRequest>()))
            .Returns(new HashPasswordResult(hash));
}