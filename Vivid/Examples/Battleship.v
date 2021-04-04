import time(): large

BOARD_SIZE = 10
MIN_COMPUTER_SHIP_LENGTH = 2
MAX_COMPUTER_SHIP_LENGTH = 4
COLORS_START = 30

Vector {
	x: normal
	y: normal

	init(x, y) {
		this.x = x
		this.y = y
	}
}

Ship {
	start: Vector
	end: Vector
	health: normal

	init(start, end) {
		this.start = start
		this.end = end
	}

	# Returns the size of the specified ship
	size => abs(start.x - end.x) + abs(start.y - end.y) + 1
}

# Returns whether the specified position, which represent a bomb, hits any of the specified ships
hits(a, x, y) {
	board = Sheet<bool>(BOARD_SIZE, BOARD_SIZE)
	fill(board.data, BOARD_SIZE * BOARD_SIZE, 0)

	# Check whether the ship is a horizontal ship
	if a.start.x == a.end.x {
		
		# If the x-component misses entirely, the bomb can not hit
		if x != a.start.x => false

		min = min(a.start.y, a.end.y)
		max = max(a.start.y, a.end.y)

		loop (iy = min, iy <= max, iy++) {
			if iy == y => true
		}
	}
	else {
		# If the y-component misses entirely, the bomb can not hit
		if y != a.start.y => false

		min = min(a.start.x, a.end.x)
		max = max(a.start.x, a.end.x)

		loop (ix = min, ix <= max, ix++) {
			if ix == x => true
		}
	}

	=> false
}

# Returns whether the two specified ships collide
collides(a, b) {
	board = Sheet<bool>(BOARD_SIZE, BOARD_SIZE)
	fill(board.data, BOARD_SIZE * BOARD_SIZE, 0)

	if a.start.x == a.end.x {
		min = min(a.start.y, a.end.y)
		max = max(a.start.y, a.end.y)

		loop (y = min, y <= max, y++) {
			board[a.start.x, y] = true as bool
		}
	}
	else {
		min = min(a.start.x, a.end.x)
		max = max(a.start.x, a.end.x)

		loop (x = min, x <= max, x++) {
			board[x, a.start.y] = true as bool
		}
	}

	if (b.start.x == b.end.x) {
		min = min(b.start.y, b.end.y)
		max = max(b.start.y, b.end.y)

		loop (y = min, y <= max, y++) {
			if board[b.start.x, y] => true as bool
		}
	}
	else {
		min = min(b.start.x, b.end.x)
		max = max(b.start.x, b.end.x)

		loop (x = min, x <= max, x++) {
			if (board[x, b.start.y]) => true as bool
		}
	}

	=> false as bool
}

# Renders the specified board into the console
render(board) {
	print('    ')

	loop (i = 0, i < BOARD_SIZE, i++) {
		put(`A` + i)
		put(` `)
	}

	print('\n  ')

	loop (i = 0, i < BOARD_SIZE + 2, i++) {
		put(-2 as u8)
		put(` `)
	}

	println()

	loop (y = 0, y < BOARD_SIZE, y++) {
		put(`0` + y)
		put(` `)
		put(-2 as u8)

		loop (x = 0, x < BOARD_SIZE, x++) {
			put(` `)

			value = board[x, y]
			color = COLORS_START + value

			if value == -1 {
				put(`X`)
			}
			else value == -2 {
				print('\x1B[1;31m')
				put(`X`)
				print('\x1B[0m')
			}
			else value != 0 {
				print('\x1B[1;')
				print(color)
				put(`m`)
				put(-2 as u8)
				print('\x1B[0m')
			}
			else {
				put(` `)
			}
		}

		put(` `)
		put(-2 as u8)
		println()
	}

	print('  ')

	loop (i = 0, i < BOARD_SIZE + 2, i++) {
		put(-2 as u8)
		put(` `)
	}

	println()
}

