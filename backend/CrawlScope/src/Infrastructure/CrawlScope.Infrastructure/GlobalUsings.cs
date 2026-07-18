global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Net.Http.Headers;
global using System.Text;
global using System.Text.Encodings.Web;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Channels;
global using System.Threading.Tasks;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Playwright;

global using HtmlAgilityPack;
global using MailKit.Net.Smtp;
global using MailKit.Security;
global using MimeKit;

global using CrawlScope.Application.Abstractions.Crawling.Models;
global using CrawlScope.Application.Abstractions.Crawling.Services;
global using CrawlScope.Application.Abstractions.Email;
global using CrawlScope.Application.Abstractions.Export.Services;
global using CrawlScope.Application.Abstractions.Persistence;
global using CrawlScope.Application.Modules.Export.DTOs;
global using CrawlScope.Domain.Modules.Crawling.Enums;
global using CrawlScope.Infrastructure.Crawling.Services;
global using CrawlScope.Infrastructure.Export.Services;
