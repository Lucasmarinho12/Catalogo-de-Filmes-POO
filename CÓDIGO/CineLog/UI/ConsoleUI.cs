using System;
using System.Collections.Generic;
using System.Linq;
using CineLog.Exceptions;
using CineLog.Models;
using CineLog.Services;

namespace CineLog.UI
{
    // Padrão Facade — simplifica a interação com o sistema via console
    public class ConsoleUI
    {
        private readonly CatalogoService _service;
        private Usuario? _usuarioLogado;

        // Paleta ANSI para visual mais rico no terminal
        private const string RESET  = "\x1b[0m";
        private const string BOLD   = "\x1b[1m";
        private const string CYAN   = "\x1b[36m";
        private const string YELLOW = "\x1b[33m";
        private const string GREEN  = "\x1b[32m";
        private const string RED    = "\x1b[31m";
        private const string MAGENTA= "\x1b[35m";
        private const string DIM    = "\x1b[2m";

        public ConsoleUI(CatalogoService service) => _service = service;

        public void Iniciar()
        {
            ExibirBanner();
            MenuPrincipal();
        }

        private void ExibirBanner()
        {
            Console.Clear();
            Console.WriteLine($"{CYAN}{BOLD}");
            Console.WriteLine("  ██████╗██╗███╗   ██╗███████╗██╗      ██████╗  ██████╗ ");
            Console.WriteLine(" ██╔════╝██║████╗  ██║██╔════╝██║     ██╔═══██╗██╔════╝ ");
            Console.WriteLine(" ██║     ██║██╔██╗ ██║█████╗  ██║     ██║   ██║██║  ███╗");
            Console.WriteLine(" ██║     ██║██║╚██╗██║██╔══╝  ██║     ██║   ██║██║   ██║");
            Console.WriteLine(" ╚██████╗██║██║ ╚████║███████╗███████╗╚██████╔╝╚██████╔╝");
            Console.WriteLine("  ╚═════╝╚═╝╚═╝  ╚═══╝╚══════╝╚══════╝ ╚═════╝  ╚═════╝ ");
            Console.WriteLine($"{RESET}{DIM}  Seu catálogo pessoal de filmes e séries{RESET}");
            Console.WriteLine();
        }

        private void MenuPrincipal()
        {
            while (true)
            {
                Console.WriteLine($"\n{BOLD}╔══ MENU PRINCIPAL ════════════════════╗{RESET}");
                Console.WriteLine($"  {CYAN}1.{RESET} Entrar / Login");
                Console.WriteLine($"  {CYAN}2.{RESET} Criar conta");
                Console.WriteLine($"  {CYAN}3.{RESET} Explorar catálogo (sem login)");
                Console.WriteLine($"  {RED}0.{RESET} Sair");
                Console.WriteLine($"{BOLD}╚══════════════════════════════════════╝{RESET}");

                switch (LerOpcao())
                {
                    case "1": Autenticar(); break;
                    case "2": CriarConta(); break;
                    case "3": ExplorarCatalogo(); break;
                    case "0": Console.WriteLine($"\n{GREEN}Até logo! 🎬{RESET}\n"); return;
                    default: MensagemErro("Opção inválida."); break;
                }
            }
        }

        private void Autenticar()
        {
            Console.Write($"\n  {CYAN}E-mail:{RESET} ");
            var email = Console.ReadLine()?.Trim() ?? "";
            Console.Write($"  {CYAN}Senha:{RESET} ");
            var senha = LerSenha();

            try
            {
                _usuarioLogado = _service.Login(email, senha);
                MensagemSucesso($"Bem-vindo, {_usuarioLogado.Nome}! 🎬");
                MenuUsuario();
            }
            catch (CineLogException ex) { MensagemErro(ex.Message); }
        }

        private void CriarConta()
        {
            Titulo("CRIAR CONTA");
            Console.Write($"  {CYAN}Nome:{RESET} ");
            var nome = Console.ReadLine()?.Trim() ?? "";
            Console.Write($"  {CYAN}E-mail:{RESET} ");
            var email = Console.ReadLine()?.Trim() ?? "";
            Console.Write($"  {CYAN}Senha (mín. 6 chars):{RESET} ");
            var senha = LerSenha();

            try
            {
                var u = _service.CadastrarUsuario(nome, email, senha);
                MensagemSucesso($"Conta criada! Bem-vindo, {u.Nome}!");
            }
            catch (CineLogException ex) { MensagemErro(ex.Message); }
        }

