using System.Text.Json;
using CineLog.Interfaces;
using CineLog.Repositories;
using CineLog.Services;

var builder = WebApplication.CreateBuilder(args);

// ── SERVIÇOS ────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.WriteIndented = true;
    });

// Pasta onde os arquivos .json de dados ficam gravados
var dataPath = Path.Combine(AppContext.BaseDirectory, "data");

// Repositórios como Singleton: uma única instância em memória, sincronizada
// com os arquivos em disco a cada escrita (persistência simultânea).
builder.Services.AddSingleton<MidiaRepositorioJson>(_ => new MidiaRepositorioJson(dataPath));
builder.Services.AddSingleton<IMidiaRepositorio>(sp => sp.GetRequiredService<MidiaRepositorioJson>());
builder.Services.AddSingleton<IUsuarioRepositorio>(_ => new UsuarioRepositorioJson(dataPath));
builder.Services.AddSingleton<IAvaliacaoRepositorio>(sp =>
    new AvaliacaoRepositorioJson(dataPath, sp.GetRequiredService<MidiaRepositorioJson>()));
builder.Services.AddSingleton<CatalogoService>();

// CORS liberado: permite abrir o HTML em qualquer origem (file://, outra porta, etc.)
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── SEED: popula dados de exemplo apenas na primeira execução ───────
SeedDados(app.Services.GetRequiredService<CatalogoService>());

// ── MIDDLEWARE ────────────────────────────────────────────────────
app.UseCors();
app.UseDefaultFiles();   // serve wwwroot/index.html em "/"
app.UseStaticFiles();    // serve o restante de wwwroot/
app.UseRouting();
app.MapControllers();

app.MapGet("/api/health", () => new { status = "ok", timestamp = DateTime.UtcNow });

Console.WriteLine();
Console.WriteLine("╔══════════════════════════════════════════════════╗");
Console.WriteLine("║         🎬  CineLog  —  API + Frontend            ║");
Console.WriteLine("╠══════════════════════════════════════════════════╣");
Console.WriteLine("║  Frontend:  http://localhost:5000                 ║");
Console.WriteLine("║  API:       http://localhost:5000/api/midias      ║");
Console.WriteLine("║  Dados em:  ./data/*.json                          ║");
Console.WriteLine("║  Ctrl+C para parar                                 ║");
Console.WriteLine("╚══════════════════════════════════════════════════╝");
Console.WriteLine();

app.Run("http://localhost:5000");

// ══════════════════════════════════════════════════════════════════
static void SeedDados(CatalogoService service)
{
    if (service.ListarMidias().Any()) return; // já existem dados salvos, não duplica

    Console.WriteLine("Nenhum dado encontrado — carregando catálogo de exemplo...");

    service.CadastrarFilme("O Poderoso Chefão", 1972, "Drama/Crime",
        "A história da família Corleone e seu império criminoso.", 175, "Francis Ford Coppola");
    service.CadastrarFilme("Interestelar", 2014, "Ficção Científica",
        "Um grupo de astronautas viaja além da galáxia em busca de um novo lar.", 169, "Christopher Nolan");
    service.CadastrarFilme("Parasita", 2019, "Thriller/Drama",
        "Duas famílias de classes sociais opostas se enredam em situações inesperadas.", 132, "Bong Joon-ho");
    service.CadastrarFilme("Cidade de Deus", 2002, "Drama/Crime",
        "A história de jovens no Rio de Janeiro marcados pela violência.", 130, "Fernando Meirelles");
    service.CadastrarFilme("A Origem", 2010, "Ficção Científica",
        "Um ladrão especializado em roubar segredos do subconsciente.", 148, "Christopher Nolan");
    service.CadastrarFilme("1917", 2019, "Guerra/Drama",
        "Dois soldados britânicos são enviados em uma missão quase impossível na Primeira Guerra.", 119, "Sam Mendes");

    service.CadastrarSerie("Breaking Bad", 2008, "Drama/Thriller",
        "Um professor de química transforma-se em produtor de metanfetamina.", 5, 62, false, "Vince Gilligan");
    service.CadastrarSerie("Dark", 2017, "Ficção Científica/Mistério",
        "Quatro famílias interligadas por viagens no tempo em uma cidade alemã.", 3, 26, false, "Baran bo Odar");
    service.CadastrarSerie("Chernobyl", 2019, "Drama Histórico",
        "A história da catástrofe nuclear de 1986 e seus heróis desconhecidos.", 1, 5, false, "Craig Mazin");
    service.CadastrarSerie("The Bear", 2022, "Drama/Comédia",
        "Um chef talentoso assume o restaurante de família após uma tragédia.", 3, 28, true, "Christopher Storer");

    var demo = service.CadastrarUsuario("Usuário Demo", "demo@cinelog.com", "Demo@123");

    try
    {
        service.AvaliarMidia(demo.Id, 1, 9.8, "Obra-prima absoluta do cinema.");
        service.AvaliarMidia(demo.Id, 2, 9.5, "Nolan no seu melhor.");
        service.AvaliarMidia(demo.Id, 3, 9.2, "Merece cada prêmio que ganhou.");
        service.AvaliarMidia(demo.Id, 7, 10.0, "A melhor série já feita.");
        service.AvaliarMidia(demo.Id, 8, 9.7, "Roteiro impecável.");
        service.AvaliarMidia(demo.Id, 9, 9.9, "Angustiante e necessário.");
    }
    catch { /* ignora se já existir avaliação */ }

    Console.WriteLine("Catálogo de exemplo carregado! Login demo: demo@cinelog.com / Demo@123");
}
