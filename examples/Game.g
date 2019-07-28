func start(num args)
{
    Player player
    Enemy enemy

    num countdown = 10

    player = new Player()
    enemy = new Enemy()

    while (player.is_alive() & enemy.is_alive())
    {
        player.attack(enemy)
        enemy.attack(player)
    }
}