        private void MenuUsuario()
        {
            while (_usuarioLogado != null)
            {
                Console.WriteLine($"\n{BOLD}╔══ OLÁ, {_usuarioLogado.Nome.ToUpper()} ══════════════════╗{RESET}");
                Console.WriteLine($"  {CYAN}1.{RESET} 🎬 Explorar catálogo");
                Console.WriteLine($"  {CYAN}2.{RESET} ⭐ Minhas listas");
                Console.WriteLine($"  {CYAN}3.{RESET} ➕ Cadastrar mídia");
                Console.WriteLine($"  {CYAN}4.{RESET} 🏆 Top mídias");
                Console.WriteLine($"  {CYAN}5.{RESET} 💡 Recomendações para mim");
                Console.WriteLine($"  {RED}0.{RESET} Sair da conta");
                Console.WriteLine($"{BOLD}╚══════════════════════════════════════╝{RESET}");

                switch (LerOpcao())
                {
                    case "1": MenuCatalogo(); break;
                    case "2": MenuListas(); break;
                    case "3": MenuCadastrarMidia(); break;
                    case "4": ExibirTop(); break;
                    case "5": ExibirRecomendacoes(); break;
                    case "0": _usuarioLogado = null; MensagemSucesso("Sessão encerrada."); break;
                    default: MensagemErro("Opção inválida."); break;
                }
            }
        }

        private void MenuCatalogo()
        {
            Titulo("CATÁLOGO");
            Console.WriteLine($"  {CYAN}1.{RESET} Listar tudo  {CYAN}2.{RESET} Só filmes  {CYAN}3.{RESET} Só séries");
            Console.WriteLine($"  {CYAN}4.{RESET} Filtrar por gênero  {CYAN}5.{RESET} Filtrar por nota mínima");
            Console.WriteLine($"  {CYAN}6.{RESET} Buscar por ano");

            var midias = LerOpcao() switch
            {
                "1" => _service.ListarMidias(),
                "2" => _service.ListarFilmes(),
                "3" => _service.ListarSeries(),
                "4" => FiltrarPorGenero(),
                "5" => FiltrarPorNota(),
                "6" => FiltrarPorAno(),
                _ => null
            };

            if (midias == null) return;

            var lista = midias.ToList();
            if (!lista.Any()) { MensagemErro("Nenhuma mídia encontrada."); return; }

            ExibirListaMidias(lista);

            Console.Write($"\n  {DIM}Informe o ID para ver detalhes (ou ENTER para voltar):{RESET} ");
            if (int.TryParse(Console.ReadLine(), out int id))
                DetalhesMidia(id);
        }

        private void DetalhesMidia(int id)
        {
            try
            {
                var midia = _service.ObterMidia(id);
                Console.WriteLine($"\n{BOLD}{CYAN}  ═══ {midia.Titulo} ═══{RESET}");
                Console.WriteLine($"  Tipo: {midia.ObterTipo()} | Ano: {midia.AnoLancamento} | Gênero: {midia.Genero}");
                Console.WriteLine($"  {midia.ObterDetalhes()}");
                Console.WriteLine($"  Sinopse: {midia.Sinopse}");
                Console.WriteLine($"  Nota média: {YELLOW}{midia.NotaMedia:F1}/10{RESET} ({midia.Avaliacoes.Count} avaliações)");

                if (midia.Avaliacoes.Any())
                {
                    Console.WriteLine($"\n  {BOLD}Avaliações recentes:{RESET}");
                    foreach (var av in midia.Avaliacoes.TakeLast(3))
                        Console.WriteLine($"  {GREEN}•{RESET} {av}");
                }

                if (_usuarioLogado != null)
                {
                    Console.WriteLine($"\n  {CYAN}1.{RESET} Avaliar  {CYAN}2.{RESET} Adicionar à lista  {CYAN}ENTER.{RESET} Voltar");
                    switch (LerOpcao())
                    {
                        case "1": AvaliarMidia(midia); break;
                        case "2": AdicionarNaLista(midia.Id); break;
                    }
                }
            }
            catch (CineLogException ex) { MensagemErro(ex.Message); }
        }

