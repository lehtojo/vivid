MAP_SIZE = 10

Map {
	static map: Sheet<tiny>

	static default_fill(map: Sheet<tiny>, x: normal, y: normal) {
		if (x == 0 or y == 0 or x == MAP_SIZE - 1 or y == MAP_SIZE - 1) {
			map[x, y] = `#`
		}
		else {
			map[x, y] = `_`
		}

		=> 1 as tiny
	}

	static create_world(filler: (Sheet<tiny>, normal, normal) -> tiny) {
		loop y in 0..(MAP_SIZE-1) {
			loop x in 0..(MAP_SIZE-1) {
				filler(map, x as normal, y as normal)
			}
		}
	}
}

init() {
	Map.map = Sheet<tiny>(MAP_SIZE, MAP_SIZE)
	
	println('Haloo')
	Map.create_world(Map.default_fill)
	println('Haloo!')

	loop (y = 0, y < MAP_SIZE, y++) {
		loop (x = 0, x < MAP_SIZE, x++) {
			put(Map.map[x, y])
		}
		println()
	}

	=> 1
}

###
init() {
	map = Sheet<tiny>(MAP_SIZE, MAP_SIZE)
	loops(map, (x, y, map) -> {
		if (x == 0 or y == 0 or x == MAP_SIZE - 1 or y == MAP_SIZE - 1) {
			map[x, y] = `#`
		}
		else {
			map[x, y] = `_`
		}

		=> 1 as tiny
	})

	loop (y = 0, y < MAP_SIZE, y++) {
		loop (x = 0, x < MAP_SIZE, x++) {
			put(map[x, y])
		}
		println()
	}
}

loops(result: Sheet<tiny>, action: (normal, normal, Sheet<tiny>) -> tiny) {
	loop (y = 0, y < MAP_SIZE, y++) {
		loop (x = 0, x < MAP_SIZE, x++) {
			action(x as normal, y as normal, result)
		}
	}
}
###