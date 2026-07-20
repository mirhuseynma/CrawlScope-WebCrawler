global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Http;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.IdentityModel.Tokens;

global using AutoMapper.QueryableExtensions;
global using FluentValidation;
global using MediatR;

global using CrawlScope.Application.Abstractions.Crawling.Models;
global using CrawlScope.Application.Abstractions.Crawling.Services;
global using CrawlScope.Application.Abstractions.Export.Services;
global using CrawlScope.Application.Abstractions.Persistence;
global using CrawlScope.Application.Common.Behaviors;
global using CrawlScope.Application.Common.Exceptions;
global using CrawlScope.Application.Common.Settings;
global using CrawlScope.Application.Modules.Auth.DTOs;
global using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlJob;
global using CrawlScope.Application.Modules.Crawling.Commands.CreateCrawlSchedule;
global using CrawlScope.Application.Modules.Crawling.Commands.CancelCrawlJob;
global using CrawlScope.Application.Modules.Crawling.DTOs;
global using CrawlScope.Application.Modules.Crawling.Services;
global using CrawlScope.Application.Modules.Export.DTOs;
global using CrawlScope.Domain.Modules.Crawling.Enums;
global using CrawlScope.Domain.Modules.Export.Models;
global using AutoMapper;
global using CrawlScope.Application.Common.Extensions;
global using CrawlScope.Application.Common.Pagination;
global using CrawlScope.Application.Modules.Admin.DTOs;
global using CrawlScope.Domain.Modules.Auth.Models;
global using CrawlScope.Domain.Modules.Crawling.Models;
global using System.Linq.Expressions;
global using System.Text;
global using CrawlScope.Application.Common.Models;

global using CrawlScope.Application.Common.Helpers;

// Missing using directives for CQRS Auth/Admin refactor
global using Microsoft.AspNetCore.Identity;
global using CrawlScope.Application.Abstractions.Email;
global using CrawlScope.Domain.Constants;
global using Microsoft.AspNetCore.WebUtilities;
global using System.Security.Claims;
global using CrawlScope.Application.Abstractions.Auth;
global using Microsoft.Extensions.Options;
