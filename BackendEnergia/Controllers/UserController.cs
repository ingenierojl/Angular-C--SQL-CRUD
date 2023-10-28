using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Data.SqlClient;


[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
/*
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        
        Console.WriteLine(model.Password);

        if (IsValidUser(model.Username, model.Password))
        {
            var token = GenerateToken(model.Username, model.Role);
            return Ok(new { token });
        }

        return Unauthorized();
    }

*/

[HttpPost("login")]
public IActionResult Login([FromBody] LoginModel model)
{
    Console.WriteLine(model.Password);

    var (isValid, userId) = IsValidUser(model.Username, model.Password);

    if (isValid)
    {
        var token = GenerateToken(model.Username, model.Role, userId);

        return Ok(new { IsValid = true, UserId = userId, Token = token });
    }

    return Unauthorized(new { IsValid = false, UserId = -1 });
}


    [HttpPost("crear-danos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "administrador")]
    public IActionResult CrearDanos([FromBody] DanoModel danoData)
    {
    Console.WriteLine("Usuario accedió a la ruta 'Crear-Daños'");
    Console.WriteLine("Datos recibidos:");
    Console.WriteLine($"Título: {danoData.Titulo}");
    Console.WriteLine($"Descripción: {danoData.Descripcion}");
    Console.WriteLine($"Fecha: {danoData.Fecha}");
    Console.WriteLine($"Estado: {danoData.Estado}");

    
    var dbConnection = new DbConnection();
    
    var dataInserter = new InsertData(dbConnection);

   
    dataInserter.InsertDamageReport(danoData);
     
        return new JsonResult(new { message = "Daño creado exitosamente" });
    }

    [HttpGet("listar-danos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "administrador")]
    public IActionResult GetReportes()
    {
        var dbConnection = new DbConnection();    
        var service = new ReporteService(dbConnection);
        var reportes = service.GetReportes();
   return Ok(reportes);
    }

      

// Controlador
[HttpPut("update/{id}")]
public IActionResult UpdateReporte(int id, Reporte updatedReporte)
{
    
  var dbConnection = new DbConnection();
  var dataUpdate = new UpdateData(dbConnection);
  dataUpdate.UpdateDamageReport(id, updatedReporte);

  return Ok();
}

    // Controlador
[HttpPut("delete/{id}")]
public IActionResult DeleteReporte(int id, Reporte updatedReporte)
{
    
    // Definir consulta SQL
  
  var dbConnection = new DbConnection();
   var dataDelete = new DeleteData(dbConnection);
   dataDelete.DeleteDamageReport(id);


  return Ok();
}

/*
    private bool IsValidUser(string username, string password)
    {   
        Console.WriteLine("Usuario recibido: " + username);
        
        
        var dbConnection = new DbConnection();
        var service = new ReporteService(dbConnection);

        var user = service.GetUsr(username).FirstOrDefault();
        

    if (user == null) 
    {
    return false; 
    }
    Console.WriteLine(user.Id);

    return user.Descripcion == password;
    }

*/

private (bool IsValid, int UserId) IsValidUser(string username, string password)
{   
    Console.WriteLine("Usuario recibido: " + username);

    var dbConnection = new DbConnection();
    var service = new ReporteService(dbConnection);

    var user = service.GetUsr(username).FirstOrDefault();

    if (user == null) 
    {
        return (false, -1); // Devuelve un valor predeterminado para el UserId en caso de usuario no válido
    }

    Console.WriteLine(user.Id);

    return (user.Descripcion == password, user.Id);
}

/*
    private string GenerateToken(string username, string role)
    {
        var secretKey = _configuration["Jwt:SecretKey"];  
        var audience = _configuration["Jwt:Audience"];    
        var issuer = _configuration["Jwt:Issuer"];        

        if (secretKey is null)
        {
            throw new InvalidOperationException("La clave secreta JWT no está configurada.");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }   

*/

private string GenerateToken(string username, string role, int userId)
{
    var secretKey = _configuration["Jwt:SecretKey"];
    var audience = _configuration["Jwt:Audience"];
    var issuer = _configuration["Jwt:Issuer"];

    if (secretKey is null)
    {
        throw new InvalidOperationException("La clave secreta JWT no está configurada.");
    }

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role),
        new Claim("UserId", userId.ToString()) // Agregar el ID del usuario como un claim personalizado
    };

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.Now.AddHours(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}



}

public class LoginModel
{
    public Int32 Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class DanoModel
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime? Fecha { get; set; } = null; 
    public string Estado { get; set; }  = string.Empty;
}
