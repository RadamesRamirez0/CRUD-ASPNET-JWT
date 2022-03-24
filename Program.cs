
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ProductoDb>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();


var securityScheme = new OpenApiSecurityScheme()
{
    Name = "Authorization",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "JSON Web Token based security",
};

var securityReq = new OpenApiSecurityRequirement()
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
        new string[] {}
    }
};

var contactInfo = new OpenApiContact()
{
    Name = "Radames Ramirez",
    Email = "rramirez31@ucol.mx",
    Url = new Uri("https://apitokenrada.azurewebsites.net") 
};

var license = new OpenApiLicense()
{
    Name = "Free License",
};

var info = new OpenApiInfo()
{
    Version = "V1",
    Title = "Api CRUD con Autenticación JWT",
    Description = "Api CRUD con Autenticación JWT",
    Contact = contactInfo,
    License = license
};

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", info);
    options.AddSecurityDefinition("Bearer", securityScheme);
    options.AddSecurityRequirement(securityReq);
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer (options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateAudience = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = false, // In any other application other then demo this needs to be true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// builder.Services.AddSingleton<ItemRepository>();
var app = builder.Build();

app.MapPost("/accounts/login", [AllowAnonymous] (UserDto user) => {
    if(user.username == "string" && user.password == "string")
    {
        var secureKey = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);

        var issuer = builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["Jwt:Audience"];
        var securityKey = new SymmetricSecurityKey(secureKey);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);

        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new [] {
                new Claim("Id", "1"),
                new Claim(JwtRegisteredClaimNames.Sub, user.username),
                new Claim(JwtRegisteredClaimNames.Email, user.username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Audience = audience,
            Issuer = issuer,
            SigningCredentials = credentials
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);
        return Results.Ok(jwtToken);  
    }
    return Results.Unauthorized();
});

app.MapGet("/producto",[Authorize] async (ProductoDb db) =>
    await db.Productos.ToListAsync());

app.MapGet("/producto/{id}",[Authorize] async (int id,ProductoDb db) =>
    await db.Productos.FindAsync(id)
    is Producto producto 
    ? Results.Ok(producto) 
    : Results.NotFound()
);


app.MapPost("/producto",[Authorize] async (Producto producto,ProductoDb db) =>
{
    db.Productos.Add(producto);
    await db.SaveChangesAsync();

    return Results.Created($"/productos/{producto.ID}", producto);
}
    );
app.MapPut("/producto/{id}",[Authorize] async (int id, Producto putProducto,ProductoDb db) =>{

    var producto = await db.Productos.FindAsync(id);
    if(producto is null) return Results.NotFound();

    producto.Nombre = putProducto.Nombre;

    await db.SaveChangesAsync();

    return Results.NoContent();

});
app.MapDelete("/producto/{id}",[Authorize] async (int id, ProductoDb db) =>{
    if(await db.Productos.FindAsync(id) is Producto producto){
        db.Productos.Remove(producto);
        await db.SaveChangesAsync();
        return Results.Ok(producto);
    }
    
    return Results.NotFound();
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "Hello from Minimal API");
app.Run();

// record Item(int id, string title, bool IsCompleted);

record UserDto (string username, string password);

class Item
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

// class ItemRepository
// {
//     private Dictionary<int, Item> items = new Dictionary<int, Item>();

//     public ItemRepository()
//     {
//         var item1 = new Item(1, "Go to the gym", false);
//         var item2 = new Item(2, "Drink Water", true);
//         var item3 = new Item(3, "Watch TV", false);

//         items.Add(item1.id, item1);
//         items.Add(item2.id, item2);
//         items.Add(item3.id, item3);
//     }

//     public IEnumerable<Item> GetAll() => items.Values;
//     public Item GetById(int id) {
//         if(items.ContainsKey(id))
//         {
//             return items[id];
//         }

//         return null;
//     }
//     public void Add(Item item) => items.Add(item.id, item);
//     public void Update(Item item) => items[item.id] = item;
//     public void Delete(int id) => items.Remove(id);
// }

class ApiDbContext : DbContext
{
    public DbSet<Item> Items { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }
}