type Enemy 
{
    private tiny health
    private tiny defense
    private tiny attack_damage

    public init () 
    {
        health = 26
        defense = 2
        attack_damage = 5
    }

    public func apply_damage (tiny damage) 
    {
        health -= (damage - defense)
    }

    public func attack (Player player) 
    {
        player.apply_damage(attack_damage)
    }

    public bool is_alive () 
    {
        return health > 0
    }
}