# Prints a horizontal line
divider() {
	println()

	loop (i = 0, i < (BOARD_SIZE + 3) * 2, i++) {
		put(`-`)
	}

	println()
	println()
}

# Generates ships which use the same amount of resources as the user can
generate_ships() {
	resources = 16
	result = List<Ship>()

	println('Creating the ships...')

	s = time()

	loop (resources > 0) {
		delta = min(resources, random(MIN_COMPUTER_SHIP_LENGTH, MAX_COMPUTER_SHIP_LENGTH)) - 1
		ship = none as Ship

		if random(0, 2) >= 1 {
			# Vertical ship:
			x = random(0, BOARD_SIZE)
			y = random(0, BOARD_SIZE - delta)

			ship = Ship(Vector(x, y), Vector(x, y + delta))
		}
		else {
			# Horizontal ship:
			x = random(0, BOARD_SIZE - delta)
			y = random(0, BOARD_SIZE)

			ship = Ship(Vector(x, y), Vector(x + delta, y))
		}

		abort = false

		loop other in result {
			if collides(ship, other) {
				abort = true
				stop
			}
		}

		if abort continue

		resources -= delta + 1
		ship.health = delta + 1

		result.add(ship)
	}

	e = time()

	print('Time elapsed creating the ships: ')
	print(e - s)
	println('ms')

	=> result
}

# Asks for a position, inside the board, from the user
ask_for_position(message) {
	loop {
		print(message)
		location = readln()

		if location.length() != 2 {
			println('Please specify a valid position')
			continue
		}
		else {
			x = location[0] - `A`
			y = location[1] - `0`

			if x < 0 or x >= BOARD_SIZE or y < 0 or y >= BOARD_SIZE {
				println('The specified position is outside the board')
				continue
			}

			=> Vector(x, y)
		}
	}
}

# Renders the specified ships into the specified renderbuffer
render_ships(renderbuffer, ships) {
	color = 0

	loop ship in ships {
		color++

		if ship.start.x == ship.end.x {
			min = min(ship.start.y, ship.end.y)
			max = max(ship.start.y, ship.end.y)

			loop (y = min, y <= max, y++) {
				renderbuffer[ship.start.x, y] = color
			}
		}
		else {
			min = min(ship.start.x, ship.end.x)
			max = max(ship.start.x, ship.end.x)

			loop (x = min, x <= max, x++) {
				renderbuffer[x, ship.start.y] = color
			}
		}
	}
}

# Clears the console and the specified renderbuffer
clear(renderbuffer) {
	fill(renderbuffer.data, BOARD_SIZE * BOARD_SIZE, 0)
	print('\x1Bc')
}

# Clears the console
clear() {
	print('\x1Bc')
}

# Lets the player determine whether to put ships so that they consume the constant amount of resources
design(renderbuffer) {
	ships = List<Ship>()
	resources = 16

	clear(renderbuffer)
	print('Resources: ')
	println(resources)
	println()
	render_ships(renderbuffer, ships)
	render(renderbuffer)
	println()

	loop {
		print('Choose whether to create a ship, destroy a ship or start the game (C/D/S): ')
		input = readln()

		if input.length() != 1 {
			println('Please enter a valid command')
			continue
		}

		c = input[0]

		if c == `S` or c == `s` {
			if resources != 0 {
				println('Not enough resources left')
				continue
			}

			=> ships
		}
		else c == `C` or c == `c` {
			start = ask_for_position('Specify where the ship starts: ')
			end = ask_for_position('Specify where the ship ends: ')

			if start.x != end.x and start.y != end.y {
				println('Only horizontal and vertical ships are allowed')
				continue
			}

			ship = Ship(start, end)
			ship.health = ship.size

			if resources - ship.health < 0 {
				println('Not enough resources left')
				continue
			}

			resources -= ship.health
			ships.add(ship)

			clear(renderbuffer)
			print('Resources: ')
			println(resources)
			println()
			render_ships(renderbuffer, ships)
			render(renderbuffer)
			println()
		}
		else c == `D` or c == `d` {
			position = ask_for_position('Specify one position which is inside the ship: ')
			removed = false

			loop ship in ships {
				if hits(ship, position.x, position.y) {
					removed = true
					resources += ship.health
					ships.remove(ship)

					clear(renderbuffer)
					print('Resources: ')
					println(resources)
					println()
					render_ships(renderbuffer, ships)
					render(renderbuffer)
					println()
					stop
				}
			}

			if removed == false {
				println('There was no ship in the specified position')
			}
		}
		else {
			println('Please enter a valid command')
		}
	}

	=> ships
}

