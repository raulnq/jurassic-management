using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPI.Infrastructure.EntityFramework;
using WebAPI.Infrastructure.ExceptionHandling;

namespace WebAPI.Proformas;

public static class MarkProformaAsIssued
{
    public class Command
    {
        [JsonIgnore]
        public Guid ProformaId { get; set; }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ProformaId).NotEmpty();
        }
    }

    public class Handler
    {
        private readonly ApplicationDbContext _context;

        public Handler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(Command command)
        {
            var proforma = await _context.Set<Proforma>()
                .Include(p => p.Weeks)
                .ThenInclude(w => w.WorkItems)
                .SingleOrDefaultAsync(p => p.ProformaId == command.ProformaId);

            if (proforma == null)
            {
                throw new NotFoundException<Proforma>();
            }

            proforma.MarkAsIssued();
        }
    }
}