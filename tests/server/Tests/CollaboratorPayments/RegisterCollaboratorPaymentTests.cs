using Tests.Infrastructure;

namespace Tests.CollaboratorPayments;

public class RegisterCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task register_should_be_ok()
    {
        var (_, collaboratorResult) = await _appDsl.Collaborator.Register();

        await _appDsl.CollaboratorPayment.Register(c => c.CollaboratorId = collaboratorResult!.CollaboratorId);
    }
}

public class EditCollaboratorPaymentTests : BaseTest
{
    [Fact]
    public async Task edit_should_be_ok()
    {
        var (_, collaboratorResult) = await _appDsl.Collaborator.Register();

        await _appDsl.CollaboratorPayment.Register(c => c.CollaboratorId = collaboratorResult!.CollaboratorId);
    }
}

