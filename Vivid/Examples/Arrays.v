Player {
	health: num
	score: num
}

Player.kill() {
	health = 0
	score = -1
}

init() {
	box = Box(num, 10, 10, -10)

	a = Array(num, 10)
	b = Array(decimal, 7)

	a[9] = 10
	b[6] = 7.0
	b[-1] = 0.0

	players = Array(Player, 10)
	players[0] = Player()
	players[0].kill()
}