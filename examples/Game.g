func print(num n) {

}

func start(num args) 
{
    Player player
    Enemy enemy

    num countdown = 10

    player = new Player
    tiny h = player.health
    tiny a = h->Player.level
    enemy = new Enemy

    Player players : 10
    num i = 0

    while (i < 10) 
    {
        players:i.health += 100
        i += 1
    }

    while (player.is_alive() & enemy.is_alive()) 
    {
        player.attack(enemy)
        enemy.attack(player)

        if (player.health > 0 & enemy.health > 0) {
            print(1)
        }
        else {
            print(0)
        }
    }
}