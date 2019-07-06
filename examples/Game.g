public func start (num args)
{
    Player player = new Player()
    Enemy enemy = new Enemy()

    while (player.is_alive() && enemy.is_alive())
    {
        player.attack(enemy)
        enemy.attack(player)
    }
}