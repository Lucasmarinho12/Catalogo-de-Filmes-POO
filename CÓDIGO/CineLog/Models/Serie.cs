namespace CineLog.Models
{
    // HERANÇA: Serie herda de Midia com atributos próprios
    public class Serie : Midia
    {
        public int NumeroTemporadas { get; private set; }
        public int NumeroEpisodios { get; private set; }
        public bool EmAndamento { get; private set; }
        public string Criador { get; private set; }

        public Serie(int id, string titulo, int anoLancamento, string genero,
                     string sinopse, int numeroTemporadas, int numeroEpisodios,
                     bool emAndamento, string criador)
            : base(id, titulo, anoLancamento, genero, sinopse)
        {
            NumeroTemporadas = numeroTemporadas > 0 ? numeroTemporadas
                : throw new Exceptions.DadosInvalidosException("Número de temporadas deve ser maior que zero.");
            NumeroEpisodios = numeroEpisodios > 0 ? numeroEpisodios
                : throw new Exceptions.DadosInvalidosException("Número de episódios deve ser maior que zero.");
            EmAndamento = emAndamento;
            Criador = string.IsNullOrWhiteSpace(criador)
                ? throw new Exceptions.DadosInvalidosException("Criador não pode ser vazio.")
                : criador;
        }

        // POLIMORFISMO: implementação específica para Série
        public override string ObterTipo() => "Série";

        public override string ObterDetalhes() =>
            $"Criador: {Criador} | Temporadas: {NumeroTemporadas} | Ep: {NumeroEpisodios} | " +
            $"{(EmAndamento ? "Em andamento" : "Finalizada")} | Nota: {NotaMedia:F1}";
    }
}
