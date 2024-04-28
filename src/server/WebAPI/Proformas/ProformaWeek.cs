using WebAPI.CollaboratorRoles;
using WebAPI.Infrastructure.ExceptionHandling;

namespace WebAPI.Proformas;

public class ProformaWeek
{
    public Guid ProformaId { get; private set; }
    public int Week { get; private set; }
    public DateTime Start { get; private set; }
    public DateTime End { get; private set; }
    public decimal Penalty { get; private set; }
    public decimal SubTotal { get; private set; }
    public List<ProformaWeekWorkItem> WorkItems { get; private set; }

    private ProformaWeek()
    {
        WorkItems = [];
    }

    public ProformaWeek(Guid platformId, int week, DateTime start, DateTime end, decimal minimumHours, decimal penaltyMinimumHours)
    {
        ProformaId = platformId;
        Week = week;
        Start = start;
        End = end;
        WorkItems = new List<ProformaWeekWorkItem>();
        Refresh(minimumHours, penaltyMinimumHours);
    }

    public void Add(Guid collaboratorId, CollaboratorRole collaboratorRole, decimal hours, decimal freeHours, decimal minimumHours, decimal penaltyMinimumHours)
    {
        if (WorkItems.Any(i => i.CollaboratorId == collaboratorId))
        {
            throw new DomainException(ExceptionCodes.Duplicated);
        }

        WorkItems.Add(new ProformaWeekWorkItem(ProformaId, Week, hours, freeHours, collaboratorId, collaboratorRole));


        Refresh(minimumHours, penaltyMinimumHours);
    }

    public void Edit(Guid collaboratorId, decimal hours, decimal freeHours, decimal minimumHours, decimal penaltyMinimumHours)
    {
        var item = WorkItems.FirstOrDefault(i => i.CollaboratorId == collaboratorId);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeekWorkItem>();
        }

        item.Edit(hours, freeHours);

        Refresh(minimumHours, penaltyMinimumHours);
    }

    public void Remove(Guid collaboratorId, decimal minimumHours, decimal penaltyMinimumHours)
    {
        var item = WorkItems.FirstOrDefault(i => i.CollaboratorId == collaboratorId);

        if (item == null)
        {
            throw new NotFoundException<ProformaWeekWorkItem>();
        }

        WorkItems.Remove(item);

        Refresh(minimumHours, penaltyMinimumHours);
    }

    private void Refresh(decimal minimumHours, decimal penaltyMinimumHours)
    {
        var hours = WorkItems.Sum(i => i.Hours);

        if (minimumHours > hours)
        {
            Penalty = (minimumHours - hours) * penaltyMinimumHours;
        }
        else
        {
            Penalty = 0;
        }

        SubTotal = WorkItems.Sum(i => i.SubTotal) + Penalty;
    }
}
