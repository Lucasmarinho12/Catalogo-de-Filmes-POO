using System.Collections.Generic;
using CineLog.Models;

namespace CineLog.Interfaces
{
    // Padrão Repository — abstrai a camada de dados
    // Permite trocar JSON por SQLite/banco de dados sem alterar a lógica de negócio
    public interface IMidiaRepositorio
    {
        void Adicionar(Midia midia);
        Midia ObterPorId(int id);
        IEnumerable<Midia> ObterTodos();
        IEnumerable<Midia> FiltrarPorGenero(string genero);
        IEnumerable<Midia> FiltrarPorAno(int ano);
        IEnumerable<Midia> FiltrarPorNotaMinima(double notaMinima);
        void Salvar();
        void Carregar();
        int ProximoId();
    }

    public interface IUsuarioRepositorio
    {
        void Adicionar(Usuario usuario);
        Usuario ObterPorId(int id);
        Usuario ObterPorEmail(string email);
        IEnumerable<Usuario> ObterTodos();
        bool EmailExiste(string email);
        void Salvar();
        void Carregar();
        int ProximoId();
    }

    public interface IAvaliacaoRepositorio
    {
        void Adicionar(Avaliacao avaliacao);
        IEnumerable<Avaliacao> ObterPorMidia(int midiaId);
        IEnumerable<Avaliacao> ObterPorUsuario(int usuarioId);
        void Salvar();
        void Carregar();
        int ProximoId();
    }
}
