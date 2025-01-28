using AccountServiceGCA.Application.AuthorizeOptions;
using System.Configuration;
using AFC.database.chat;
using Microsoft.EntityFrameworkCore;
using SignalRChatRoom.Server.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.AllowAnyMethod().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(origin => true)));

builder.Services.AddSwaggerGen( swagger =>
{
	//This is to generate the Default UI of Swagger Documentation
	swagger.SwaggerDoc( "v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "CHat",
		Description = "������ ���� �������� ������� AccountServiceGCA"
	} );
	swagger.AddSignalRSwaggerGen();
	// To Enable authorization using Swagger (JWT)
	swagger.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme()
	{
		Name = "Authorization",
		Scheme = "Bearer",
		BearerFormat = "JWT",
		In = ParameterLocation.Header,
		Description =
			"JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
	} );
	swagger.AddSecurityRequirement( new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},	
			new string[] { }
		}
	} );
} );

builder.Services.AddSignalR();
builder.Services.AddDbContext<ApplicationContext>( options => options.UseNpgsql( builder.Configuration.GetSection( "ConnectionString" ).Value ) );
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ChatsManageRepository>();
builder.Services.AddSingleton( builder.Configuration.GetSection( "AuthOptions" ).Get<AuthOptions>() );
builder.Services.AddAuthentication( JwtBearerDefaults.AuthenticationScheme )
	.AddJwtBearer( options =>
	{
		AuthOptions authOptions = builder.Configuration.GetSection( "AuthOptions" ).Get<AuthOptions>();
		options.RequireHttpsMetadata = false;
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = authOptions.Issuer,

			ValidateAudience = true,
			ValidAudience = authOptions.Audience,

			ValidateLifetime = true,

			IssuerSigningKey = authOptions.GetSymmetricSecurityKey(),
			ValidateIssuerSigningKey = true,
		};
	} );
var app = builder.Build();
if ( app.Environment.IsDevelopment() )
{
	app.UseDeveloperExceptionPage();
	app.UseSwagger();
	app.UseSwaggerUI( c =>
	{
		c.SwaggerEndpoint( "/swagger/v1/swagger.json", "FractalzBackend" );
		c.RoutePrefix = string.Empty;
	} );
}

app.UseCors();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.UseEndpoints( endpoints =>
{
	app.MapHub<ChatHub>( "/chathub" ); // Замените YourHub на имя вашего хаба
} );

app.MapHub<ChatHub>("/chathub");


app.Run();
