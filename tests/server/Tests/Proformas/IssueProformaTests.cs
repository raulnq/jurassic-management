using Tests.Infrastructure;

namespace Tests.Proformas;

public class IssueProformaTests : BaseTest
{
    [Fact]
    public async Task issue_should_be_ok()
    {
        var today = _appDsl.Clock.Now.DateTime;

        var result = await _appDsl.RegisterProformaReadyToIssue(today);

        await _appDsl.Proformas.Issue(c =>
        {
            c.ProformaId = result.ProformaId;
            c.IssueAt = today.AddDays(7);
        });
    }
}