        private void AvaliarMidia(Midia midia)
        {
            Console.Write($"  {CYAN}Nota (0-10):{RESET} ");
            if (!double.TryParse(Console.ReadLine()?.Replace(',', '.'),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double nota))
            { MensagemErro("Nota inválida."); return; }

            Console.Write($"  {CYAN}Comentário:{RESET} ");
            var comentario = Console.ReadLine()?.Trim() ?? "";

            try
            {
                _service.AvaliarMidia(_usuarioLogado!.Id, midia.Id, nota, comentario);
                MensagemSucesso($"Avaliação registrada! Nova média: {midia.NotaMedia:F1}/10");
            }
            catch (CineLogException ex) { MensagemErro(ex.Message); }
        }

        private void AdicionarNaLista(int midiaId)
        {
            Console.WriteLine($"  {CYAN}1.{RESET} Quero assistir  {CYAN}2.{RESET} Assistidos  {CYAN}3.{RESET} Favoritos");
            var lista = LerOpcao() switch
            {
                "1" => TipoLista.QueroAssistir,
                "2" => TipoLista.Assistidos,
                "3" => TipoLista.Favoritos,
                _ => (TipoLista?)null
            };
            if (lista == null) return;

            try
            {
                _service.AdicionarNaLista(_usuarioLogado!.Id, midiaId, lista.Value);
                MensagemSucesso("Adicionado à lista!");
            }
            catch (CineLogException ex) { MensagemErro(ex.Message); }
        }

        private void MenuListas()
        {
            Titulo("MINHAS LISTAS");
            Console.WriteLine($"  {CYAN}1.{RESET} 📋 Quero assistir");
            Console.WriteLine($"  {CYAN}2.{RESET} ✅ Assistidos");
            Console.WriteLine($"  {CYAN}3.{RESET} ❤️  Favoritos");

            TipoLista? tipo = LerOpcao() switch
            {
                "1" => TipoLista.QueroAssistir,
                "2" => TipoLista.Assistidos,
                "3" => TipoLista.Favoritos,
                _ => null
            };
            if (tipo == null) return;

            try
            {
                var midias = _service.ObterListaDoUsuario(_usuarioLogado!.Id, tipo.Value).ToList();
                if (!midias.Any()) { MensagemErro("Lista vazia."); return; }
                ExibirListaMidias(midias);
            }
            catch (CineLogException ex) { MensagemErro(ex.Message); }
        }

        private void MenuCadastrarMidia()
        {
            Titulo("CADASTRAR MÍDIA");
            Console.WriteLine($"  {CYAN}1.{RESET} Filme  {CYAN}2.{RESET} Série");
            switch (LerOpcao())
            {
                case "1": CadastrarFilme(); break;
                case "2": CadastrarSerie(); break;
            }
        }

        private void CadastrarFilme()
        {
            try
            {
                Console.Write($"  {CYAN}Título:{RESET} "); var titulo = Console.ReadLine()!.Trim();
                Console.Write($"  {CYAN}Ano:{RESET} "); var ano = int.Parse(Console.ReadLine()!);
                Console.Write($"  {CYAN}Gênero:{RESET} "); var genero = Console.ReadLine()!.Trim();
                Console.Write($"  {CYAN}Sinopse:{RESET} "); var sinopse = Console.ReadLine()!.Trim();
                Console.Write($"  {CYAN}Duração (min):{RESET} "); var duracao = int.Parse(Console.ReadLine()!);
                Console.Write($"  {CYAN}Diretor:{RESET} "); var diretor = Console.ReadLine()!.Trim();

                var f = _service.CadastrarFilme(titulo, ano, genero, sinopse, duracao, diretor);
                MensagemSucesso($"Filme '{f.Titulo}' cadastrado com ID {f.Id}!");
            }
            catch (Exception ex) { MensagemErro(ex.Message); }
        }

        private void CadastrarSerie()
        {
            try
            {
                Console.Write($"  {CYAN}Título:{RESET} "); var titulo = Console.ReadLine()!.Trim();
                Console.Write($"  {CYAN}Ano:{RESET} "); var ano = int.Parse(Console.ReadLine()!);
                Console.Write($"  {CYAN}Gênero:{RESET} "); var genero = Console.ReadLine()!.Trim();
                Console.Write($"  {CYAN}Sinopse:{RESET} "); var sinopse = Console.ReadLine()!.Trim();
                Console.Write($"  {CYAN}Temporadas:{RESET} "); var temp = int.Parse(Console.ReadLine()!);
                Console.Write($"  {CYAN}Episódios:{RESET} "); var ep = int.Parse(Console.ReadLine()!);
                Console.Write($"  {CYAN}Em andamento? (s/n):{RESET} ");
                var andamento = Console.ReadLine()?.Trim().ToLower() == "s";
                Console.Write($"  {CYAN}Criador:{RESET} "); var criador = Console.ReadLine()!.Trim();

                var s = _service.CadastrarSerie(titulo, ano, genero, sinopse, temp, ep, andamento, criador);
                MensagemSucesso($"Série '{s.Titulo}' cadastrada com ID {s.Id}!");
            }
            catch (Exception ex) { MensagemErro(ex.Message); }
        }

