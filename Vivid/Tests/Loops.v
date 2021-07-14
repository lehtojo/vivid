export loops(start: large, count: large) {
	result = start

	i = 0
	j = 0

	loop (i, i < count, ++i) {
		result += j
		j += 3
	}

	=> result
}

export forever_loop() {
	result = 0

	loop {
		++result
	}

	=> result
}

export conditional_loop(i: large) {
	loop (i < 10) {
		++i
	}

	=> i
}

export conditional_action_loop(i: large) {
	loop (i < 1000, i *= 2) {}
	=> i
}

export normal_for_loop(start: large, count: large) {
	result = start

	loop (i = 0, i < count, ++i) {
		result += i
	}

	=> result
}

export normal_for_loop_with_stop(start: large, count: large) {
	result = start

	loop (i = 0, i <= count, ++i) {
		
		if (i > 100) {
			result = -1
			stop
		}

		result += i
	}

	=> result
}

export normal_for_loop_with_continue(start: large, count: large) {
	result = start

	loop (i = 0, i < count, ++i) {
		
		if (i % 2 == 0) {
			result += 1
			continue
		}

		result += i
	}

	=> result
}

export nested_for_loops(memory: link, width: large) {
	w = 0

	loop (z = 0, z < width, ++z) {
		loop (y = 0, y < width, ++y) {
			if (y == 0) {
				++w
			}

			loop (x = 0, x < width, ++x) {
				if x % 2 == 0 and y % 2 == 0 and z % 2 == 0 {
					memory[z * width * width + y * width + x] = 100
				}
				else {
					memory[z * width * width + y * width + x] = 0
				}

				if (x == 0) {
					++w
				}
			}
		}

		if (z == 0) {
			++w
		}
	}

	=> w
}

import large_function()

export normal_for_loop_with_memory_evacuation(start: large, count: large) {
	loop (i = start, i < count, ++i) {
		large_function()
	}
}

init() {
	=> 1
}