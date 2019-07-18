public func start (num args)
{
    var player
    var enemy

    var countdown = 10

    for (var i = countdown, i >= 1, i--) {

    }

    player = new Player()
    enemy = new Enemy()

    while (player.is_alive() && enemy.is_alive())
    {
        player.attack(enemy)
        enemy.attack(player)
    }
}