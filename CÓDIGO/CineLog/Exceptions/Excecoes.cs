using System;

namespace CineLog.Exceptions
{
    // HERANÇA aplicada à hierarquia de exceções de domínio
    public class CineLogException : Exception
    {
        public CineLogException(string message) : base(message) { }
    }

    public class DadosInvalidosException : CineLogException
    {
        public DadosInvalidosException(string message) : base(message) { }
    }

    public class AvaliacaoDuplicadaException : CineLogException
    {
        public AvaliacaoDuplicadaException(string message) : base(message) { }
    }

    public class EntidadeNaoEncontradaException : CineLogException
    {
        public EntidadeNaoEncontradaException(string message) : base(message) { }
    }

    public class AutenticacaoException : CineLogException
    {
        public AutenticacaoException(string message) : base(message) { }
    }

    public class EmailDuplicadoException : CineLogException
    {
        public EmailDuplicadoException(string message) : base(message) { }
    }
}
