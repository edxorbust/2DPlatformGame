
public abstract class BaseState
{
    // Se ejecuta una sola vez al entrar al estado (ej: reproducir animación de ataque)
    public virtual void Enter() { }

    // Se ejecuta cada frame (ej: escuchar si el jugador presiona otro botón)
    public virtual void Update() { }

    // Se ejecuta en los frames de físicas (ej: mover al personaje durante un dash)
    public virtual void PhysicsUpdate() { }

    public virtual void AnimationFinishTrigger() { }

    // Se llamará en el fotograma exacto donde la espada debe hacer daño
    public virtual void AnimationImpactTrigger() { }

    public virtual void WindupFinishedTrigger() { }

    // Se ejecuta una sola vez justo antes de cambiar a otro estado (ej: apagar las cajas de daño de la espada)
    public virtual void Exit() { }
}
