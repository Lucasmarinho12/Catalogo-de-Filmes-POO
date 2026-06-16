namespace CineLog.Models
{
    // HERANÇA: Filme herda de Midia e adiciona atributos específicos
    public class Filme : Midia
    {
        public int DuracaoMinutos { get; private set; }
        public string Diretor { get; private set; }

        public Filme(int id, string titulo, int anoLancamento, string genero,
                     string sinopse, int duracaoMinutos, string diretor)
            : base(id, titulo, anoLancamento, genero, sinopse)
        {
            DuracaoMinutos = duracaoMinutos > 0 ? duracaoMinutos
                : throw new Exceptions.DadosInvalidosException("Duração deve ser maior que zero.");
            Diretor = string.IsNullOrWhiteSpace(diretor)
                ? throw new Exceptions.DadosInvalidosException("Diretor não pode ser vazio.")
                : diretor;
        }

        // POLIMORFISMO: override dos métodos abstratos
        public override string ObterTipo() => "Filme";

        public override string ObterDetalhes() =>
            $"Diretor: {Diretor} | Duração: {DuracaoMinutos} min | Nota: {NotaMedia:F1}";
    }
}
