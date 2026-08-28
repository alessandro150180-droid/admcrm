namespace CucineCRM.Application.Common;

/// <summary>Credenziali non valide o utente non attivo.</summary>
public class AuthenticationException : Exception
{
    public AuthenticationException(string message) : base(message) { }
}

/// <summary>L'utente autenticato non ha i permessi per l'operazione richiesta.</summary>
public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Accesso negato: permessi insufficienti.")
        : base(message) { }
}

/// <summary>Entità non trovata (o non visibile nello scope dell'utente corrente).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} con chiave '{key}' non trovato.") { }
}

/// <summary>Errore di validazione applicativa (es. email duplicata).</summary>
public class ValidationAppException : Exception
{
    public ValidationAppException(string message) : base(message) { }
}
