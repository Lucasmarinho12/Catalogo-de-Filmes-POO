using System.Collections.Generic;
using CineLog.Models;

namespace CineLog.Interfaces
{
    // Padrão Repository: abstrai a persistência. Hoje JSON; amanhã troca por
    // banco de dados (EF Core) sem alterar uma linha do CatalogoService.
    public interface IMidiaRepositorio
    {
        void Adicionar(Midia midia);
        Midia ObterPorId(int id);
        IEnumerable<Midia> ObterTodos();
        int ProximoId();
        void Salvar();
        void Carregar();
    }

    public interface IUsuarioRepositorio
    {
        void Adicionar(Usuario usuario);
        Usuario ObterPorId(int id);
        Usuario ObterPorEmail(string email);
        IEnumerable<Usuario> ObterTodos();
        bool EmailExiste(string email);
        int ProximoId();
        void Salvar();
        void Carregar();
    }

    public interface IAvaliacaoRepositorio
    {
        void Adicionar(Avaliacao avaliacao);
        IEnumerable<Avaliacao> ObterPorMidia(int midiaId);
        IEnumerable<Avaliacao> ObterTodos();
        int ProximoId();
        void Salvar();
        void Carregar();
    }
}