create_options() {
	positions = List<Vector>()

	loop (y = 0, y < BOARD_SIZE, y++) {
		loop (x = 0, x < BOARD_SIZE, x++) {
			positions.add(Vector(x, y))
		}
	}

	=> positions
}

# Bombs the specified position taking into account the specified ships
# Returns whether the shot hit
bomb(ships, renderbuffer, position) {
	color = -1

	loop ship in ships {
		if hits(ship, position.x, position.y) {
			color = -2

			if --ship.health <= 0 {
				ships.remove(ship)
			}

			stop
		}
	}

	renderbuffer[position.x, position.y] = color
	=> color == -2
}

# Decides the next shot position
decide_next_shot(hits: List<Vector>, options: List<Vector>) {
	if options.size() == 0 => Vector(0, 0)

	loop hit in hits {
		loop option in options {
			dx = abs(option.x - hit.x)
			dy = abs(option.y - hit.y)

			if (dx == 1 and dy == 0) or (dx == 0 and dy == 1) {
				options.remove(option)
				=> option
			}
		}
	}

	# Randomly choose the next shot position
	i = random(options.size())
	position = options[i]
	options.remove_at(i)

	=> position
}

init() {
	set_random_seed(time())

	player_renderbuffer = Sheet<tiny>(BOARD_SIZE, BOARD_SIZE)
	enemy_renderbuffer = Sheet<tiny>(BOARD_SIZE, BOARD_SIZE)

	fill(player_renderbuffer.data, BOARD_SIZE * BOARD_SIZE, 0)
	fill(enemy_renderbuffer.data, BOARD_SIZE * BOARD_SIZE, 0)

	player_ships = design(player_renderbuffer)
	enemy_ships = generate_ships()
	
	render_ships(player_renderbuffer, player_ships)

	clear()
	render(enemy_renderbuffer)
	divider()
	render(player_renderbuffer)

	player_options = create_options()
	enemy_options = create_options()

	enemy_hits = List<Vector>()

	println()

	loop {
		# Player bombs here
		position = ask_for_position('Specify the position of the next shot: ')
		valid = false
		i = 0

		loop (i < player_options.size(), i++) {
			option = player_options[i]

			# TODO: Operator overload
			if option.x == position.x and option.y == position.y {
				valid = true
				stop
			}
		}

		if valid == false {
			println('The specified position is already used!')
			continue
		}

		player_options.remove_at(i)
		bomb(enemy_ships, enemy_renderbuffer, position)

		if enemy_ships.size() == 0 {
			clear()
			render(enemy_renderbuffer)
			divider()
			render(player_renderbuffer)
			println()
			println('You win!')
			stop
		}

		# Computer bombs here
		position = decide_next_shot(enemy_hits, enemy_options)

		if bomb(player_ships, player_renderbuffer, position) {
			enemy_hits.add(position)
		}

		if player_ships.size() == 0 {
			clear()
			render(enemy_renderbuffer)
			divider()
			render(player_renderbuffer)
			println()
			println('You lose!')
			stop
		}

		clear()
		render(enemy_renderbuffer)
		divider()
		render(player_renderbuffer)
		println()
	}

	=> 0
}