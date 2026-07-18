global using System;
global using System.Collections.Generic;
global using System.IdentityModel.Tokens.Jwt;
global using System.Linq;
global using System.Security.Claims;
global using System.Text;
global using System.Threading.Tasks;

global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Infrastructure;
global using Microsoft.EntityFrameworkCore.Metadata;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.EntityFrameworkCore.Migrations;
global using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

global using CrawlScope.Application.Abstractions.Auth;
global using CrawlScope.Application.Abstractions.Email;
global using CrawlScope.Application.Abstractions.Persistence;
global using CrawlScope.Application.Common.Exceptions;
global using CrawlScope.Application.Common.Models;
global using CrawlScope.Application.Common.Pagination;
global using CrawlScope.Application.Common.Settings;
global using CrawlScope.Application.Modules.Auth.DTOs;
global using CrawlScope.Domain.Constants;
global using CrawlScope.Domain.Modules.Auth.Models;
global using CrawlScope.Domain.Modules.Crawling.Models;
global using CrawlScope.Domain.Modules.Export.Models;
global using CrawlScope.Persistence.Context;
global using CrawlScope.Persistence.Services;
