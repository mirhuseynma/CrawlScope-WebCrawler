using CrawlScope.Application.Modules.Admin.DTOs;
using MediatR;

namespace CrawlScope.Application.Modules.Admin.Queries.GetAdminOverview
{
    public record GetAdminOverviewQuery : IRequest<AdminOverviewDto>;
}
