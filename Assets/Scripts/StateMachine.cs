
public class StateMachine
{
    // Variable para saber en qué estado estamos actualmente
    public BaseState CurrentState { get; private set; }

    // Arranca la máquina con un estado por defecto (ej: EstadoQuieto)
    public void Initialize(BaseState inicialState)
    {
        CurrentState = inicialState;
        CurrentState.Enter();
    }

    // Hace la transición limpia de un estado a otro
    public void ChangeState(BaseState newState)
    {
        // 1. Apagamos el estado anterior
        CurrentState.Exit();

        // 2. Cambiamos la referencia al nuevo estado
        CurrentState = newState;

        // 3. Encendemos el nuevo estado
        CurrentState.Enter();
    }
}