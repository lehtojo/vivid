public type Player
{
    public tiny health
    public short mana
    public num level

    private num action_cost
    private num attack_damage

    public init ()
    {
        health = 100
        mana = 10000
        level = 1
        action_cost = 100
        attack_damage = 9
    }

    public func apply_damage(tiny damage) 
    {
        health -= damage
    }

    public func attack(Enemy enemy) 
    {
        if (get_actions_remaining() > 0) 
        {
            enemy.apply_damage(attack_damage)
        }
    }
    
    public func get_actions_remaining()
    {
        return mana / action_cost
    }

    public func add_health(tiny amount)
    {
        health += amount
    }

    public func add_mana(short amount)
    {
        mana += amount
    }

    public func is_alive ()
    {
        return health > 0
    }
}