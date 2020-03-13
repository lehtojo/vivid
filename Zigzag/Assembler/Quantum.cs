using System.Collections.Generic;

public class Quantum<T> where T : class, new()
{
    public T Value { get; private set; }
    private List<Quantum<T>> Peers = new List<Quantum<T>>();
    
    public Quantum()
    {
        Value = new T();
    }

    public Quantum(T value)
    {
        Value = value;
    }

    private Quantum(T value, Quantum<T> creator, List<Quantum<T>> peers)
    {
        Value = value;
        Peers = peers;
        Peers.Add(creator);
    }

    public void Set(T value)
    {
        Value = value;
        Peers.ForEach(p => p.Value = value);
    }

    public Quantum<T> Entangle()
    {
        var peer = new Quantum<T>(Value, this, new List<Quantum<T>>(Peers));
        
        foreach (var p in Peers)
        {
            p.Peers.Add(peer);
        }

        Peers.Add(peer);

        return peer;
    }

    public void SetParent(Quantum<T> parent)
    {
        // Update current system
        Value = parent.Value;
        Peers.ForEach(p => p.Value = Value);

        // Add the new parent to the system
        Peers.ForEach(p => { p.Peers.AddRange(parent.Peers); p.Peers.Add(parent); });

        // Introduce this system to the parent and its peers
        parent.Peers.ForEach(p => { p.Peers.AddRange(Peers); p.Peers.Add(this); });
        
        parent.Peers.AddRange(Peers);
        parent.Peers.Add(this);

        Peers.AddRange(parent.Peers);
        Peers.Add(parent);
    }

    public void Entangle(Quantum<T> other)
    {  
        other.Set(Value);
        other.Peers.AddRange(Peers);
        other.Peers.Add(this);

        foreach (var peer in Peers)
        {
            peer.Peers.Add(other);
        }

        Peers.Add(other);
    }

    public override string ToString() 
    {
        return Value?.ToString() ?? "[?]";
    }
}