pack CustomList {
	memory: i32*
	size: i32
	position: i32

	shared new(size: i32): CustomList {
		return pack {
			memory: allocate(size * sizeof(i32)),
			size: size,
			position: 0
		}
	}

	add(element: i32): this {
		memory[position++] = element
	}

	get(i: i32): i32 {
		return memory[i]
	}

	destroy() {
		deallocate(memory)

		memory = 0
		size = 0
		position = 0
	}
}

pack Player {
	x: i32
	y: i32

	shared new(x: i32, y: i32): Player {
		return pack { x: x, y: y } as Player
	}

	move(dx: i32, dy: i32): this {
		x += dx
		y += dy
	}

	string(): String {
		return "(" + to_string(x) + ', ' + to_string(y) + `)`
	}
}

constant CUSTOM_LIST_SIZE_FACTOR_1 = 7
constant CUSTOM_LIST_SIZE_FACTOR_2_FACTOR_1 = 2
constant CUSTOM_LIST_SIZE = CUSTOM_LIST_SIZE_FACTOR_1 * CUSTOM_LIST_SIZE_FACTOR_2
constant CUSTOM_LIST_SIZE_FACTOR_2_FACTOR_2 = 3
constant CUSTOM_LIST_SIZE_FACTOR_2 = CUSTOM_LIST_SIZE_FACTOR_2_FACTOR_1 * CUSTOM_LIST_SIZE_FACTOR_2_FACTOR_2

export test_1() {
	list = CustomList.new(CUSTOM_LIST_SIZE)

	loop i in 0..(list.size - 1) {
		list.add(i * i)
	}

	loop i in 0..(list.size - 1) {
		console.write_line(list[i])
	}

	list.destroy()
}

export test_2(player: Player**) {
	player[][].move(7, -2)
}

init() {
	test_1()

	player: Player[1]
	player[] = Player.new(-3, 10)
	console.write_line(player[].string())

	pointer: link<Player>[1]
	pointer[] = player

	test_2(pointer)

	console.write_line(player[].string())
	return 0
}