using System;
using CineLog.Models;
using CineLog.Repositories;
using CineLog.Services;
using CineLog.UI;

namespace CineLog
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Composição das dependências (Injeção de Dependência manual)
                var midiaRepo    = new MidiaRepositorioJson("data");
                var usuarioRepo  = new UsuarioRepositorioJson("data");
                var avaliacaoRepo = new AvaliacaoRepositorioJson("data");

                var service = new CatalogoService(midiaRepo, usuarioRepo, avaliacaoRepo);

                // Seed: popula dados de exemplo se o catálogo estiver vazio
                SeedDados(service);

                // Inicia a interface de console
                var ui = new ConsoleUI(service);
                ui.Iniciar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nErro crítico: {ex.Message}");
                Console.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey();
            }
        }

        private static void SeedDados(CatalogoService service)
        {
            // Só popula se o catálogo estiver vazio
            if (service.ListarMidias().Any()) return;

            Console.WriteLine("Carregando dados de exemplo...");

            // Filmes
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

            // Séries
            service.CadastrarSerie("Breaking Bad", 2008, "Drama/Thriller",
                "Um professor de química transforma-se em produtor de metanfetamina.", 5, 62, false, "Vince Gilligan");

            service.CadastrarSerie("Dark", 2017, "Ficção Científica/Mistério",
                "Quatro famílias interligadas por viagens no tempo em uma cidade alemã.", 3, 26, false, "Baran bo Odar");

            service.CadastrarSerie("Chernobyl", 2019, "Drama Histórico/Minissérie",
                "A história da catástrofe nuclear de 1986 e seus heróis desconhecidos.", 1, 5, false, "Craig Mazin");

            service.CadastrarSerie("The Bear", 2022, "Drama/Comédia",
                "Um chef talentoso assume o restaurante de família após uma tragédia.", 3, 28, true, "Christopher Storer");

            // Usuário demo
            service.CadastrarUsuario("Admin Demo", "admin@cinelog.com", "Admin@123");

            // Avaliações de exemplo
            try
            {
                service.AvaliarMidia(1, 1, 9.8, "Obra-prima absoluta do cinema.");
                service.AvaliarMidia(1, 2, 9.5, "Nolan no seu melhor.");
                service.AvaliarMidia(1, 3, 9.2, "Merece cada prêmio que ganhou.");
                service.AvaliarMidia(1, 6, 10.0, "A melhor série já feita.");
                service.AvaliarMidia(1, 7, 9.7, "Roteiro impecável.");
            }
            catch { /* Ignora avaliações duplicadas no seed */ }

            Console.WriteLine("Dados carregados!\n");
        }
    }
}
