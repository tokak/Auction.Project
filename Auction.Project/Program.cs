using Auction.Business.Abstraction;
using Auction.Business.Concrete;
using Auction.Business.Mapper;
using Auction.Core.Models;
using Auction.DataAccess.Context;
using Auction.DataAccess.Models;
using Auction.Project.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddPersistenceLayer(builder.Configuration);
builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddInfrastructureLayer(builder.Configuration);
builder.Services.AddAutoMapper(typeof(MappingProfile));
//builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddCors();


builder.Services.AddSwaggerCollection(builder.Configuration);
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x=>x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin=>true).AllowCredentials());
app.UseHttpsRedirection();
app.UseAuthentication();    
app.UseAuthorization();

app.MapControllers();

app.Run();
