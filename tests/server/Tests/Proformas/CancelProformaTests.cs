using Tests.Infrastructure;

namespace Tests.Proformas;

public class CancelProformaTests : BaseTest
{
    [Fact]
    public async Task cancel_should_be_ok()
    {
        var result = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.Proformas.Cancel(c => c.ProformaId = result.ProformaId);
    }

    [Fact]
    public async Task cancel_should_throw_an_error_when_proforma_is_not_pending()
    {
        var result = await _appDsl.IssuedProforma(_appDsl.Clock.Now.DateTime);

        await _appDsl.Proformas.Cancel(c => c.ProformaId = result.ProformaId, "code: proforma-status-not-pending");
    }
}