        private void ExibirTop()
        {
            Titulo("🏆 TOP MÍDIAS");
            var top = _service.ObterTopMidias(10).ToList();
            if (!top.Any()) { MensagemErro("Ainda sem avaliações."); return; }
            for (int i = 0; i < top.Count; i++)
            {
                var m = top[i];
                Console.WriteLine($"  {YELLOW}{i + 1,2}.{RESET} {m.Titulo,-35} {GREEN}{m.NotaMedia:F1}/10{RESET}  {DIM}({m.ObterTipo()}){RESET}");
            }
        }

        private void ExibirRecomendacoes()
        {
            Titulo("💡 RECOMENDAÇÕES PARA VOCÊ");
            var recs = _service.RecomendarPorGenero(_usuarioLogado!.Id).ToList();
            if (!recs.Any()) { Console.WriteLine("  Adicione mídias aos favoritos para receber recomendações personalizadas!"); return; }
            ExibirListaMidias(recs);
        }

        private void ExplorarCatalogo()
        {
            var midias = _service.ListarMidias().ToList();
            if (!midias.Any()) { MensagemErro("Catálogo vazio."); return; }
            Titulo("CATÁLOGO PÚBLICO");
            ExibirListaMidias(midias);
        }

        // ─── Helpers ──────────────────────────────────────────────────
        private void ExibirListaMidias(IEnumerable<Midia> midias)
        {
            Console.WriteLine($"\n  {DIM}{"ID",-4} {"Tipo",-8} {"Título",-35} {"Ano",-6} {"Nota",-5} Gênero{RESET}");
            Console.WriteLine($"  {DIM}{"──",4} {"────",8} {"─────",35} {"────",6} {"────",5} ──────{RESET}");
            foreach (var m in midias)
            {
                var nota = m.NotaMedia > 0 ? $"{m.NotaMedia:F1}" : "—";
                Console.WriteLine($"  {CYAN}{m.Id,-4}{RESET} {m.ObterTipo(),-8} {m.Titulo,-35} {m.AnoLancamento,-6} {nota,-5} {m.Genero}");
            }
        }

        private IEnumerable<Midia> FiltrarPorGenero()
        {
            Console.Write($"  {CYAN}Gênero:{RESET} ");
            return _service.FiltrarMidias(genero: Console.ReadLine()?.Trim());
        }

        private IEnumerable<Midia> FiltrarPorNota()
        {
            Console.Write($"  {CYAN}Nota mínima (0-10):{RESET} ");
            return double.TryParse(Console.ReadLine()?.Replace(',', '.'),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double n)
                ? _service.FiltrarMidias(notaMinima: n)
                : _service.ListarMidias();
        }

        private IEnumerable<Midia> FiltrarPorAno()
        {
            Console.Write($"  {CYAN}Ano:{RESET} ");
            return int.TryParse(Console.ReadLine(), out int a)
                ? _service.FiltrarMidias(ano: a)
                : _service.ListarMidias();
        }

        private static string LerOpcao()
        {
            Console.Write($"\n  {BOLD}Opção:{RESET} ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        private static string LerSenha()
        {
            var senha = "";
            ConsoleKeyInfo key;
            do
            {
                key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    senha += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && senha.Length > 0)
                {
                    senha = senha[..^1];
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            return senha;
        }

        private static void Titulo(string texto)
        {
            Console.WriteLine($"\n{BOLD}{CYAN}  ══ {texto} ══{RESET}");
        }

        private static void MensagemSucesso(string msg) =>
            Console.WriteLine($"\n  {GREEN}✓ {msg}{RESET}");

        private static void MensagemErro(string msg) =>
            Console.WriteLine($"\n  {RED}✗ {msg}{RESET}");
    }
}
