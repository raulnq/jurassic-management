using Tests.Infrastructure;

namespace Tests.Proformas;

public class IssueProformaTests : BaseTest
{
    [Fact]
    public async Task issue_should_be_ok()
    {
        var result = await _appDsl.RegisterProformaReadyToIssue(_appDsl.Clock.Now.DateTime);

        await _appDsl.Proformas.Issue(c => c.ProformaId = result.ProformaId);
    }
}